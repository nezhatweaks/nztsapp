using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
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
            DisableHeapCoalesceOnFreeToggle.Click += DisableHeapCoalesceOnFreeToggle_Click; // New toggle click event
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

                        // SecondLevelDataCache
                        

                        // ContextSwitchDeadband
                        var contextSwitchValue = key.GetValue("ContextSwitchDeadband");
                        ContextSwitchDeadbandToggle.IsChecked = (contextSwitchValue is int && (int)contextSwitchValue == 1);

                        // LatencySensitivityHint
                        var latencyHintValue = key.GetValue("LatencySensitivityHint");
                        LatencySensitivityHintToggle.IsChecked = (latencyHintValue is int && (int)latencyHintValue == 1);

                        // DisableHeapCoalesceOnFree
                        var heapCoalesceValue = key.GetValue("DisableHeapCoalesceOnFree");
                        DisableHeapCoalesceOnFreeToggle.IsChecked = (heapCoalesceValue is int disableHeapInt && disableHeapInt == 1);
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
