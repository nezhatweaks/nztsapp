using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class NVIDIA : UserControl
    {
        private const string DynamicPstateKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
        private MainWindow mainWindow;

        public NVIDIA(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            // Temporarily detach event to avoid premature triggering during initialization
            DynamicPstateSwitch.Click -= DynamicPstateSwitch_Click;
            LoadCurrentDynamicPStateValue(); // Load the current value on initialization
            DynamicPstateSwitch.Click += DynamicPstateSwitch_Click; // Reattach event after loading
        }

        private void LoadCurrentDynamicPStateValue()
        {
            try
            {
                // Open the registry key
                using (var key = Registry.LocalMachine.OpenSubKey(DynamicPstateKeyPath))
                {
                    if (key != null)
                    {
                        // Retrieve the value of DisableDynamicPstate
                        var value = key.GetValue("DisableDynamicPstate");

                        // Ensure the value is correctly interpreted as an integer (DWORD)
                        if (value is int currentValue)
                        {
                            // Set the ToggleButton state based on the registry value (1 means disabled, 0 means enabled)
                            DynamicPstateSwitch.IsChecked = currentValue == 1;


                        }
                        else
                        {
                            // Log if the value is not an integer or null

                        }
                    }
                    else
                    {
                        // Log if the registry key is not found
                        MessageBox.Show("Failed to access DisableDynamicPstate registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading current value: {ex.Message}");
            }
        }

        private void DynamicPstateSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool isDynamicPstateDisabled = DynamicPstateSwitch.IsChecked == true;

            try
            {
                // Open the registry key for DisableDynamicPstate in writable mode
                using (var key = Registry.LocalMachine.OpenSubKey(DynamicPstateKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        // Set the registry value: 1 to disable, 0 to enable
                        key.SetValue("DisableDynamicPstate", isDynamicPstateDisabled ? 1 : 0, RegistryValueKind.DWord);

                        // Log and show success message
                        App.changelogUserControl?.AddLog("Applied", isDynamicPstateDisabled ? "Dynamic P-State disabled." : "Dynamic P-State enabled.");
                        mainWindow?.MarkSettingsApplied(); // Use the stored MainWindow reference;
                    }
                    else
                    {
                        // Log if the registry key is not found
                        string errorMsg = "Failed to access DisableDynamicPstate registry key.";
                        MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access permission errors
                string errorMsg = "You do not have permission to modify the registry. Please run the application as an administrator.";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                string errorMsg = $"Error updating registry: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            App.changelogUserControl?.AddLog("Failed", message);
        }
    }
}
