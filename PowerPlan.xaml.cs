using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;


namespace NZTS_App
{

    public class PowerPlanInfo
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }


    public partial class PowerPlan : UserControl
    {
        private string? selectedDirectory;

        private MainWindow mainWindow;
        public PowerPlan(MainWindow window)
        {
            InitializeComponent();
            LoadPowerPlans();
            CheckPlatformAoAcOverride();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Power Plan";
        }

        private void LoadPowerPlans()
        {
            if (!string.IsNullOrEmpty(selectedDirectory))
            {
                string[] powFiles = Directory.GetFiles(selectedDirectory, "*.pow");

                foreach (string file in powFiles)
                {
                    PowerPlanListBox.Items.Add(Path.GetFileName(file));
                }
                App.changelogUserControl?.AddLog("Applied", "Loaded power plans from directory.");
            }
        }

        private void CheckPlatformAoAcOverride()
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Control\Power";
            string registryValue = "PlatformAoAcOverride";

            using var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true);
            if (key != null)
            {
                var value = key.GetValue(registryValue);

                if (value is int intValue)
                {
                    PlatformAoAcToggleButton.IsChecked = intValue == 0;
                    BrowseButton.IsEnabled = intValue == 0;
                }
                else
                {
                    // Value is null or not an int, create it with a default value
                    key.SetValue(registryValue, 0, RegistryValueKind.DWord);
                    PlatformAoAcToggleButton.IsChecked = true; // Set toggle based on the new value
                    BrowseButton.IsEnabled = true; // Enable the button
                    string infoMsg = "PlatformAoAcOverride value was not found and has been created with a default value of 0.";
                    MessageBox.Show(infoMsg);
                    App.changelogUserControl?.AddLog("Info", infoMsg);
                }
            }
            else
            {
                string errorMsg = "Failed to open registry key.";
                MessageBox.Show(errorMsg);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }


        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Power Plan Files (*.pow)|*.pow",
                Title = "Select a Power Plan File",
                Multiselect = false
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                selectedDirectory = Path.GetDirectoryName(dialog.FileName);
                PowerPlanListBox.Items.Clear();
                LoadPowerPlans();
                PowerPlanListBox.SelectedItem = Path.GetFileName(dialog.FileName);
                
            }
        }

        private void PlatformAoAcToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SetPlatformAoAcOverride(0);
            BrowseButton.IsEnabled = true;
            ImportPresetPlanButton.IsEnabled = true; // Enable the preset import button
        }

        private void PlatformAoAcToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPlatformAoAcOverride(1);
            BrowseButton.IsEnabled = false;
            ImportPresetPlanButton.IsEnabled = false; // Disable the preset import button
        }


        private void SetPlatformAoAcOverride(int value)
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Control\Power";
            using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("PlatformAoAcOverride", value, RegistryValueKind.DWord);
                    
                }
                else
                {
                    string errorMsg = "Failed to open registry key for PlatformAoAcOverride.";
                    MessageBox.Show(errorMsg);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                }
            }
        }

        private void PromptRestart()
        {
            MessageBoxResult result = MessageBox.Show("Do you want to restart the computer now or later?", "Restart Required", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                RestartComputer();
            }
        }

        private void RestartComputer()
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("shutdown", "/r /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            System.Diagnostics.Process.Start(processInfo);
            App.changelogUserControl?.AddLog("Applied", "Initiated system restart.");
        }

        private void PowerPlanListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImportPowerPlanButton.IsEnabled = PowerPlanListBox.SelectedItem != null;
        }

        private void OpenPowerOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "control.exe",
                    Arguments = "powercfg.cpl",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void ImportPowerPlan_Click(object sender, RoutedEventArgs e)
        {
            if (PlatformAoAcToggleButton.IsChecked == false)
            {
                string errorMsg = "Please enable the Power Plan Import toggle to fully import a power plan.";
                MessageBox.Show(errorMsg, "Toggle Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
                return;
            }

            // Check if PowerPlanListBox is not null and has a selected item
            if (PowerPlanListBox != null && PowerPlanListBox.SelectedItem is string selectedFile)
            {
                if (string.IsNullOrEmpty(selectedDirectory))
                {
                    string errorMsg = "No directory selected. Please select a power plan file first.";
                    MessageBox.Show(errorMsg);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                    return;
                }

                string fullPath = Path.Combine(selectedDirectory, selectedFile);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        string command = $"powercfg -import \"{fullPath}\"";

                        var processInfo = new System.Diagnostics.ProcessStartInfo("powershell", command)
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = System.Diagnostics.Process.Start(processInfo))
                        {
                            if (process != null)
                            {
                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();

                                process.WaitForExit();

                                if (string.IsNullOrEmpty(error))
                                {
                                    MessageBox.Show($"Successfully imported {selectedFile}.");
                                    App.changelogUserControl?.AddLog("Applied", $"Successfully imported {selectedFile}.");
                                }
                                else
                                {
                                    string errorMsg = $"Error importing {selectedFile}: {error}";
                                    MessageBox.Show(errorMsg);
                                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                                }
                            }
                            else
                            {
                                string errorMsg = "Error: The process is null and cannot be accessed.";
                                MessageBox.Show(errorMsg);
                                App.changelogUserControl?.AddLog("Failed", errorMsg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"An error occurred while importing the power plan: {ex.Message}";
                        MessageBox.Show(errorMsg);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }
                else
                {
                    string errorMsg = $"The selected file does not exist: {fullPath}";
                    MessageBox.Show(errorMsg);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                }
            }
            else
            {
                string errorMsg = "Please select a power plan to import.";
                MessageBox.Show(errorMsg);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }
        private void ImportPresetPlan_Click(object sender, RoutedEventArgs e)
        {
            // Construct the full path for the preset directory
            string presetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");

            // Check if the preset directory exists
            if (Directory.Exists(presetDirectory))
            {
                // Get all .pow files in the directory
                string[] presetFiles = Directory.GetFiles(presetDirectory, "*.pow");

                if (presetFiles.Length > 0)
                {
                    foreach (string presetFile in presetFiles)
                    {
                        string fileName = Path.GetFileName(presetFile);
                        try
                        {
                            // Ensure the full path is correctly formatted and quoted
                            string command = $"\"{presetFile}\"";  // Just the path to the file

                            var processInfo = new System.Diagnostics.ProcessStartInfo("powercfg", $"-import {command}")
                            {
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (var process = System.Diagnostics.Process.Start(processInfo))
                            {
                                if (process != null)
                                {
                                    string output = process.StandardOutput.ReadToEnd();
                                    string error = process.StandardError.ReadToEnd();

                                    process.WaitForExit();

                                    if (string.IsNullOrEmpty(error))
                                    {
                                        MessageBox.Show($"Successfully imported preset {fileName}.");
                                        App.changelogUserControl?.AddLog("Applied", $"Successfully imported preset {fileName}.");
                                    }
                                    else
                                    {
                                        string errorMsg = $"Error importing preset {fileName}: {error}";
                                        MessageBox.Show(errorMsg);
                                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                                    }
                                }
                                else
                                {
                                    string errorMsg = "Error: The process is null and cannot be accessed.";
                                    MessageBox.Show(errorMsg);
                                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"An error occurred while importing the preset power plan {fileName}: {ex.Message}";
                            MessageBox.Show(errorMsg);
                            App.changelogUserControl?.AddLog("Failed", errorMsg);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No preset power plans found in the directory.");
                    App.changelogUserControl?.AddLog("Info", "No preset power plans found in the directory.");
                }
            }
            else
            {
                string errorMsg = "Preset directory does not exist.";
                MessageBox.Show(errorMsg);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        










    }
}
