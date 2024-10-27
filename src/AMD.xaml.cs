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
        private const string UMDRegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\UMD";
        private MainWindow mainWindow;

        public AMD(MainWindow window)
        {
            InitializeComponent();
            LoadCurrentSettings();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Radeon";
        }

        private void LoadCurrentSettings()
        {
            // Load general settings
            LoadRegistryValue("EnableUlps", EnableUlpsSwitch);
            LoadRegistryValue("PP_ThermalAutoThrottlingEnable", ThermalThrottlingSwitch);
            
            LoadRegistryValue("DisableBlockWrite", DisableBlockWriteSwitch);
            LoadRegistryValue("StutterMode", StutterModeSwitch);
            
            
            LoadRegistryValue("DisableFBCForFullScreenApp", DisableFBCForFullscreenSwitch);
            LoadRegistryValue("DisableFBCSupport", DisableFBCSupportSwitch);
            LoadRegistryValue("EnableAspmL0s", EnableAspmL0sSwitch);
            LoadRegistryValue("EnableAspmL1", EnableAspmL1Switch);

            // Load new settings from UMD path
            LoadRegistryValue("Main3D", Main3DSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("Main3D_DEF", Main3DDefSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("FlipQueueSize", FlipQueueSizeSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("ForceTripleBuffering", ForceTripleBufferingSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("PowerState", PowerStateSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("ShaderCache", ShaderCacheSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("Tessellation", TessellationSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("VSyncControl", VSyncControlSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("CatalystAI", CatalystAISwitch, UMDRegistryKeyPath);
            LoadRegistryValue("CatalystAI_DEF", CatalystAIDefSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("TextureOpt", TextureOptSwitch, UMDRegistryKeyPath);
            LoadRegistryValue("GLPBMode_DEF", GLPBModeDefSwitch, UMDRegistryKeyPath);
        }

        private void LoadRegistryValue(string valueName, ToggleButton toggleButton, string? registryPath = null)
        {
            string path = registryPath ?? AMDRegistryKeyPath;
            RegistryKey? key = null;

            try
            {
                key = OpenRegistryKey(path);

                if (key != null)
                {
                    object? value = key.GetValue(valueName);
                    if (value == null)
                    {
                        // Create the registry value if it doesn't exist
                        SetRegistryValue(key, valueName, false); // Default to false if not set
                        toggleButton.IsChecked = false;
                    }
                    else
                    {
                        toggleButton.IsChecked = GetToggleState(value);
                    }
                }
                else
                {
                    ShowError($"Failed to access {valueName} registry key.");
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
            finally
            {
                key?.Close();
            }
        }

        private RegistryKey? OpenRegistryKey(string path)
        {
            RegistryKey? key = Registry.LocalMachine.OpenSubKey(path);

            if (key == null)
            {
                // Attempt to find alternative keys
                string basePath = path.Substring(0, path.Length - 4);
                string[] subkeys = Registry.LocalMachine.OpenSubKey(basePath)?.GetSubKeyNames() ?? Array.Empty<string>();

                foreach (string subkey in subkeys)
                {
                    key = Registry.LocalMachine.OpenSubKey($"{basePath}{subkey}");
                    if (key != null)
                    {
                        break;
                    }
                }
            }

            return key;
        }

        private bool? GetToggleState(object? value)
        {
            return value switch
            {
                int currentValue => currentValue != 1, // 1 means disabled
                byte[] binaryValue => binaryValue.Length > 0 && binaryValue[0] == 0x30, // Assuming 0x30 means enabled
                string stringValue => stringValue == "0", // Assuming "0" means enabled
                _ => null // Return null for unexpected value types without showing an error
            };
        }


        private RegistryKey? OpenRegistryKey(string path, bool writable = false)
        {
            // Attempt to open the specified registry key
            RegistryKey? key = Registry.LocalMachine.OpenSubKey(path, writable);

            // If the key is null, create it
            if (key == null && writable)
            {
                // Create the key if it doesn't exist
                string basePath = path.Substring(0, path.LastIndexOf('\\'));
                key = Registry.LocalMachine.CreateSubKey(path);
            }

            return key;
        }

        private void ToggleRegistryValue(string valueName, bool enable, string? registryPath = null)
        {
            string path = registryPath ?? AMDRegistryKeyPath;
            RegistryKey? key = null;

            try
            {
                // Open the registry key, creating it if it doesn't exist
                key = OpenRegistryKey(path, writable: true);

                if (key != null)
                {
                    // Set the value, creating it if necessary
                    SetRegistryValue(key, valueName, enable);
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", $"{valueName} has been set to {(enable ? "Enabled" : "Disabled")}");
                }
                else
                {
                    ShowError($"Failed to access the registry key for '{valueName}'.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating the value '{valueName}': {ex.Message}");
            }
            finally
            {
                key?.Close();
            }
        }


        private void SetRegistryValue(RegistryKey key, string valueName, bool enable)
        {
            switch (valueName)
            {
                case "EnableUlps":
                case "PP_ThermalAutoThrottlingEnable":
                case "StutterMode":
                case "EnableAspmL0s":
                case "EnableAspmL1":
                case "DisableBlockWrite":
                    key.SetValue(valueName, enable ? 0 : 1, RegistryValueKind.DWord);
                    break;

                case "DisableDMACopy":
                case "DisableDrmdmaPowerGating":
                case "DisableFBCSupport":
                    key.SetValue(valueName, enable ? 1 : 0, RegistryValueKind.DWord);
                    break;

                case "Main3D":
                case "FlipQueueSize":
                case "ShaderCache":
                case "VSyncControl":
                case "CatalystAI":
                case "TFQ":
                    byte[] newValue = enable ? new byte[] { 0x30, 0x00 } : new byte[] { 0x31, 0x00 };
                    key.SetValue(valueName, newValue, RegistryValueKind.Binary);
                    break;

                case "ForceTripleBuffering":
                case "PowerState":
                case "Tessellation":
                case "TextureOpt":
                    byte[] multiByteValue = enable ? new byte[] { 0x30, 0x00, 0x00, 0x00 } : new byte[] { 0x31, 0x00, 0x00, 0x00 };
                    key.SetValue(valueName, multiByteValue, RegistryValueKind.Binary);
                    break;

                case "Main3D_DEF":
                case "CatalystAI_DEF":
                case "GLPBMode_DEF":
                case "DisableFBCForFullScreenApp":
                    string stringValue = enable ? "0" : "1";
                    key.SetValue(valueName, stringValue, RegistryValueKind.String);
                    break;

                default:
                    ShowError($"Unsupported registry value: '{valueName}'");
                    break;
            }
        }







        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            // Update button states
            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            // Update button states
            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Active";
        }


        private void EnableUlpsSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("EnableUlps", EnableUlpsSwitch.IsChecked == true);
        }

        private void ThermalThrottlingSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("PP_ThermalAutoThrottlingEnable", ThermalThrottlingSwitch.IsChecked == true);
        }

        

        private void DisableBlockWriteSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("DisableBlockWrite", DisableBlockWriteSwitch.IsChecked == true);
        }

        private void StutterModeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("StutterMode", StutterModeSwitch.IsChecked == true);
        }

        

        

        private void DisableFBCForFullscreenSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("DisableFBCForFullScreenApp", DisableFBCForFullscreenSwitch.IsChecked == true);
        }

        private void DisableFBCSupportSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("DisableFBCSupport", DisableFBCSupportSwitch.IsChecked == true);
        }

        private void EnableAspmL0sSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("EnableAspmL0s", EnableAspmL0sSwitch.IsChecked == true);
        }

        private void EnableAspmL1Switch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("EnableAspmL1", EnableAspmL1Switch.IsChecked == true);
        }

        private void Main3DSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("Main3D", Main3DSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void FlipQueueSizeSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("FlipQueueSize", FlipQueueSizeSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void ForceTripleBufferingSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("ForceTripleBuffering", ForceTripleBufferingSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void PowerStateSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("PowerState", PowerStateSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void ShaderCacheSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("ShaderCache", ShaderCacheSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void TessellationSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("Tessellation", TessellationSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void VSyncControlSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("VSyncControl", VSyncControlSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void CatalystAISwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("CatalystAI", CatalystAISwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void TextureOptSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("TextureOpt", TextureOptSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void Main3DDefSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("Main3D_DEF", Main3DDefSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        

        private void CatalystAIDefSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("CatalystAI_DEF", CatalystAIDefSwitch.IsChecked == true, UMDRegistryKeyPath);
        }

        private void GLPBModeDefSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleRegistryValue("GLPBMode_DEF", GLPBModeDefSwitch.IsChecked == true, UMDRegistryKeyPath);
        }




        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
