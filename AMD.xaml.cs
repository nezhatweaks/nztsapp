using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace NZTS_App.Views
{
    public partial class AMD : UserControl
    {
        private const string AMDRegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
        private MainWindow mainWindow;

        public AMD(MainWindow window)
        {
            InitializeComponent();
            LoadCurrentSettings();
            mainWindow = window;
        }

        private void LoadCurrentSettings()
        {
            LoadRegistryValue("EnableUlps", EnableUlpsSwitch);
            LoadRegistryValue("PP_ThermalAutoThrottlingEnable", ThermalThrottlingSwitch);
        }

        private void LoadRegistryValue(string valueName, ToggleButton toggleButton)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AMDRegistryKeyPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue(valueName);
                        if (value is int currentValue)
                        {
                            toggleButton.IsChecked = currentValue == 0; // 0 means enabled
                        }
                    }
                    else
                    {
                        ShowError($"Failed to access {valueName} registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading {valueName} value: {ex.Message}");
            }
        }

        private void EnableUlpsSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("EnableUlps", EnableUlpsSwitch.IsChecked == true);
        }

        private void ThermalThrottlingSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("PP_ThermalAutoThrottlingEnable", ThermalThrottlingSwitch.IsChecked == true);
        }

        private void ToggleRegistryValue(string valueName, bool enable)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AMDRegistryKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, enable ? 0 : 1, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied(); // Use the stored MainWindow reference
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} has been set to {(enable ? "Disabled" : "Enabled")}");
                    }
                    else
                    {
                        ShowError($"Failed to access {valueName} registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating {valueName}: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
