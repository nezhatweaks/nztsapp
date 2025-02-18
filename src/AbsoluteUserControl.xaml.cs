using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class AbsoluteUserControl : UserControl 
    {
        private const string AbsoluteKeyPath = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\Absolute";
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
                using (var key = Registry.LocalMachine.CreateSubKey(AbsoluteKeyPath))
                {
                    if (key != null)
                    {
                        // Logic for enabling or disabling
                        if (AbsoluteToggle.IsChecked == true)
                        {
                            // Set the registry values as required
                            key.SetValue("AbsoluteEnabled", 1, RegistryValueKind.DWord); // Enable the feature
                            key.SetValue("./devdetail", 100000, RegistryValueKind.DWord); // Set devdetail value
                            key.SetValue("./devinfo", 100000, RegistryValueKind.DWord);  // Set devinfo value
                            key.SetValue("./syncml/dmacc", 100000, RegistryValueKind.DWord); // Set syncml/dmacc value

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
                        App.changelogUserControl?.AddLog("Failed", "Hypervisor registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Core Isolation.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Absolute settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating MSFT settings: {ex.Message}");
            }
        }






        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
