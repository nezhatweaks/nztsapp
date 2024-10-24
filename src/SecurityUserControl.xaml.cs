using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

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

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
