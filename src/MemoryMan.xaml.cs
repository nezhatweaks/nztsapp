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

                        // LargePageMinimum
                        var largePageMinimumValue = key.GetValue("LargePageMinimum");
                        LargePageMinimumToggle.IsChecked = (largePageMinimumValue is int largePageMinInt && largePageMinInt == unchecked((int)0xFFFFFFFF));

                        var secondLevelCacheValue = key.GetValue("SecondLevelDataCache");
                        SecondLevelDataCacheToggle.IsChecked = secondLevelCacheValue != null;

                        // ThirdLevelDataCache
                        var thirdLevelCacheValue = key.GetValue("ThirdLevelDataCache");
                        ThirdLevelDataCacheToggle.IsChecked = thirdLevelCacheValue != null;
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
