using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;

namespace NZTS_App.Views
{
    public partial class SecurityUserControl : UserControl
    {
        private const string HypervisorKeyPath = @"SYSTEM\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";
        private MainWindow mainWindow;

        public SecurityUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Security";

            // Temporarily detach event to avoid premature triggering during initialization
            CoreIsolationToggle.Click -= CoreIsolationToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            CoreIsolationToggle.Click += CoreIsolationToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load HypervisorEnforcedCodeIntegrity Enabled value
                using (var key = Registry.LocalMachine.CreateSubKey(HypervisorKeyPath))
                {
                    if (key != null)
                    {
                        var enabledValue = key.GetValue("Enabled");
                        if (enabledValue == null) // Key does not exist
                        {
                            key.SetValue("Enabled", 0, RegistryValueKind.DWord); // Default value
                            CoreIsolationToggle.IsChecked = false; // Default state
                        }
                        else
                        {
                            CoreIsolationToggle.IsChecked = (enabledValue is int enabledInt && enabledInt == 1);
                        }
                    }
                    else
                    {
                        ShowError("Failed to access Hypervisor registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Hypervisor registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry key. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to registry.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error loading settings: {ex.Message}");
            }
        }

        private void CoreIsolationToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(HypervisorKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("Enabled", CoreIsolationToggle.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"Core Isolation set to {(CoreIsolationToggle.IsChecked == true ? "Enabled" : "Disabled")}");
                    }
                    else
                    {
                        ShowError("Failed to access Hypervisor registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Hypervisor registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Core Isolation.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Core Isolation: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Core Isolation: {ex.Message}");
            }
        }

        private void DefButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show a warning message before proceeding with the execution
                var result = MessageBox.Show(
                    "This operation will make irreversible changes to your system. It is highly recommended to create a system restore point before proceeding. Do you want to continue?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return; // Exit if the user chooses not to proceed
                }

                // Name of the embedded resource (usually the namespace + file name)
                string resourceName = "NZTS_App.removedefend.bat";  // Adjust this to match your namespace and file name

                // Get the current executing assembly
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                // Read the resource file as a stream
                using (Stream? resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        MessageBox.Show("Batch file not found in resources.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Create a temporary file to store the batch content
                    string tempBatchFilePath = Path.Combine(Path.GetTempPath(), "removedefend.bat");

                    // Write the content of the resource to the temporary file
                    using (FileStream fileStream = new FileStream(tempBatchFilePath, FileMode.Create, FileAccess.Write))
                    {
                        // Safely copy stream content, null check is already done above
                        resourceStream.CopyTo(fileStream);
                    }

                    // Make sure the file is created successfully
                    if (File.Exists(tempBatchFilePath))
                    {
                        // Start the batch file
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = tempBatchFilePath,
                            UseShellExecute = true,  // Run using the shell
                            CreateNoWindow = false  // Show command prompt window
                        };

                        // Start the batch file process
                        Process? process = Process.Start(processInfo);

                        // Ensure the process started successfully
                        if (process != null)
                        {
                            // Wait for the process to exit (i.e., for the batch file to finish running)
                            process.WaitForExit();

                            // Log success and notify the user after the batch file has finished
                            mainWindow?.MarkSettingsApplied();
                            App.changelogUserControl?.AddLog("Applied", "Security batch file executed successfully.");
                            MessageBox.Show("Security batch file executed successfully. Please restart your PC to complete the changes.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to start the batch file process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to create temporary batch file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                string errorMsg = $"An error occurred while executing the batch file: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }




        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
