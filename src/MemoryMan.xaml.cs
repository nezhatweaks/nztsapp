using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NZTS_App.Views
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? Brushes.Green : Brushes.Red; // Adjust colors as needed
            }
            return Brushes.Transparent; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MemoryMan : UserControl
    {
        private const string MemoryKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
        private MainWindow mainWindow;

        public MemoryMan(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            LoadCurrentSettings();
            mainWindow.TitleTextBlock.Content = "Memory";

            DisablePagingExecutiveToggle.Click += DisablePagingExecutiveToggle_Click;
            ContextSwitchDeadbandToggle.Click += ContextSwitchDeadbandToggle_Click;
            LatencySensitivityHintToggle.Click += LatencySensitivityHintToggle_Click;
            DisableHeapCoalesceOnFreeToggle.Click += DisableHeapCoalesceOnFreeToggle_Click; 
            LargePageMinimumToggle.Click += LargePageMinimumToggle_Click; 
            SecondLevelDataCacheToggle.Click += SecondLevelDataCacheToggle_Click;
            ThirdLevelDataCacheToggle.Click += ThirdLevelDataCacheToggle_Click;
            DisableOSMitigationsToggle.Click += DisableOSMitigationsToggle_Click;
            SystemCacheDirtyPageThresholdToggle.Click += SystemCacheDirtyPageThresholdToggle_Click;
            LargePageSizeInBytesToggle.Click += LargePageSizeInBytesToggle_Click;
            LockPagesInMemoryToggle.Click += LockPagesInMemoryToggle_Click;
            LargePageHeapSizeThresholdToggle.Click += LargePageHeapSizeThresholdToggle_Click;
            UseBiasedLockingToggle.Click += UseBiasedLockingToggle_Click;
            TieredCompilationToggle.Click += TieredCompilationToggle_Click;
            TieredStopAtLevelToggle.Click += TieredStopAtLevelToggle_Click;
            ThreadStackSizeToggle.Click += ThreadStackSizeToggle_Click;

        }



        private void LoadCurrentSettings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath))
                {
                    if (key != null)
                    {
                        // DisablePagingExecutive
                        var disablePagingValue = key.GetValue("DisablePagingExecutive");
                        DisablePagingExecutiveToggle.IsChecked = (disablePagingValue is int disablePagingInt && disablePagingInt == 1);

                        // ContextSwitchDeadband
                        var contextSwitchValue = key.GetValue("ContextSwitchDeadband");
                        ContextSwitchDeadbandToggle.IsChecked = (contextSwitchValue is int && (int)contextSwitchValue == 1);

                        // LatencySensitivityHint
                        var latencyHintValue = key.GetValue("LatencySensitivityHint");
                        LatencySensitivityHintToggle.IsChecked = (latencyHintValue is int && (int)latencyHintValue == 1);

                        // DisableHeapCoalesceOnFree
                        var heapCoalesceValue = key.GetValue("DisableHeapCoalesceOnFree");
                        DisableHeapCoalesceOnFreeToggle.IsChecked = (heapCoalesceValue is int disableHeapInt && disableHeapInt == 1);

                        var systemCacheDirtyPageThresholdValue = key.GetValue("SystemCacheDirtyPageThreshold");
                        SystemCacheDirtyPageThresholdToggle.IsChecked = systemCacheDirtyPageThresholdValue != null && (int)systemCacheDirtyPageThresholdValue == 3;


                        // LargePageMinimum
                        var largePageMinimumValue = key.GetValue("LargePageMinimum");
                        LargePageMinimumToggle.IsChecked = (largePageMinimumValue is int largePageMinInt && largePageMinInt == unchecked((int)0xFFFFFFFF));

                        var secondLevelCacheValue = key.GetValue("SecondLevelDataCache");
                        SecondLevelDataCacheToggle.IsChecked = secondLevelCacheValue != null;

                        // ThirdLevelDataCache
                        var thirdLevelCacheValue = key.GetValue("ThirdLevelDataCache");
                        ThirdLevelDataCacheToggle.IsChecked = thirdLevelCacheValue != null;

                        // LargePageSizeInBytes
                        var LargePageSizeInByteseValue = key.GetValue("LargePageSizeInBytes");
                        LargePageSizeInBytesToggle.IsChecked = LargePageSizeInByteseValue != null;

                        // LockPagesInMemoryValue
                        var LockPagesInMemoryValue = key.GetValue("LockPagesInMemory");
                        LockPagesInMemoryToggle.IsChecked = LockPagesInMemoryValue != null;

                        // LargePageHeapSizeThreshold
                        var LargePageHeapSizeThreshold = key.GetValue("LargePageHeapSizeThreshold");
                        LargePageHeapSizeThresholdToggle.IsChecked = LargePageHeapSizeThreshold != null;

                        // UseBiasedLocking
                        var UseBiasedLockingValue = key.GetValue("UseBiasedLocking");
                        UseBiasedLockingToggle.IsChecked = UseBiasedLockingValue != null;

                        // TieredCompilation
                        var TieredCompilationValue = key.GetValue("TieredCompilation");
                        TieredCompilationToggle.IsChecked = TieredCompilationValue != null;

                        // TieredStopAtLevel
                        var TieredStopAtLevelValue = key.GetValue("TieredStopAtLevel");
                        TieredStopAtLevelToggle.IsChecked = TieredStopAtLevelValue != null;

                        // ThreadStackSize
                        var ThreadStackSizeValue = key.GetValue("ThreadStackSize");
                        ThreadStackSizeToggle.IsChecked = ThreadStackSizeValue != null;

                        // Mitigations
                        var DisableOSMitigationsValue = key.GetValue("FeatureSettingsOverride");
                        var DisableOSMitigationsMask = key.GetValue("FeatureSettingsOverrideMask");

                        // Check if FeatureSettingsOverride is 0x00000048 (default value)
                        if (DisableOSMitigationsValue is int disableOSMitigations && disableOSMitigations == 0x00000048)
                        {
                            // Set the toggle off if the value is 0x00000048
                            DisableOSMitigationsToggle.IsChecked = false;
                        }
                        else
                        {
                            // Use existing logic if FeatureSettingsOverride is not the default value
                            DisableOSMitigationsToggle.IsChecked = DisableOSMitigationsValue != null && DisableOSMitigationsMask != null;
                        }

                    }
                    else
                    {
                        ShowError("Failed to access Memory Management registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
            }
        }

        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            // Update the active tag
            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            // Update the active tag
            ExperimentalButton.Tag = "Active";
            VerifiedButton.Tag = "Inactive";
        }

        private void SystemCacheDirtyPageThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SystemCacheDirtyPageThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("SystemCacheDirtyPageThreshold", 3); // Set to tweaked value
            }
            else
            {
                DeleteRegistryValue("SystemCacheDirtyPageThreshold"); // Delete if toggled off (reset to original)
            }
        }


        private void SecondLevelDataCacheToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SecondLevelDataCacheToggle.IsChecked == true)
            {
                UpdateRegistryValue("SecondLevelDataCache", 0x00FA332A); // Set to default value
            }
            else
            {
                DeleteRegistryValue("SecondLevelDataCache");
            }
        }

        private void ThirdLevelDataCacheToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ThirdLevelDataCacheToggle.IsChecked == true)
            {
                UpdateRegistryValue("ThirdLevelDataCache", 0x00FA332A); // Set to default value
            }
            else
            {
                DeleteRegistryValue("ThirdLevelDataCache");
            }
        }

        private void DisablePagingExecutiveToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistryValue("DisablePagingExecutive", DisablePagingExecutiveToggle.IsChecked == true ? 1 : 0);
        }

        private void ContextSwitchDeadbandToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ContextSwitchDeadbandToggle.IsChecked == true)
            {
                UpdateRegistryValue("ContextSwitchDeadband", 1);
            }
            else
            {
                DeleteRegistryValue("ContextSwitchDeadband");
            }
        }

        private void LatencySensitivityHintToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LatencySensitivityHintToggle.IsChecked == true)
            {
                UpdateRegistryValue("LatencySensitivityHint", 1);
            }
            else
            {
                DeleteRegistryValue("LatencySensitivityHint");
            }
        }

        private void DisableHeapCoalesceOnFreeToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistryValue("DisableHeapCoalesceOnFree", DisableHeapCoalesceOnFreeToggle.IsChecked == true ? 1 : 0);
        }

        private void LargePageMinimumToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageMinimumToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageMinimum", unchecked((int)0xFFFFFFFF)); // Set to 0xFFFFFFFF
            }
            else
            {
                DeleteRegistryValue("LargePageMinimum");
            }
        }

        private void DisableOSMitigationsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DisableOSMitigationsToggle.IsChecked == true)
            {
                // Set to tweaked values
                UpdateRegistryValue("FeatureSettingsOverride", 0x00000003);  // Tweaked value for FeatureSettingsOverride
                UpdateRegistryValue("FeatureSettingsOverrideMask", 0x00000003);  // Tweaked value for FeatureSettingsOverrideMask
            }
            else
            {
                // Set to default values (default value for FeatureSettingsOverride is 0x00000048, FeatureSettingsOverrideMask is 0x00000003)
                UpdateRegistryValue("FeatureSettingsOverride", 0x00000048);  // Default value for FeatureSettingsOverride
                UpdateRegistryValue("FeatureSettingsOverrideMask", 0x00000003);  // Default value for FeatureSettingsOverrideMask
            }
        }

        private void LargePageSizeInBytesToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageSizeInBytesToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageSizeInBytes", 3);
            }
            else
            {
                DeleteRegistryValue("LargePageSizeInBytes");
            }
        }

        private void LockPagesInMemoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LockPagesInMemoryToggle.IsChecked == true)
            {
                UpdateRegistryValue("LockPagesInMemory", 1);
            }
            else
            {
                DeleteRegistryValue("LockPagesInMemory");
            }
        }

        private void LargePageHeapSizeThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageHeapSizeThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageHeapSizeThreshold", 3);
            }
            else
            {
                DeleteRegistryValue("LargePageHeapSizeThreshold");
            }
        }

        private void UseBiasedLockingToggle_Click(object sender, RoutedEventArgs e)
        {
            if (UseBiasedLockingToggle.IsChecked == true)
            {
                UpdateRegistryValue("UseBiasedLocking", 1);
            }
            else
            {
                DeleteRegistryValue("UseBiasedLocking");
            }
        }

        private void TieredCompilationToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TieredCompilationToggle.IsChecked == true)
            {
                UpdateRegistryValue("TieredCompilation", 16);
            }
            else
            {
                DeleteRegistryValue("TieredCompilation");
            }
        }

        private void TieredStopAtLevelToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TieredStopAtLevelToggle.IsChecked == true)
            {
                UpdateRegistryValue("TieredStopAtLevel", 1);
            }
            else
            {
                DeleteRegistryValue("TieredStopAtLevel");
            }
        }

        private void ThreadStackSizeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ThreadStackSizeToggle.IsChecked == true)
            {
                UpdateRegistryValue("ThreadStackSize", 3);
            }
            else
            {
                DeleteRegistryValue("ThreadStackSize");
            }
        }


        private void UpdateRegistryValue(string valueName, int value)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} set to {value}.");
                    }
                    else
                    {
                        ShowError($"Failed to access registry key: {valueName}");
                        App.changelogUserControl?.AddLog("Failed", $"Failed to access registry key: {valueName}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access while modifying {valueName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating {valueName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating {valueName}: {ex.Message}");
            }
        }

        private void DeleteRegistryValue(string valueName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(valueName, false);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} deleted.");
                    }
                    else
                    {
                        ShowError($"Failed to access registry key: {valueName}");
                        App.changelogUserControl?.AddLog("Failed", $"Failed to access registry key: {valueName}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access while deleting {valueName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting {valueName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error deleting {valueName}: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
