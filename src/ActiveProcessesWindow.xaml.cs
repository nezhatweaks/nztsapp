using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NZTS_App
{
    public partial class ActiveProcessesWindow : Window
    {
        private CPUPriorityControl mainControl;
        private List<ProcessInfo> allProcesses;

        public ActiveProcessesWindow(CPUPriorityControl control)
        {
            InitializeComponent();
            mainControl = control;
            allProcesses = new List<ProcessInfo>(); // Store all processes for searching
            LoadActiveProcesses();
        }

        private void LoadActiveProcesses()
        {
            // Clear existing items
            ProcessesListView.Items.Clear();
            allProcesses.Clear();

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
                        var processInfo = new ProcessInfo
                        {
                            Name = Path.GetFileName(process.MainModule.FileName),
                            FileName = process.MainModule.FileName,
                            Icon = GetProcessIcon(process.MainModule.FileName)
                        };

                        processList.Add(processInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing process {process.ProcessName}: {ex.Message}");
                }
            }

            // Save all processes for future filtering
            allProcesses = processList;

            // Display all processes initially
            FilterProcesses(); // Display all initially
        }

        // Helper method to filter and display processes based on search text
        private void FilterProcesses()
        {
            var searchQuery = SearchTextBox.Text.ToLower();
            var filteredProcesses = allProcesses
                .Where(p => p.Name.ToLower().Contains(searchQuery))
                .OrderBy(p => p.Name)
                .ToList();

            ProcessesListView.Items.Clear();
            foreach (var processInfo in filteredProcesses)
            {
                ProcessesListView.Items.Add(processInfo);
            }
        }

        // Event handler for the search box
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilterProcesses();
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
                return System.Drawing.Icon.ExtractAssociatedIcon(filePath);
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
                mainControl.AddGame(selectedProcess.Name);
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
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;

        private Icon? _icon;

        public BitmapSource? IconImage
        {
            get
            {
                if (_icon != null)
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        _icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                }
                return null;
            }
        }

        public Icon? Icon
        {
            get => _icon;
            set => _icon = value;
        }
    }
}
