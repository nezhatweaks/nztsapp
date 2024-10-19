using System;
using System.Diagnostics;
using System.Drawing; // Ensure you have the right reference to System.Drawing.Common
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NZTS_App
{
    public partial class ActiveProcessesWindow : Window
    {
        private CPUPriorityControl mainControl;

        public ActiveProcessesWindow(CPUPriorityControl control)
        {
            InitializeComponent();
            mainControl = control;
            LoadActiveProcesses();
        }

        private void LoadActiveProcesses()
        {
            // Clear existing items
            ProcessesListView.Items.Clear();

            // Get all processes
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // Check if the process has a MainModule and a MainWindowHandle
                    if (process.MainModule != null && process.MainWindowHandle != IntPtr.Zero)
                    {
                        var processInfo = new ProcessInfo
                        {
                            Name = Path.GetFileName(process.MainModule.FileName), // Get only the executable name
                            FileName = process.MainModule.FileName,
                            Icon = GetProcessIcon(process.MainModule.FileName) // Get the icon using the file name
                        };

                        // Add the process info to the ListView
                        ProcessesListView.Items.Add(processInfo);
                    }
                }
                catch (Exception ex)
                {
                    // Handle processes that can't be accessed (optional logging)
                    Console.WriteLine($"Error accessing process {process.ProcessName}: {ex.Message}");
                }
            }
        }

        // Method to get the icon from the executable file
        private Icon? GetProcessIcon(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null; // Ensure the file exists
            }

            try
            {
                // Extract the icon from the executable file
                return System.Drawing.Icon.ExtractAssociatedIcon(filePath); // Use the full namespace to avoid confusion
            }
            catch (Exception ex)
            {
                // Log or handle exceptions if necessary
                Console.WriteLine($"Error extracting icon from {filePath}: {ex.Message}");
                return null;
            }
        }

        private void AddSelectedProcessButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProcess = ProcessesListView.SelectedItem as ProcessInfo;
            if (selectedProcess != null)
            {
                // Use only the Name property for adding the game
                mainControl.AddGame(selectedProcess.Name); // Pass the application name, e.g., cs2.exe
                Close();
            }
            else
            {
                MessageBox.Show("Please select a process to add.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ProcessInfo
    {
        public string Name { get; set; } = string.Empty; // Initialize to avoid null warnings
        public string FileName { get; set; } = string.Empty; // Initialize to avoid null warnings

        private Icon? _icon; // Make _icon nullable

        public BitmapSource? IconImage // Make IconImage nullable
        {
            get
            {
                if (_icon != null)
                {
                    // Create BitmapSource from Icon
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        _icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                return null;
            }
        }

        public Icon? Icon // Make Icon nullable
        {
            get => _icon;
            set => _icon = value;
        }
    }
}
