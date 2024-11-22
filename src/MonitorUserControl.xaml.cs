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
        private const string PCIMaxPerfKeyPath = @"SYSTEM\CurrentControlSet\Control\PnP\Pci";
        private const string IntelPpmKeyPath = @"SYSTEM\CurrentControlSet\Services\intelppm\Parameters";
        private const string AmdPpmKeyPath = @"SYSTEM\CurrentControlSet\Services\amdppm\Parameters";
        private const string PowerHackFlagsKeyPath = @"SYSTEM\CurrentControlSet\Services\Power\Parameters";
        private const string PCIHackFlagsKeyPath = @"SYSTEM\CurrentControlSet\Services\pci\Parameters";
        private const string BasicRenderKeyPath = @"SYSTEM\CurrentControlSet\Services\BasicRender\Parameters";
        private const string GraphicsPerfKeyPath = @"SYSTEM\CurrentControlSet\Services\GraphicsPerfSvc\Parameters";
        private const string nvstorKeyPath = @"SYSTEM\CurrentControlSet\Services\nvstor\Parameters";

        private MainWindow mainWindow;

        public MonitorUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Misc";

            // Temporarily detach event handlers to avoid premature triggering during initialization
            VsyncIdleToggle.Click -= VsyncIdleToggle_Click;
            RefreshLatencyToggle.Click -= RefreshLatencyToggle_Click;
            LatencyToggle.Click -= LatencyToggle_Click;
            PCIMaxPerfToggle.Click -= PCIMaxPerfToggle_Click;
            IntelPpmToggle.Click -= IntelPpmToggle_Click;
            AmdPpmToggle.Click -= AmdPpmToggle_Click;
            TweakPowerHackFlagsToggle.Click -= TweakPowerHackFlagsToggle_Click;
            PCIHackFlagsToggle.Click -= PCIHackFlagsToggle_Click;
            BasicRenderToggle.Click -= BasicRenderToggle_Click;
            GraphicsPerfToggle.Click -= GraphicsPerfToggle_Click;
            nvstorToggle.Click -= nvstorToggle_Click;


            LoadCurrentSettings(); // Load current values on initialization

            // Reattach event handlers after loading
            VsyncIdleToggle.Click += VsyncIdleToggle_Click;
            RefreshLatencyToggle.Click += RefreshLatencyToggle_Click;
            LatencyToggle.Click += LatencyToggle_Click;
            PCIMaxPerfToggle.Click += PCIMaxPerfToggle_Click;
            IntelPpmToggle.Click += IntelPpmToggle_Click;
            AmdPpmToggle.Click += AmdPpmToggle_Click;
            TweakPowerHackFlagsToggle.Click += TweakPowerHackFlagsToggle_Click;
            PCIHackFlagsToggle.Click += PCIHackFlagsToggle_Click;
            BasicRenderToggle.Click += BasicRenderToggle_Click;
            GraphicsPerfToggle.Click += GraphicsPerfToggle_Click;
            nvstorToggle.Click += nvstorToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load VsyncIdleTimeout
                using (var key = Registry.LocalMachine.CreateSubKey(VsyncKeyPath))
                {
                    if (key != null)
                    {
                        var vsyncValue = key.GetValue("VsyncIdleTimeout");
                        VsyncIdleToggle.IsChecked = vsyncValue is int vsyncInt && vsyncInt == 0;
                    }
                }

                // Load MonitorRefreshLatencyTolerance
                using (var key = Registry.LocalMachine.CreateSubKey(PowerKeyPath))
                {
                    if (key != null)
                    {
                        var refreshLatencyValue = key.GetValue("MonitorRefreshLatencyTolerance");
                        RefreshLatencyToggle.IsChecked = refreshLatencyValue is int refreshInt && refreshInt == 1;
                    }
                }

                // Load PCI Max Perf setting (HackFlags)
                using (var key = Registry.LocalMachine.CreateSubKey(PCIMaxPerfKeyPath))
                {
                    if (key != null)
                    {
                        var hackFlagsValue = key.GetValue("HackFlags");
                        PCIMaxPerfToggle.IsChecked = hackFlagsValue is int hackFlagsInt && hackFlagsInt == 0x100;
                    }
                }

                // Load MonitorLatencyTolerance
                using (var key = Registry.LocalMachine.CreateSubKey(DXGKrnlKeyPath))
                {
                    if (key != null)
                    {
                        var latencyValue = key.GetValue("MonitorLatencyTolerance");
                        LatencyToggle.IsChecked = latencyValue is int latencyInt && latencyInt == 1;
                    }
                }

                // Load Intel HackFlags
                using (var key = Registry.LocalMachine.CreateSubKey(IntelPpmKeyPath))
                {
                    if (key != null)
                    {
                        var intelHackFlags = key.GetValue("HackFlags");
                        IntelPpmToggle.IsChecked = intelHackFlags is int intelHackFlagsInt && intelHackFlagsInt == 1;
                    }
                }

                // Load AMD HackFlags
                using (var key = Registry.LocalMachine.CreateSubKey(AmdPpmKeyPath))
                {
                    if (key != null)
                    {
                        var amdHackFlags = key.GetValue("HackFlags");
                        AmdPpmToggle.IsChecked = amdHackFlags is int amdHackFlagsInt && amdHackFlagsInt == 1;
                    }
                }

                using (var key = Registry.LocalMachine.CreateSubKey(PowerHackFlagsKeyPath))
                {
                    if (key != null)
                    {
                        var tweakPowerHackFlags = key.GetValue("HackFlags");
                        TweakPowerHackFlagsToggle.IsChecked = tweakPowerHackFlags is int value && value == 1;
                    }
                }

                using (var key = Registry.LocalMachine.CreateSubKey(PCIHackFlagsKeyPath))
                {
                    if (key != null)
                    {
                        var pciHackFlags = key.GetValue("HackFlags");
                        PCIHackFlagsToggle.IsChecked = pciHackFlags is int value && value == 1;
                    }
                }

                // Load BasicRender HackFlags
                using (var key = Registry.LocalMachine.CreateSubKey(BasicRenderKeyPath))
                {
                    if (key != null)
                    {
                        var hackFlagsValue = key.GetValue("HackFlags");
                        BasicRenderToggle.IsChecked = hackFlagsValue is int hackFlagsInt && hackFlagsInt == 1;
                    }
                }

                // Load GraphicsPerf HackFlags
                using (var key = Registry.LocalMachine.CreateSubKey(BasicRenderKeyPath))
                {
                    if (key != null)
                    {
                        var hackFlagsValue = key.GetValue("HackFlags");
                        GraphicsPerfToggle.IsChecked = hackFlagsValue is int hackFlagsInt && hackFlagsInt == 1;
                    }
                }

                // Load nvstor HackFlags
                using (var key = Registry.LocalMachine.CreateSubKey(BasicRenderKeyPath))
                {
                    if (key != null)
                    {
                        var hackFlagsValue = key.GetValue("HackFlags");
                        nvstorToggle.IsChecked = hackFlagsValue is int hackFlagsInt && hackFlagsInt == 1;
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

        private void VsyncIdleToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(VsyncKeyPath, "VsyncIdleTimeout", VsyncIdleToggle.IsChecked == true ? 0 : 1);
        }

        private void RefreshLatencyToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(PowerKeyPath, "MonitorRefreshLatencyTolerance", RefreshLatencyToggle.IsChecked == true ? 1 : 0);
        }

        private void LatencyToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(DXGKrnlKeyPath, "MonitorLatencyTolerance", LatencyToggle.IsChecked == true ? 1 : 0);
        }

        private void PCIMaxPerfToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(PCIMaxPerfKeyPath, "HackFlags", PCIMaxPerfToggle.IsChecked == true ? 0x100 : 0);
        }

        private void IntelPpmToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(IntelPpmKeyPath, "HackFlags", IntelPpmToggle.IsChecked == true ? 1 : 0);
        }

        private void AmdPpmToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(AmdPpmKeyPath, "HackFlags", AmdPpmToggle.IsChecked == true ? 1 : 0);
        }

        private void TweakPowerHackFlagsToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(PowerHackFlagsKeyPath, "HackFlags", TweakPowerHackFlagsToggle.IsChecked == true ? 1 : 0);
        }

        private void PCIHackFlagsToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(PCIHackFlagsKeyPath, "HackFlags", PCIHackFlagsToggle.IsChecked == true ? 1 : 0);
        }

        private void BasicRenderToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(BasicRenderKeyPath, "HackFlags", BasicRenderToggle.IsChecked == true ? 1 : 0);
        }

        private void GraphicsPerfToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(GraphicsPerfKeyPath, "HackFlags", BasicRenderToggle.IsChecked == true ? 1 : 0);
        }

        private void nvstorToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(nvstorKeyPath, "HackFlags", nvstorToggle.IsChecked == true ? 1 : 0);
        }


        private void UpdateRegistry(string keyPath, string valueName, int value)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} set to {value}");
                    }
                    else
                    {
                        ShowError($"Failed to access registry key at {keyPath}");
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

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
