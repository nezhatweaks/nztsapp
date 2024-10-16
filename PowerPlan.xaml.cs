using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App
{
    public partial class PowerPlan : UserControl
    {
        private string? selectedDirectory; // Nullable string
        

        public PowerPlan()
        {
            InitializeComponent();
            LoadPowerPlans(); // Load plans initially if you have a default directory set
            CheckPlatformAoAcOverride(); // Check the registry value on load
        }

        private void LoadPowerPlans()
        {
            if (!string.IsNullOrEmpty(selectedDirectory))
            {
                string[] powFiles = Directory.GetFiles(selectedDirectory, "*.pow");

                foreach (string file in powFiles)
                {
                    // Add the filename (without path) to the ListBox
                    PowerPlanListBox.Items.Add(Path.GetFileName(file));
                }
            }
        }

        private void CheckPlatformAoAcOverride()
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Control\Power";
            string registryValue = "PlatformAoAcOverride";

            // Open the registry key
            using var key = Registry.LocalMachine.OpenSubKey(registryPath);

            if (key != null) // Ensure the key is not null
            {
                // Use var to retrieve the value
                var value = key.GetValue(registryValue);

                // Check if the value is not null and is of type int
                if (value is int intValue) // Pattern matching to check the type
                {
                    // Set the toggle state based on the registry value
                    if (intValue == 1)
                    {
                        PlatformAoAcToggleButton.IsChecked = false; // Toggle off
                        BrowseButton.IsEnabled = false; // Disable the Browse button
                        
                    }
                    else if (intValue == 0)
                    {
                        PlatformAoAcToggleButton.IsChecked = true; // Toggle on
                        BrowseButton.IsEnabled = true; // Enable the Browse button
                        
                    }
                }
                else
                {
                    // Handle the case where the value is not found or is not an integer
                    MessageBox.Show("PlatformAoAcOverride value not found or is not an integer.");
                }
            }
            else
            {
                MessageBox.Show("Failed to open registry key.");
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Use OpenFileDialog to allow the user to select a .pow file
            var dialog = new OpenFileDialog
            {
                Filter = "Power Plan Files (*.pow)|*.pow",
                Title = "Select a Power Plan File",
                Multiselect = false // Prevent multiple selections
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                // Set the directory to the folder where the selected file is located
                selectedDirectory = Path.GetDirectoryName(dialog.FileName);
                PowerPlanListBox.Items.Clear(); // Clear previous items
                LoadPowerPlans(); // Load power plans from the selected directory
                PowerPlanListBox.SelectedItem = Path.GetFileName(dialog.FileName); // Select the chosen file
            }
        }

        private void PlatformAoAcToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Set the registry value to 0
            SetPlatformAoAcOverride(0);
            BrowseButton.IsEnabled = true; // Enable Browse button

            

            
        }


        private void PlatformAoAcToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Set the registry value to 1
            SetPlatformAoAcOverride(1);
            BrowseButton.IsEnabled = false; // Disable Browse button
            
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
            }
        }

        private void PromptRestart()
        {
            MessageBoxResult result = MessageBox.Show("Do you want to restart the computer now or later?",
                                                        "Restart Required",
                                                        MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                RestartComputer(); // Call the method to restart the computer
            }
            else if (result == MessageBoxResult.No)
            {
                // Logic for postponing the restart can go here
                
            }
            else if (result == MessageBoxResult.Cancel)
            {
                // Logic for cancelling the prompt, if needed
                
            }
        }

        private void RestartComputer()
        {
            // Command to restart the computer
            var processInfo = new System.Diagnostics.ProcessStartInfo("shutdown", "/r /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Start the process
            System.Diagnostics.Process.Start(processInfo);
        }


        private void PowerPlanListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if any item is selected
            ImportPowerPlanButton.IsEnabled = PowerPlanListBox.SelectedItem != null; // Enable or disable button
        }

        private void ImportPowerPlan_Click(object sender, RoutedEventArgs e)
        {
            // Check if the toggle is off (indicating PlatformAoAcOverride is set to 1)
            if (PlatformAoAcToggleButton.IsChecked == false)
            {
                MessageBox.Show("Please enable the Power Plan Import toggle to fully import a power plan.",
                                "Toggle Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return; // Exit the method early
            }

            if (PowerPlanListBox.SelectedItem != null)
            {
                string selectedFile = (string)PowerPlanListBox.SelectedItem;

                // Ensure selectedDirectory is not null before using it
                if (string.IsNullOrEmpty(selectedDirectory))
                {
                    MessageBox.Show("No directory selected. Please select a power plan file first.");
                    return;
                }

                string fullPath = Path.Combine(selectedDirectory, selectedFile);

                if (File.Exists(fullPath))
                {
                    // Logic to import the power plan
                    try
                    {
                        // Command to import the power plan using PowerShell
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
                            string output = string.Empty;
                            string error = string.Empty;

                            if (process != null)
                            {
                                try
                                {
                                    // Ensure the process has started and redirect the standard output/error
                                    if (process.StartInfo.RedirectStandardOutput && process.StartInfo.RedirectStandardError)
                                    {
                                        // Wait for the process to exit
                                        process.WaitForExit();

                                        // Read the output and error streams
                                        output = process.StandardOutput.ReadToEnd();
                                        error = process.StandardError.ReadToEnd();

                                        // Display output or handle errors
                                        if (string.IsNullOrEmpty(error))
                                        {
                                            MessageBox.Show($"Successfully imported {selectedFile}.");
                                        }
                                        else
                                        {
                                            MessageBox.Show($"Error importing {selectedFile}: {error}");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Standard output or error redirection is not enabled.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log the exception
                                    MessageBox.Show($"Error during process execution: {ex.Message}");
                                }
                            }
                            else
                            {
                                // Log an error if the process is null
                                MessageBox.Show("Error: The process is null and cannot be accessed.");
                            }

                            // Display output or handle errors
                            if (string.IsNullOrEmpty(error))
                            {
                                Console.WriteLine($"Successfully imported {selectedFile}.");
                            }
                            else
                            {
                                Console.WriteLine($"Error importing {selectedFile}: {error}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while importing the power plan: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"The selected file does not exist: {fullPath}");
                }
            }
            else
            {
                Console.WriteLine("Please select a power plan to import.");
            }
        }

    }
}
