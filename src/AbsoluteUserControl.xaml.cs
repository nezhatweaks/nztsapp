using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class AbsoluteUserControl : UserControl 
    {
        private const string AbsoluteKeyPath = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\Absolute";
        private const string AbsoluteKeyPath2 = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\Absolute\./syncml/dmacc";
        private const string AbsoluteKeyPath3 = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\Absolute\DevDetail";
        private const string AbsoluteKeyPath4 = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\Absolute\DevInfo";
        private MainWindow mainWindow;

        public AbsoluteUserControl(MainWindow window) 
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Absolute";

            // Temporarily detach event to avoid premature triggering during initialization
            AbsoluteToggle.Click -= AbsoluteToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            AbsoluteToggle.Click += AbsoluteToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load Absolute Enabled value
                using (var key = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath))
                {
                    if (key != null)
                    {
                        var enabledValue = key.GetValue("AbsoluteEnabled");
                        if (enabledValue == null) // Key does not exist
                        {
                            key.SetValue("AbsoluteEnabled", 0, RegistryValueKind.DWord); // Default value
                            AbsoluteToggle.IsChecked = false; // Default state
                        }
                        else
                        {
                            AbsoluteToggle.IsChecked = (enabledValue is int enabledInt && enabledInt == 1);
                        }
                    }
                    else
                    {
                        ShowError("Failed to access Absolute registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Absoluteregistry key not found.");
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

        private void AbsoluteToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open all four registry keys
                using (var key = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath))
                using (var key2 = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath2))
                using (var key3 = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath3))
                using (var key4 = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath4))
                {
                    if (key != null)
                    {
                        // Logic for enabling or disabling for the first registry path (AbsoluteKeyPath)
                        if (AbsoluteToggle.IsChecked == true)
                        {
                            // Set the registry values for AbsoluteKeyPath
                            key.SetValue("AbsoluteEnabled", 1, RegistryValueKind.DWord); // Enable the feature
                            key.SetValue("./devdetail", 1000000, RegistryValueKind.DWord); // Set devdetail value
                            key.SetValue("./devinfo", 1000000, RegistryValueKind.DWord);  // Set devinfo value
                            key.SetValue("./syncml/dmacc", 1000000, RegistryValueKind.DWord); // Set syncml/dmacc value

                            App.changelogUserControl?.AddLog("Applied", "Absolute settings enabled.");
                        }
                        else
                        {
                            key.SetValue("AbsoluteEnabled", 0, RegistryValueKind.DWord);
                            key.SetValue("./devdetail", 1000030, RegistryValueKind.DWord); // Set devdetail value
                            key.SetValue("./devinfo", 1000030, RegistryValueKind.DWord);  // Set devinfo value
                            key.SetValue("./syncml/dmacc", 1000030, RegistryValueKind.DWord); // Set syncml/dmacc value

                            App.changelogUserControl?.AddLog("Applied", "Absolute settings disabled.");
                        }

                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError("Failed to access Absolute registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Absolute registry key not found.");
                    }

                    // Logic for AbsoluteKeyPath2 (same as before)
                    if (key2 != null)
                    {
                        if (AbsoluteToggle.IsChecked == true)
                        {
                            // Set the registry values for AbsoluteKeyPath2 (same logic as before)
                            key2.SetValue("MdmEvaluate", 1, RegistryValueKind.DWord); // Enable the feature

                            App.changelogUserControl?.AddLog("Applied", "MdmEvaluate tweak is set to 1");
                        }
                        else
                        {
                            key2.SetValue("MdmEvaluate", 3, RegistryValueKind.DWord); // Disable the feature

                            App.changelogUserControl?.AddLog("Restore", "MdmEvaluate tweak is set to 3");
                        }

                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError("Failed to access AbsoluteKeyPath2 registry key.");
                        App.changelogUserControl?.AddLog("Failed", "AbsoluteKeyPath2 registry key not found.");
                    }

                    // Logic for AbsoluteKeyPath3 and AbsoluteKeyPath4 (new behavior)
                    foreach (var keyPath in new[] { key3, key4 })
                    {
                        if (keyPath != null)
                        {
                            if (AbsoluteToggle.IsChecked == true)
                            {
                                // For key3 and key4, set MdmEvaluate to 0 when enabled
                                keyPath.SetValue("MdmEvaluate", 0, RegistryValueKind.DWord); // Disable the feature (MdmEvaluate = 0)

                                App.changelogUserControl?.AddLog("Applied", "MdmEvaluate tweak is set to 0 for " + keyPath.ToString());
                            }
                            else
                            {
                                // Set MdmEvaluate to 1 when disabled (default value)
                                keyPath.SetValue("MdmEvaluate", 1, RegistryValueKind.DWord); // Enable the feature (MdmEvaluate = 1)

                                App.changelogUserControl?.AddLog("Restore", "MdmEvaluate tweak is set to 1 for " + keyPath.ToString());
                            }

                            mainWindow?.MarkSettingsApplied();
                        }
                        else
                        {
                            ShowError($"Failed to access registry key at {keyPath.ToString()}.");
                            App.changelogUserControl?.AddLog("Failed", $"Registry key {keyPath.ToString()} not found.");
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify registry.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Absolute settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating settings: {ex.Message}");
            }
        }








        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
