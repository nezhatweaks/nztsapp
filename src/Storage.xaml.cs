using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NZTS_App.Properties;

namespace NZTS_App.Views
{
    public partial class Storage : UserControl
    {
        private MainWindow mainWindow;
        private DispatcherTimer? smartTrimTimer;
        private const int TrimIntervalSeconds = 10;

        public Storage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Storage";

            // Load toggle states from settings
            AppDataTempToggle.IsChecked = Properties.Settings.Default.AppDataTempToggle;
            WindowsTempToggle.IsChecked = Properties.Settings.Default.WindowsTempToggle;

            
            InitializeSmartTrimTimer();
            SmartTrimManager.Instance.SetCpuUsageThreshold(50);
            SmartTrimToggle.IsChecked = SmartTrimManager.Instance.IsRunning;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SmartTrimToggle.IsChecked = SmartTrimManager.Instance.IsRunning;
        }

        private void InitializeSmartTrimTimer()
        {
            smartTrimTimer = new DispatcherTimer();
            smartTrimTimer.Interval = TimeSpan.FromSeconds(TrimIntervalSeconds);
            smartTrimTimer.Tick += SmartTrimTimer_Tick;
        }

        private float cpuUsageThreshold = 50;

        private void SmartTrimToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SmartTrimToggle.IsChecked == true)
            {
                SmartTrimManager.Instance.Start();
                App.changelogUserControl?.AddLog("Applied", "Smart Trim enabled.");
                Console.WriteLine("Smart Trim started.");
            }
            else
            {
                SmartTrimManager.Instance.Stop();
                App.changelogUserControl?.AddLog("Applied", "Smart Trim disabled.");
                Console.WriteLine("Smart Trim stopped.");
            }
        }

        

        private void SmartTrimTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        float cpuUsage = GetCpuUsage(process);
                        if (cpuUsage > cpuUsageThreshold)
                        {
                            if (process.PriorityClass != ProcessPriorityClass.High)
                            {
                                process.PriorityClass = ProcessPriorityClass.High;
                                App.changelogUserControl?.AddLog("Applied", $"Increased priority for {process.ProcessName}.");
                            }
                        }
                        else
                        {
                            if (process.PriorityClass != ProcessPriorityClass.Normal)
                            {
                                process.PriorityClass = ProcessPriorityClass.Normal;
                                App.changelogUserControl?.AddLog("Applied", $"Set priority for {process.ProcessName} to Normal.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.changelogUserControl?.AddLog("Failed", $"Error managing {process.ProcessName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error during Smart Trim: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error during Smart Trim: {ex.Message}");
            }
        }

        

        private void AppDataTempToggle_Click(object sender, RoutedEventArgs e)
        {
            
            SaveToggleStates(); // Save toggle states when changed
        }

        private void WindowsTempToggle_Click(object sender, RoutedEventArgs e)
        {
            
            SaveToggleStates(); // Save toggle states when changed
        }

        private void SaveToggleStates()
        {
            Properties.Settings.Default.AppDataTempToggle = AppDataTempToggle.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.WindowsTempToggle = WindowsTempToggle.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.Save();
        }

        private void DeleteTempFiles(string tempPath, string sourceName)
        {
            int totalFiles = 0;
            int successfullyDeleted = 0;
            int accessDenied = 0;

            try
            {
                // Get only .tmp files
                var tempFiles = Directory.GetFiles(tempPath, "*.tmp");
                totalFiles = tempFiles.Length;

                foreach (var file in tempFiles)
                {
                    try
                    {
                        File.Delete(file);
                        successfullyDeleted++;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        accessDenied++;
                    }
                    catch (Exception)
                    {
                        // Simply count the failed deletions without showing a specific message
                        accessDenied++;
                    }
                }

                // Display the summary
                string resultMessage = $"Source: {sourceName}\n" +
                                        $"Total .tmp files: {totalFiles}\n" +
                                        $"Successfully deleted: {successfullyDeleted}\n" +
                                        $"Access denied (or failed to delete): {accessDenied}";

                MessageBox.Show(resultMessage, "Deletion Summary", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing {tempPath}: {ex.Message}");
            }
        }

        // Update the DeleteTempFilesButton_Click method
        private void DeleteTempFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppDataTempToggle.IsChecked.GetValueOrDefault() && !WindowsTempToggle.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("Please toggle at least one option to delete temporary files.",
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected temporary files?",
                                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (AppDataTempToggle.IsChecked == true)
                {
                    DeleteTempFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"), "AppData Temp");
                }

                if (WindowsTempToggle.IsChecked == true)
                {
                    DeleteTempFiles(@"C:\Windows\Temp", "Windows Temp");
                }
            }

            
        }









        private float GetCpuUsage(Process process)
        {
            if (process == null || process.HasExited)
                return 0;

            using (var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName))
            {
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(100);
                return cpuCounter.NextValue();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
