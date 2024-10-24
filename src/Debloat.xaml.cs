using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class Debloat : UserControl
    {
        private MainWindow mainWindow;

        public Debloat(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Telemetry";

            // Temporarily detach events to avoid premature triggering during initialization
            WsqmconsToggle.Click -= WsqmconsToggle_Click;
            CompattelToggle.Click -= CompattelToggle_Click;
            ElevationServiceToggle.Click -= ElevationServiceToggle_Click;
            DeviceCensusToggle.Click -= DeviceCensusToggle_Click;
            GameBarPresenceToggle.Click -= GameBarPresenceToggle_Click;
            MusNotificationToggle.Click -= MusNotificationToggle_Click;
            WwahostToggle.Click -= WwahostToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            WsqmconsToggle.Click += WsqmconsToggle_Click;
            CompattelToggle.Click += CompattelToggle_Click;
            ElevationServiceToggle.Click += ElevationServiceToggle_Click;
            DeviceCensusToggle.Click += DeviceCensusToggle_Click;
            GameBarPresenceToggle.Click += GameBarPresenceToggle_Click;
            MusNotificationToggle.Click += MusNotificationToggle_Click;
            WwahostToggle.Click += WwahostToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                WsqmconsToggle.IsChecked = CheckProcessEnabled("wsqmcons.exe");
                CompattelToggle.IsChecked = CheckProcessEnabled("compattelrunner.exe");
                ElevationServiceToggle.IsChecked = CheckProcessEnabled("elevation_service.exe");
                DeviceCensusToggle.IsChecked = CheckProcessEnabled("devicecensus.exe");
                GameBarPresenceToggle.IsChecked = CheckProcessEnabled("gamebarpresencewriter.exe");
                MusNotificationToggle.IsChecked = CheckProcessEnabled("MusNotification.exe");
                WwahostToggle.IsChecked = CheckProcessEnabled("wwahost.exe");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error loading settings: {ex.Message}");
            }
        }

        private bool CheckProcessEnabled(string processName)
        {
            string registryKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{processName}";

            using (var key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                // If the registry key exists, check for the debugger
                if (key != null)
                {
                    var debuggerValue = key.GetValue("Debugger");
                    return debuggerValue != null; // Process is enabled if a debugger is set
                }
            }

            return false; // Process is disabled if the registry key doesn't exist
        }






        private void WsqmconsToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("wsqmcons.exe", WsqmconsToggle.IsChecked == true);
        }

        private void CompattelToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("compattelrunner.exe", CompattelToggle.IsChecked == true);
        }

        private void ElevationServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("elevation_service.exe", ElevationServiceToggle.IsChecked == true);
        }

        private void DeviceCensusToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("devicecensus.exe", DeviceCensusToggle.IsChecked == true);
        }

        private void GameBarPresenceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("gamebarpresencewriter.exe", GameBarPresenceToggle.IsChecked == true);
        }

        private void MusNotificationToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("MusNotification.exe", MusNotificationToggle.IsChecked == true);
        }

        private void WwahostToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("wwahost.exe", WwahostToggle.IsChecked == true);
        }

        private void ToggleProcess(string processName, bool isEnabled)
        {
            string registryKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{processName}";

            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(registryKeyPath))
                {
                    if (isEnabled) // When checked, add the debugger to disable the process
                    {
                        key?.SetValue("Debugger", @"C:\Windows\System32\systray.exe");
                        App.changelogUserControl?.AddLog("Applied", $"{processName} is now disabled.");
                        mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
                    }
                    else // When unchecked, remove the debugger to enable the process
                    {
                        if (key != null)
                        {
                            key.DeleteValue("Debugger", false);
                            App.changelogUserControl?.AddLog("Applied", $"{processName} is now enabled.");
                            mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access to modify {processName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error modifying {processName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error modifying {processName}: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
