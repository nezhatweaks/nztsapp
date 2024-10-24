using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class MonitorUserControl : UserControl
    {
        private const string VsyncKeyPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler";
        private const string PowerKeyPath = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power";
        private const string DXGKrnlKeyPath = @"SYSTEM\CurrentControlSet\Services\DXGKrnl";
        private MainWindow mainWindow;

        public MonitorUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Monitor";

            // Temporarily detach event to avoid premature triggering during initialization
            VsyncIdleToggle.Click -= VsyncIdleToggle_Click;
            RefreshLatencyToggle.Click -= RefreshLatencyToggle_Click;
            LatencyToggle.Click -= LatencyToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            VsyncIdleToggle.Click += VsyncIdleToggle_Click;
            RefreshLatencyToggle.Click += RefreshLatencyToggle_Click;
            LatencyToggle.Click += LatencyToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load VsyncIdleTimeout
                using (var key = Registry.LocalMachine.OpenSubKey(VsyncKeyPath))
                {
                    if (key != null)
                    {
                        var vsyncValue = key.GetValue("VsyncIdleTimeout");
                        VsyncIdleToggle.IsChecked = (vsyncValue is int vsyncInt && vsyncInt == 0);
                    }
                    else
                    {
                        ShowError("Failed to access Vsync registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Vsync registry key not found.");
                    }
                }

                // Load MonitorRefreshLatencyTolerance
                using (var key = Registry.LocalMachine.OpenSubKey(PowerKeyPath))
                {
                    if (key != null)
                    {
                        var refreshLatencyValue = key.GetValue("MonitorRefreshLatencyTolerance");
                        RefreshLatencyToggle.IsChecked = (refreshLatencyValue is int refreshInt && refreshInt == 1);
                    }
                    else
                    {
                        ShowError("Failed to access Power registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Power registry key not found.");
                    }
                }

                // Load MonitorLatencyTolerance
                using (var key = Registry.LocalMachine.OpenSubKey(DXGKrnlKeyPath))
                {
                    if (key != null)
                    {
                        var latencyValue = key.GetValue("MonitorLatencyTolerance");
                        LatencyToggle.IsChecked = (latencyValue is int latencyInt && latencyInt == 1);
                    }
                    else
                    {
                        ShowError("Failed to access DXGKrnl registry key.");
                        App.changelogUserControl?.AddLog("Failed", "DXGKrnl registry key not found.");
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

        private void VsyncIdleToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(VsyncKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("VsyncIdleTimeout", VsyncIdleToggle.IsChecked == true ? 0 : 1, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"VsyncIdleTimeout set to {(VsyncIdleToggle.IsChecked == true ? "0" : "1")}");
                    }
                    else
                    {
                        ShowError("Failed to access Vsync registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Vsync registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify VsyncIdleTimeout.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating VsyncIdleTimeout: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating VsyncIdleTimeout: {ex.Message}");
            }
        }

        private void RefreshLatencyToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(PowerKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("MonitorRefreshLatencyTolerance", RefreshLatencyToggle.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"MonitorRefreshLatencyTolerance set to {(RefreshLatencyToggle.IsChecked == true ? "1" : "0")}");
                    }
                    else
                    {
                        ShowError("Failed to access Power registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Power registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify MonitorRefreshLatencyTolerance.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating MonitorRefreshLatencyTolerance: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating MonitorRefreshLatencyTolerance: {ex.Message}");
            }
        }

        private void LatencyToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(DXGKrnlKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("MonitorLatencyTolerance", LatencyToggle.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"MonitorLatencyTolerance set to {(LatencyToggle.IsChecked == true ? "1" : "0")}");
                    }
                    else
                    {
                        ShowError("Failed to access DXGKrnl registry key.");
                        App.changelogUserControl?.AddLog("Failed", "DXGKrnl registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify MonitorLatencyTolerance.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating MonitorLatencyTolerance: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating MonitorLatencyTolerance: {ex.Message}");
            }
        }


        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
