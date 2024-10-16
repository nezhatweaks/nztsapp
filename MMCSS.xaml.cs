using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class MMCSS : UserControl
    {
        private const string MMCSSKeyPath = @"SYSTEM\CurrentControlSet\Services\MMCSS";
        private const string SystemProfileKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        private MainWindow mainWindow; // Field to store reference to MainWindow

        // Constructor that accepts a MainWindow reference
        public MMCSS(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window; // Store the reference
            LoadCurrentSettings();
        }

        private void MMCSSToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isMMCSSEnabled = MMCSSToggle.IsChecked == true;

            using (var key = Registry.LocalMachine.OpenSubKey(MMCSSServiceKeyPath, writable: true))
            {
                if (key != null)
                {
                    // Set Start value to 2 if enabled, 4 if disabled
                    key.SetValue("Start", isMMCSSEnabled ? 2 : 4, RegistryValueKind.DWord);

                    // Notify user about the change
                    string message = isMMCSSEnabled ? "MMCSS enabled." : "MMCSS disabled.";
                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to access MMCSS service registry key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // Load current values from registry
        private void LoadCurrentSettings()
        {
            try
            {
                // Load MMCSS state to enable/disable UI
                using (var mmcssKey = Registry.LocalMachine.OpenSubKey(MMCSSKeyPath))
                {
                    if (mmcssKey != null)
                    {
                        int mmcssState = (int)(mmcssKey.GetValue("Start", 2) ?? 2); // Default to 2 (enabled)
                        bool isMMCSSEnabled = mmcssState == 2;

                        // Update toggle to reflect the MMCSS service state
                        MMCSSToggle.IsChecked = isMMCSSEnabled;

                        // Enable/disable input fields based on MMCSS state
                        SystemResponsivenessInput.IsEnabled = isMMCSSEnabled;
                        LazyModeToggle.IsEnabled = isMMCSSEnabled;
                        LazyModeTimeoutInput.IsEnabled = isMMCSSEnabled;
                    }
                    else
                    {
                        MessageBox.Show("MMCSS registry key not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Load SystemResponsiveness value
                using (var systemProfileKey = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath))
                {
                    if (systemProfileKey != null)
                    {
                        int systemResponsiveness = (int)(systemProfileKey.GetValue("SystemResponsiveness", 20) ?? 20); // Default to 20
                        SystemResponsivenessInput.Text = systemResponsiveness.ToString();

                        // Load NoLazyMode value
                        int noLazyMode = (int)(systemProfileKey.GetValue("NoLazyMode", 0) ?? 0); // Default to 0
                        LazyModeToggle.IsChecked = noLazyMode == 0;

                        // Load LazyModeTimeout value
                        int lazyModeTimeout = (int)(systemProfileKey.GetValue("LazyModeTimeout", 5000) ?? 5000); // Default to 5000 ms
                        LazyModeTimeoutInput.Text = lazyModeTimeout.ToString();
                    }
                    else
                    {
                        MessageBox.Show("SystemProfile registry key not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");
            }
        }




        // Apply SystemResponsiveness
        private void ApplySystemResponsiveness_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SystemResponsivenessInput.Text, out int systemResponsiveness) && systemResponsiveness >= 0 && systemResponsiveness <= 100)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemResponsiveness", systemResponsiveness, RegistryValueKind.DWord);
                        MessageBox.Show("SystemResponsiveness applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow?.MarkSettingsApplied(); // Mark settings as applied
                    }
                    else
                    {
                        MessageBox.Show("Failed to access SystemProfile registry key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid value (0-100)", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LazyModeToggle_Click(object sender, RoutedEventArgs e)
        {
            // Get the current state of the toggle
            bool isChecked = LazyModeToggle.IsChecked == true;

            // Update the registry value based on the toggle state
            SetLazyModeRegistryValue(isChecked ? 0 : 1); // 0 means enabled (LazyMode ON), 1 means disabled (LazyMode OFF)
            mainWindow?.MarkSettingsApplied(); // Mark settings as applied
        }

        // Method to set the LazyMode registry value
        private void SetLazyModeRegistryValue(int value)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("NoLazyMode", value, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting NoLazyMode: {ex.Message}");
            }
        }

        // Apply LazyModeTimeout
        private void ApplyLazyModeTimeout_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LazyModeTimeoutInput.Text, out int lazyModeTimeout) && lazyModeTimeout >= 0)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("LazyModeTimeout", lazyModeTimeout, RegistryValueKind.DWord);
                        MessageBox.Show("LazyModeTimeout applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow?.MarkSettingsApplied(); // Mark settings as applied
                    }
                    else
                    {
                        MessageBox.Show("Failed to access SystemProfile registry key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid timeout value in milliseconds", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private const string MMCSSServiceKeyPath = @"SYSTEM\CurrentControlSet\Services\MMCSS";

        private void ResetMMCSS_Click(object sender, RoutedEventArgs e)
        {
            // Reset MMCSS settings
            using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("SystemResponsiveness", 20, RegistryValueKind.DWord); // Reset to default 20
                    key.SetValue("NoLazyMode", 0, RegistryValueKind.DWord); // Reset to default 0
                    key.SetValue("LazyModeTimeout", 5000, RegistryValueKind.DWord); // Reset to default 5000 ms
                }
                else
                {
                    MessageBox.Show("Failed to access SystemProfile registry key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Reset MMCSS service Start value to 2 (enabled)
            using (var key = Registry.LocalMachine.OpenSubKey(MMCSSServiceKeyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("Start", 2, RegistryValueKind.DWord); // Enable MMCSS service by setting Start to 2
                }
                else
                {
                    MessageBox.Show("Failed to access MMCSS service registry key.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            MessageBox.Show("Settings reset to default!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload current settings
            LoadCurrentSettings();
        }
    }
}
