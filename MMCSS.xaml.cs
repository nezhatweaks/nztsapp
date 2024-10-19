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
        private MainWindow mainWindow;

        public MMCSS(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            LoadCurrentSettings();
        }

        private void MMCSSToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isMMCSSEnabled = MMCSSToggle.IsChecked == true;

            using (var key = Registry.LocalMachine.OpenSubKey(MMCSSKeyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("Start", isMMCSSEnabled ? 2 : 4, RegistryValueKind.DWord);
                    App.changelogUserControl?.AddLog("Applied", isMMCSSEnabled ? "MMCSS enabled." : "MMCSS disabled.");
                    
                }
                else
                {
                    string errorMsg = "Failed to access MMCSS service registry key.";
                    MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                }
            }
        }

        private void LoadCurrentSettings()
        {
            try
            {
                using (var mmcssKey = Registry.LocalMachine.OpenSubKey(MMCSSKeyPath))
                {
                    if (mmcssKey != null)
                    {
                        int mmcssState = (int)(mmcssKey.GetValue("Start", 2) ?? 2);
                        bool isMMCSSEnabled = mmcssState == 2;

                        MMCSSToggle.IsChecked = isMMCSSEnabled;

                        SystemResponsivenessInput.IsEnabled = isMMCSSEnabled;
                        LazyModeToggle.IsEnabled = isMMCSSEnabled;
                        LazyModeTimeoutInput.IsEnabled = isMMCSSEnabled;

                        
                    }
                    else
                    {
                        string errorMsg = "MMCSS registry key not found.";
                        MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }

                using (var systemProfileKey = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath))
                {
                    if (systemProfileKey != null)
                    {
                        int systemResponsiveness = (int)(systemProfileKey.GetValue("SystemResponsiveness", 20) ?? 20);
                        SystemResponsivenessInput.Text = systemResponsiveness.ToString();

                        int noLazyMode = (int)(systemProfileKey.GetValue("NoLazyMode", 0) ?? 0);
                        LazyModeToggle.IsChecked = noLazyMode == 0;

                        int lazyModeTimeout = (int)(systemProfileKey.GetValue("LazyModeTimeout", 5000) ?? 5000);
                        LazyModeTimeoutInput.Text = lazyModeTimeout.ToString();

                        
                    }
                    else
                    {
                        string errorMsg = "SystemProfile registry key not found.";
                        MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading settings: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        private void ApplySystemResponsiveness_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(SystemResponsivenessInput.Text, out int systemResponsiveness) && systemResponsiveness >= 0 && systemResponsiveness <= 100)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("SystemResponsiveness", systemResponsiveness, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", "SystemResponsiveness value applied.");
                        MessageBox.Show("SystemResponsiveness applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        string errorMsg = "Failed to access SystemProfile registry key.";
                        MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }
            }
            else
            {
                string errorMsg = "Please enter a valid value (0-100)";
                MessageBox.Show(errorMsg, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        private void LazyModeToggle_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = LazyModeToggle.IsChecked == true;
            SetLazyModeRegistryValue(isChecked ? 0 : 1);
            mainWindow?.MarkSettingsApplied();
        }

        private void SetLazyModeRegistryValue(int value)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("NoLazyMode", value, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", $"LazyMode set to {(value == 0 ? "Enabled" : "Disabled")}.");
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error setting NoLazyMode: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        private void ApplyLazyModeTimeout_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LazyModeTimeoutInput.Text, out int lazyModeTimeout) && lazyModeTimeout >= 0)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("LazyModeTimeout", lazyModeTimeout, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", "LazyModeTimeout value applied.");
                        MessageBox.Show("LazyModeTimeout applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        string errorMsg = "Failed to access SystemProfile registry key.";
                        MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.changelogUserControl?.AddLog("Failed", errorMsg);
                    }
                }
            }
            else
            {
                string errorMsg = "Please enter a valid timeout value in milliseconds";
                MessageBox.Show(errorMsg, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        private void ResetMMCSS_Click(object sender, RoutedEventArgs e)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(SystemProfileKeyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("SystemResponsiveness", 20, RegistryValueKind.DWord);
                    key.SetValue("NoLazyMode", 0, RegistryValueKind.DWord);
                    key.SetValue("LazyModeTimeout", 5000, RegistryValueKind.DWord);
                    App.changelogUserControl?.AddLog("Applied", "SystemProfile settings reset to default.");
                }
                else
                {
                    string errorMsg = "Failed to access SystemProfile registry key.";
                    MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                    return;
                }
            }

            using (var key = Registry.LocalMachine.OpenSubKey(MMCSSKeyPath, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("Start", 2, RegistryValueKind.DWord);
                    App.changelogUserControl?.AddLog("Applied", "MMCSS service Start value reset to 2 (enabled).");
                }
                else
                {
                    string errorMsg = "Failed to access MMCSS service registry key.";
                    MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                    return;
                }
            }

            MessageBox.Show("Settings reset to default!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadCurrentSettings();
        }
    }
}
