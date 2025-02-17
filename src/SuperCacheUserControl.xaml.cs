using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class SuperCacheUserControl : UserControl
    {
        private const string SuperCacheKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\FS Templates";
        private const string PowerfulKey = "Powerful";
        private readonly MainWindow mainWindow;

        private static readonly string[] CacheTypes = { "Super Cache", "Huge Cache", "Large Cache", "Medium Cache" };

        public SuperCacheUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            LoadCurrentSettings();
            mainWindow.TitleTextBlock.Content = "File System";
        }

        private void LoadCurrentSettings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(SuperCacheKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Create new key and load settings
                        using (var newKey = Registry.LocalMachine.CreateSubKey(SuperCacheKeyPath))
                        {
                            CreatePowerfulEntry(newKey);
                        }
                    }
                    else
                    {
                        DeleteExistingCacheEntries(key);
                        CreatePowerfulEntry(key);
                        LoadCacheSettings(key);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
            }
        }

        private void LoadCacheSettings(RegistryKey key)
        {
            PowerfulToggle.IsChecked = GetRegistryValueAsBool(key, PowerfulKey);
        }

        private bool GetRegistryValueAsBool(RegistryKey key, string cacheType)
        {
            using (var subKey = key.OpenSubKey(cacheType))
            {
                if (subKey != null)
                {
                    object value = subKey.GetValue("Enabled", 0);
                    return (value is int intValue) && (intValue != 0);
                }
            }
            return false; // Default to false if the subkey or value does not exist
        }

        private void DeleteExistingCacheEntries(RegistryKey key)
        {
            foreach (var cacheType in CacheTypes)
            {
                key.DeleteSubKeyTree(cacheType, false);
            }
        }

        private void CreatePowerfulEntry(RegistryKey key)
        {
            using (var subKey = key.CreateSubKey(PowerfulKey))
            {
                if (subKey != null)
                {
                    // Set the default value ("@") for the Powerful key to "Powerful PC"
                    subKey.SetValue(null, "Powerful PC", RegistryValueKind.String); // Default value
                    subKey.SetValue("Enabled", 1, RegistryValueKind.DWord); // Enable Powerful by default
                    subKey.SetValue("NameCache", GetBinaryData(), RegistryValueKind.Binary);
                    subKey.SetValue("PathCache", GetBinaryData(), RegistryValueKind.Binary);
                }
            }

            // Now, set the default string value for the FS Templates key
            using (var fsTemplatesKey = Registry.LocalMachine.OpenSubKey(SuperCacheKeyPath, writable: true))
            {
                fsTemplatesKey?.SetValue(null, "Powerful PC", RegistryValueKind.String); // Set default value
            }
        }

        private byte[] GetBinaryData()
        {
            byte[] binaryData = new byte[32]; // 32 bytes of 0xFF
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = 0xFF;
            }
            return binaryData;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PowerfulToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistryValue(PowerfulKey, PowerfulToggle.IsChecked == true);
        }

        private void UpdateRegistryValue(string cacheType, bool isChecked)
        {
            try
            {
                string subKeyPath = $@"{SuperCacheKeyPath}\{cacheType}";

                using (var key = Registry.LocalMachine.CreateSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        // Set default string value if not set
                        if (key.GetValue("") == null)
                        {
                            key.SetValue("", cacheType, RegistryValueKind.String);
                        }

                        key.SetValue("Enabled", isChecked ? 1 : 0, RegistryValueKind.DWord);

                        byte[] binaryData = GetBinaryData();

                        if (isChecked)
                        {
                            key.SetValue("NameCache", binaryData, RegistryValueKind.Binary);
                            key.SetValue("PathCache", binaryData, RegistryValueKind.Binary);
                        }
                        else
                        {
                            key.DeleteValue("NameCache", false);
                            key.DeleteValue("PathCache", false);
                        }

                        // Now, set the binary values in the FileSystem key
                        using (var fileSystemKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem", writable: true))
                        {
                            if (fileSystemKey != null)
                            {
                                if (isChecked)
                                {
                                    fileSystemKey.SetValue("NameCache", binaryData, RegistryValueKind.Binary);
                                    fileSystemKey.SetValue("PathCache", binaryData, RegistryValueKind.Binary);
                                }
                                else
                                {
                                    fileSystemKey.DeleteValue("NameCache", false);
                                    fileSystemKey.DeleteValue("PathCache", false);
                                }
                            }
                        }

                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError($"Failed to access registry key: {subKeyPath}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating {cacheType}: {ex.Message}");
            }
        }

        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            ExperimentalButton.Tag = "Active";
            VerifiedButton.Tag = "Inactive";
        }
    }
}
