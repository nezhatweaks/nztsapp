using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using NvAPIWrapper;
using NvAPIWrapper.DRS;
using NvAPIWrapper.DRS.SettingValues;


namespace NZTS_App.Views
{
    public partial class NVIDIAUserControl : UserControl
    {
        private const string DynamicPstateKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
        private const string NvidiaServiceKeyPath = @"SYSTEM\CurrentControlSet\Services\nvlddmkm";
        private const string NVTweakKeyPath = @"Software\NVIDIA Corporation\Global\NVTweak";
        private const int GestaltDefault = 0x203;
        private const int GestaltOptimized = 0xF423F;
        private const int SedonaHasRunDefault = 1;
        private const int SedonaHasRunOptimized = 0;
        private MainWindow mainWindow;

        public NVIDIAUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "NVIDIA";

            // Temporarily detach event to avoid premature triggering during initialization
            DynamicPstateSwitch.Click -= DynamicPstateSwitch_Click;
            LoadCurrentSettings(); // Load current values on initialization
            DynamicPstateSwitch.Click += DynamicPstateSwitch_Click; // Reattach event after loading
        }

        private void LoadCurrentSettings()
        {
            LoadCurrentDynamicPStateValue();
            LoadNvidiaRegistrySettings();
            LoadNVTweakSettings();
        }



        private void LoadCurrentDynamicPStateValue()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(DynamicPstateKeyPath))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("DisableDynamicPstate");
                        if (value is int currentValue)
                        {
                            DynamicPstateSwitch.IsChecked = currentValue == 1;
                        }
                    }
                    else
                    {
                        ShowError("Failed to access DisableDynamicPstate registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current value: {ex.Message}");
            }
        }

        private void LoadNvidiaRegistrySettings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(NvidiaServiceKeyPath))
                {
                    if (key != null)
                    {
                        // Load AllowMaxPerf
                        var allowMaxPerf = key.GetValue("AllowMaxPerf");
                        AllowMaxPerfSwitch.IsChecked = (allowMaxPerf is int value) && (value == 1);

                        // Load DisableMshybridNvsrSwitch
                        var disableHybridSwitch = key.GetValue("DisableMshybridNvsrSwitch");
                        DisableMshybridNvsrSwitch.IsChecked = (disableHybridSwitch is int hybridValue) && (hybridValue == 1);

                        // Load DisablePreemption
                        var disablePreemption = key.GetValue("DisablePreemption");
                        DisablePreemptionSwitch.IsChecked = (disablePreemption is int preemptionValue) && (preemptionValue == 1);

                        // Load DisableCudaContextPreemption
                        var disableCudaContextPreemption = key.GetValue("DisableCudaContextPreemption");
                        DisableCudaContextPreemptionSwitch.IsChecked = (disableCudaContextPreemption is int cudaValue) && (cudaValue == 1);
                    }
                    else
                    {
                        ShowError("Failed to access NVIDIA registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading NVIDIA settings: {ex.Message}");
            }
        }

        private void LoadNVTweakSettings()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NVTweakKeyPath))
                {
                    if (key != null)
                    {
                        var gestaltValue = key.GetValue("Gestalt");
                        var sedonaValue = key.GetValue("SedonaHasRun");

                        GestaltOptimizationSwitch.IsChecked = (gestaltValue is int gestalt && gestalt == GestaltOptimized);
                        SedonaHasRunSwitch.IsChecked = (sedonaValue is int sedona && sedona == SedonaHasRunOptimized);
                    }
                    else
                    {
                        ShowError("Failed to access NVTweak registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access NVTweak registry keys.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading NVTweak settings: {ex.Message}");
            }
        }

        private void GestaltOptimizationSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool isOptimized = GestaltOptimizationSwitch.IsChecked == true;
            UpdateNVTweakSetting("Gestalt", isOptimized ? GestaltOptimized : GestaltDefault);
        }

        private void SedonaHasRunSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool isOptimized = SedonaHasRunSwitch.IsChecked == true;
            UpdateNVTweakSetting("SedonaHasRun", isOptimized ? SedonaHasRunOptimized : SedonaHasRunDefault);
        }


        private void UpdateNVTweakSetting(string valueName, int value)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NVTweakKeyPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(NVTweakKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} set to {value}");
                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError($"Failed to access or create NVTweak registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify NVTweak settings. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating {valueName}: {ex.Message}");
            }
        }



        private void DynamicPstateSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool isDynamicPstateDisabled = DynamicPstateSwitch.IsChecked == true;

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(DynamicPstateKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableDynamicPstate", isDynamicPstateDisabled ? 1 : 0, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", isDynamicPstateDisabled ? "Dynamic P-State disabled." : "Dynamic P-State enabled.");
                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError("Failed to access DisableDynamicPstate registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating registry: {ex.Message}");
            }
        }

        private void UpdateNvidiaSetting(string valueName, bool isChecked)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(NvidiaServiceKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, isChecked ? 1 : 0, RegistryValueKind.DWord);
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} set to {(isChecked ? 1 : 0)}");
                        mainWindow?.MarkSettingsApplied();
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

        private void AllowMaxPerfToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateNvidiaSetting("AllowMaxPerf", AllowMaxPerfSwitch.IsChecked == true);
        }

        private void DisableMshybridNvsrSwitchToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateNvidiaSetting("DisableMshybridNvsrSwitch", DisableMshybridNvsrSwitch.IsChecked == true);
        }

        private void DisablePreemptionToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateNvidiaSetting("DisablePreemption", DisablePreemptionSwitch.IsChecked == true);
        }

        private void DisableCudaContextPreemptionToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateNvidiaSetting("DisableCudaContextPreemption", DisableCudaContextPreemptionSwitch.IsChecked == true);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            App.changelogUserControl?.AddLog("Failed", message);
        }
    }
}
