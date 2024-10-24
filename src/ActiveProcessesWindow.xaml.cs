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

            // Create a list to hold process info
            var processList = new List<ProcessInfo>();

            foreach (var process in processes)
            {
                try
                {
                    // Check if the process has a MainModule
                    if (process.MainModule != null)
                    {
                        // Log the process name for debugging
                        Console.WriteLine($"Found process: {process.ProcessName}");

                        var processInfo = new ProcessInfo
                        {
                            Name = Path.GetFileName(process.MainModule.FileName),
                            FileName = process.MainModule.FileName,
                            Icon = GetProcessIcon(process.MainModule.FileName)
                        };

                        // Check for specific game names if needed
                        if (processInfo.Name.Contains("cod", StringComparison.OrdinalIgnoreCase)) // Customize as needed
                        {
                            Console.WriteLine($"Found COD process: {processInfo.Name}");
                        }

                        processList.Add(processInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing process {process.ProcessName}: {ex.Message}");
                }
            }

            // Sort and display processes as before
            var sortedProcesses = processList
                .OrderBy(p => GetProcessPriority(p.FileName))
                .ThenBy(p => p.Name)
                .ToList();

            foreach (var processInfo in sortedProcesses)
            {
                ProcessesListView.Items.Add(processInfo);
            }
        }


        // Helper method to determine process priority
        private ProcessPriorityClass GetProcessPriority(string fileName)
        {
            try
            {
                using (var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(fileName)).FirstOrDefault())
                {
                    if (process != null)
                    {
                        return process.PriorityClass;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving priority for {fileName}: {ex.Message}");
            }

            // Default to Normal priority if unable to determine
            return ProcessPriorityClass.Normal;
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
