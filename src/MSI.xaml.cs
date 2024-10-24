using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;

namespace NZTS_App.Views
{
    public class MsiToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && str == "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "1" : "0";
        }
    }

    public partial class MSI : UserControl
    {
        private const string PciRegistryKeyPath = @"SYSTEM\CurrentControlSet\Enum\PCI";
        private ObservableCollection<PciDevice> devices = new ObservableCollection<PciDevice>();
        private MainWindow mainWindow;

        public MSI(MainWindow window)
        {
            InitializeComponent();
            DataContext = this; // Ensure DataContext is set
            LoadPciDevicesAsync();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "MSI";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is PciDevice device)
            {
                device.UpdateMsiValue(true); // Set MSISupported to 1
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is PciDevice device)
            {
                device.UpdateMsiValue(false); // Set MSISupported to 0
            }
        }

        


private async void LoadPciDevicesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(PciRegistryKeyPath))
                    {
                        if (key != null)
                        {
                            foreach (var deviceId in key.GetSubKeyNames())
                            {
                                using (var deviceKey = key.OpenSubKey(deviceId))
                                {
                                    if (deviceKey != null)
                                    {
                                        var deviceDescription = GetDeviceDescription(deviceKey) ?? deviceId;
                                        var fullRegistryPath = deviceKey.Name;
                                        var parts = fullRegistryPath.Split('\\');
                                        var instanceId = parts.Length > 1 ? parts[^1] : deviceId;

                                        if (IsUnwantedDevice(deviceId, deviceDescription))
                                        {
                                            continue;
                                        }

                                        var device = new PciDevice
                                        {
                                            DeviceName = deviceDescription,
                                            InstanceId = instanceId,
                                            DeviceInstancePath = fullRegistryPath,
                                            Irq = GetIrqValue(deviceKey),
                                            Msi = GetMsiValue(deviceKey),
                                            MessageNumberLimit = GetMessageNumberLimit(deviceKey),
                                            MaxLimit = GetRegistryValue(deviceKey, "MaxLimit"),
                                            SupportedModes = GetSupportedModes(deviceKey),
                                            InterruptPriority = GetInterruptPriority(deviceKey)
                                        };

                                        if (device.Irq != "N/A")
                                        {
                                            devices.Add(device);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ShowError("Failed to access PCI registry key.");
                        }
                    }
                });

                DeviceListView.ItemsSource = devices;
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to access the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading PCI devices: {ex.Message}");
            }
        }

        private string GetSupportedModes(RegistryKey deviceKey)
        {
            bool supportsMsi = false;
            bool supportsMsix = false;

            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        using (var parametersKey = subKey.OpenSubKey("Device Parameters"))
                        {
                            if (parametersKey != null)
                            {
                                using (var msiKey = parametersKey.OpenSubKey("Interrupt Management\\MessageSignaledInterruptProperties"))
                                {
                                    if (msiKey != null)
                                    {
                                        var msiSupportedValue = msiKey.GetValue("MSISupported");
                                        if (msiSupportedValue is int msiSupported && msiSupported == 1)
                                        {
                                            supportsMsi = true;
                                        }
                                    }
                                }

                                using (var interruptManagementKey = parametersKey.OpenSubKey("Interrupt Management\\MessageSignaledInterruptProperties"))
                                {
                                    if (interruptManagementKey != null)
                                    {
                                        var msixValue = interruptManagementKey.GetValue("0x00000010");
                                        if (msixValue != null && msixValue is string)
                                        {
                                            supportsMsix = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var modes = "LineBased";
            if (supportsMsi)
            {
                modes += ", MSI";
            }
            if (supportsMsix)
            {
                modes += ", MSI-X";
            }

            return modes;
        }

        





        private string GetMsiValue(RegistryKey deviceKey)
        {
            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        using (var parametersKey = subKey.OpenSubKey("Device Parameters"))
                        {
                            if (parametersKey != null)
                            {
                                using (var msiKey = parametersKey.OpenSubKey("Interrupt Management\\MessageSignaledInterruptProperties"))
                                {
                                    if (msiKey != null)
                                    {
                                        var msiSupportedValue = msiKey.GetValue("MSISupported");
                                        if (msiSupportedValue is int msiSupported)
                                        {
                                            return msiSupported == 1 ? "1" : "0";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "N/A"; // If value not found
        }

        private string GetIrqValue(RegistryKey deviceKey)
        {
            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        using (var parametersKey = subKey.OpenSubKey("Device Parameters"))
                        {
                            if (parametersKey != null)
                            {
                                using (var routingInfoKey = parametersKey.OpenSubKey("Interrupt Management\\Routing Info"))
                                {
                                    if (routingInfoKey != null)
                                    {
                                        var staticVectorValue = routingInfoKey.GetValue("StaticVector");
                                        if (staticVectorValue is int staticVector)
                                        {
                                            return staticVector.ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "N/A"; // If value not found
        }

        private string GetMessageNumberLimit(RegistryKey deviceKey)
        {
            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        using (var parametersKey = subKey.OpenSubKey("Device Parameters"))
                        {
                            if (parametersKey != null)
                            {
                                using (var messageKey = parametersKey.OpenSubKey("Interrupt Management\\MessageSignaledInterruptProperties"))
                                {
                                    if (messageKey != null)
                                    {
                                        var messageNumberLimitValue = messageKey.GetValue("MessageNumberLimit");
                                        return messageNumberLimitValue?.ToString() ?? "-"; // Return value or N/A
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "-"; // If no valid subkeys are found
        }

        private string GetInterruptPriority(RegistryKey deviceKey)
        {
            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        using (var parametersKey = subKey.OpenSubKey("Device Parameters"))
                        {
                            if (parametersKey != null)
                            {
                                using (var interruptKey = parametersKey.OpenSubKey("Interrupt Management\\Affinity Policy"))
                                {
                                    if (interruptKey != null)
                                    {
                                        var priorityValue = interruptKey.GetValue("DevicePriority");
                                        if (priorityValue is int priority)
                                        {
                                            return ConvertInterruptPriority(priority);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return "-"; // Default to - if not found
        }

        private string ConvertInterruptPriority(int priority)
        {
            return priority switch
            {
                1 => "LOW",
                2 => "NORMAL",
                3 => "HIGH",
                _ => "-",
            };
        }

        private bool IsUnwantedDevice(string deviceId, string deviceDescription)
        {
            var unwantedIdentifiers = new[]
            {
                "PEG10", "PEG60", "Host Bridge", "LPC", "SMBus",
                "SPI Flash Controller", "Express Root", "LPSS Controller",
                "Standard SATA AHCI Controller", "SATA", "AHCI",
                "VEN_1B4B", "VEN_1022"
            };

            foreach (var identifier in unwantedIdentifiers)
            {
                if (deviceId.IndexOf(identifier, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    deviceDescription.IndexOf(identifier, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true; // It's an unwanted device
                }
            }

            return false; // Not an unwanted device
        }

        private string GetRegistryValue(RegistryKey key, string valueName)
        {
            try
            {
                var value = key.GetValue(valueName);
                return value?.ToString() ?? "N/A";
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing {valueName}: {ex.Message}");
                return "N/A";
            }
        }

        private void SetLimitButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var device = button.DataContext as PciDevice;

                if (device != null && int.TryParse(device.MessageNumberLimit, out int newLimit) && newLimit >= 0 && newLimit <= 2048)
                {
                    device.UpdateLimitInRegistry(newLimit);
                }
                else
                {
                    MessageBox.Show("Please enter a valid limit value (0 - 2048).");
                }
            }
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCreateRestorePointDialog();
        }

        private void OpenCreateRestorePointDialog()
        {
            try
            {
                // Open the System Properties dialog for creating a restore point
                System.Diagnostics.Process.Start("SystemPropertiesProtection.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the restore point dialog. Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private string? GetDeviceDescription(RegistryKey deviceKey)
        {
            var deviceDescValue = deviceKey.GetValue("DeviceDesc") as string;

            if (!string.IsNullOrEmpty(deviceDescValue))
            {
                return CleanDeviceName(deviceDescValue);
            }

            foreach (var subKeyName in deviceKey.GetSubKeyNames())
            {
                using (var subKey = deviceKey.OpenSubKey(subKeyName))
                {
                    if (subKey != null)
                    {
                        deviceDescValue = subKey.GetValue("DeviceDesc") as string;
                        if (!string.IsNullOrEmpty(deviceDescValue))
                        {
                            return CleanDeviceName(deviceDescValue);
                        }
                    }
                }
            }

            return null; // Return null if no description is found
        }

        private string CleanDeviceName(string deviceDescValue)
        {
            var parts = deviceDescValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[1].Trim() : deviceDescValue.Trim();
        }
    }

    public class PciDevice : INotifyPropertyChanged
    {
        private string? msi;
        private string? messageNumberLimit;
        private string? interruptPriority; // Add this field for Interrupt Priority
        private const int MaxLimitValue = 64;

        public string? DeviceName { get; set; }
        public string? InstanceId { get; set; }
        public string? DeviceInstancePath { get; set; }
        public string? Irq { get; set; }

        public string? Msi
        {
            get => msi;
            set
            {
                if (msi != value)
                {
                    msi = value;
                    OnPropertyChanged(nameof(Msi));
                }
            }
        }

        public string? MessageNumberLimit
        {
            get => messageNumberLimit;
            set
            {
                if (messageNumberLimit != value)
                {
                    messageNumberLimit = value;
                    OnPropertyChanged(nameof(MessageNumberLimit));
                }
            }
        }

        public string? Limit { get; set; }
        public string? MaxLimit { get; set; }
        public string? SupportedModes { get; set; }

        // New property for Interrupt Priority
        public string? InterruptPriority
        {
            get => interruptPriority;
            set
            {
                if (interruptPriority != value)
                {
                    interruptPriority = value;
                    OnPropertyChanged(nameof(InterruptPriority));
                    UpdateInterruptPriorityInRegistry(value); // Call the method to update the registry
                }
            }
        }

        public void UpdateLimitInRegistry(int newValue)
        {
            // Construct the correct registry path for the specific device
            string registryPath = @$"SYSTEM\CurrentControlSet\Enum\PCI\{InstanceId}";

            try
            {
                // Open the registry key with write access
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                    {
                        MessageBox.Show("Device key not found.");
                        return;
                    }

                    // Iterate through the subkeys to find the Device Parameters key
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName, writable: true))
                        {
                            if (subKey != null)
                            {
                                // Open the Device Parameters subkey
                                var parametersKey = subKey.OpenSubKey("Device Parameters", writable: true);
                                if (parametersKey != null)
                                {
                                    // Open the Interrupt Management key
                                    using (var interruptManagementKey = parametersKey.OpenSubKey("Interrupt Management", writable: true))
                                    {
                                        if (interruptManagementKey != null)
                                        {
                                            // Open the MessageSignaledInterruptProperties key
                                            using (var msiPropertiesKey = interruptManagementKey.OpenSubKey("MessageSignaledInterruptProperties", writable: true))
                                            {
                                                if (msiPropertiesKey != null)
                                                {
                                                    // Set the new limit value
                                                    msiPropertiesKey.SetValue("MessageNumberLimit", newValue, RegistryValueKind.DWord);
                                                    MessageBox.Show($"MessageNumberLimit successfully updated to {newValue}.");
                                                    return; // Exit once the update is done
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unauthorized access: run as administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating MessageNumberLimit value: {ex.Message}");
            }
        }




        private void UpdateInterruptPriorityInRegistry(string? newValue)
        {
            if (newValue == null) return;

            string registryPath = @$"SYSTEM\CurrentControlSet\Enum\PCI\{InstanceId}";

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                    {
                        MessageBox.Show("Device key not found.");
                        return;
                    }

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName, writable: true))
                        {
                            if (subKey != null)
                            {
                                var parametersKey = subKey.OpenSubKey("Device Parameters", writable: true);
                                if (parametersKey != null)
                                {
                                    using (var interruptKey = parametersKey.OpenSubKey("Interrupt Management\\Affinity Policy", writable: true))
                                    {
                                        if (interruptKey != null)
                                        {
                                            int priorityValue = newValue switch
                                            {
                                                "LOW" => 1,
                                                "NORMAL" => 2,
                                                "HIGH" => 3,
                                                _ => 0 // Default case for UNDEFINED
                                            };

                                            interruptKey.SetValue("DevicePriority", priorityValue, RegistryValueKind.DWord);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unauthorized access: run as administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Interrupt Priority: {ex.Message}");
            }
        }

        // Property for Priority Options
        public string[] PriorityOptions => new[] { "LOW", "NORMAL", "HIGH", "-" };

        public void UpdateMsiValue(bool isEnabled)
        {
            string registryPath = @$"SYSTEM\CurrentControlSet\Enum\PCI\{InstanceId}";

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                    {
                        MessageBox.Show("Device key not found.");
                        return;
                    }

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                var parametersKey = subKey.OpenSubKey("Device Parameters", writable: true);
                                if (parametersKey != null)
                                {
                                    var msiKey = parametersKey.OpenSubKey("Interrupt Management\\MessageSignaledInterruptProperties", writable: true);
                                    if (msiKey != null)
                                    {
                                        msiKey.SetValue("MSISupported", isEnabled ? 1 : 0, RegistryValueKind.DWord);
                                        Msi = isEnabled ? "1" : "0";
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show("MSI key not found under any subkey.");
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unauthorized access: run as administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating MSI value: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}


