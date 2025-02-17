using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        private CpuBenchmark _cpuBenchmark;


        public MSI(MainWindow window)
        {
            InitializeComponent();
            DataContext = this;  // Ensure DataContext is set
            LoadPciDevicesAsync();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Affinity";

            // Initialize the benchmark and pass the metrics and status window
            _cpuBenchmark = new CpuBenchmark(new BenchmarkMetrics(), new CpuStatusWindow());

            // Subscribe to progress updates and completion events
            _cpuBenchmark.ProgressChanged += OnBenchmarkProgressChanged;
            _cpuBenchmark.BenchmarkCompleted += OnBenchmarkCompleted;
        }

        // Event handler for the Benchmark button click
        private async void StartBenchmarkButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button to prevent clicking while benchmarking
            StartBenchmarkButton.IsEnabled = false;


            // Call the method to start the benchmark
            await _cpuBenchmark.RunBenchmarkAsync();

            // Re-enable the button once benchmarking is complete
            StartBenchmarkButton.IsEnabled = true;
        }

        // Progress update handler
        private void OnBenchmarkProgressChanged(string progress)
        {
            // Check if we're on the UI thread, and if not, invoke the update on the UI thread
            if (CpuStatusTextBlock.Dispatcher.CheckAccess())
            {
                // Update the UI directly if we're on the UI thread
                CpuStatusTextBlock.Text = progress;
            }
            else
            {
                // Use the Dispatcher to marshal the update to the UI thread
                CpuStatusTextBlock.Dispatcher.Invoke(() =>
                {
                    CpuStatusTextBlock.Text = progress;
                });
            }
        }
        public void SubscribeToBenchmarkEvents(CpuBenchmark benchmark)
        {
            benchmark.ProgressChanged += OnBenchmarkProgressChanged;
            benchmark.BenchmarkCompleted += OnBenchmarkCompleted;
        }


        // Benchmark completion handler
        private void OnBenchmarkCompleted()
        {
            // Re-enable the button after the benchmark is complete
            StartBenchmarkButton.IsEnabled = true;
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
















        public class CpuTopology
        {
            public List<string> GetCpuCoreThreadInfo()
            {
                var coreThreadInfo = new List<string>();

                try
                {
                    // Query to get processor information
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

                    // List to track which cores and threads belong to which CPU
                    Dictionary<int, string> cpuToCoreThreadMap = new Dictionary<int, string>();

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // Null-safe way of getting the ProcessorId
                        string processorId = obj["ProcessorId"]?.ToString() ?? "Unknown Processor ID";  // Use default if null
                        int numberOfCores = Convert.ToInt32(obj["NumberOfCores"]);
                        int numberOfLogicalProcessors = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);

                        int logicalProcessorIndex = 0;  // Tracks logical processor index (CPU index)

                        // Iterate through the cores
                        for (int coreIndex = 0; coreIndex < numberOfCores; coreIndex++)
                        {
                            // Assign threads to this core (assuming 1 thread per core for simplicity)
                            for (int threadIndex = 0; threadIndex < (numberOfLogicalProcessors / numberOfCores); threadIndex++)
                            {
                                // Map logical processor to core and thread
                                string coreThread = $"Core {coreIndex + 1}, Thread {threadIndex + 1}";
                                cpuToCoreThreadMap[logicalProcessorIndex] = coreThread;

                                // Display information in a readable format like "CPU 0 = Core 1, Thread 1"
                                coreThreadInfo.Add($"CPU {logicalProcessorIndex} = {coreThread}");

                                logicalProcessorIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                return coreThreadInfo;
            }
        }

        public class PerformanceMetrics
        {
            public double AverageFPS { get; set; }
            public double MinFPS { get; set; }
            public double MaxFPS { get; set; }
            public double StdDeviation { get; set; }
            public double Percentile1Lows { get; set; }
            public double Percentile01Lows { get; set; }
            public List<int> FPSValues { get; set; }  // List to store all FPS values for charting or further analysis

            public PerformanceMetrics()
            {
                FPSValues = new List<int>();
            }
        }







        private void DeviceListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DeviceListView.SelectedItem is PciDevice selectedDevice)
            {
                // Show the dialog where user selects CPU affinity (CPU0, CPU1, etc.)
                SetCpuAffinityForDevice(selectedDevice);
            }
        }

        // Assume we have a PciDevice object that contains the instanceId
        // and a method to get the current selected cores from the registry.

        private void SetCpuAffinityForDevice(PciDevice device)
        {
            var dialog = new CpuAffinityDialog();
            dialog.CpuCores = GetAvailableCpuCores(); // List of CPU cores to choose from
            dialog.InitializeDialog();  // Populate the dialog with CPU cores

            // Retrieve the currently selected cores from the registry (if any)
            List<int> currentSelectedCores = GetCpuAffinityFromRegistry(device);

            // Pre-select the cores in the dialog based on the current affinity
            dialog.PreSelectCores(currentSelectedCores);

            if (dialog.ShowDialog() == true)
            {
                // After user clicks OK, update the device with the selected CPU affinity
                UpdateCpuAffinityInRegistry(device, dialog.SelectedCores);
            }
        }

        private List<int> GetCpuAffinityFromRegistry(PciDevice device)
        {
            List<int> selectedCores = new List<int>();

            try
            {
                // Check if InstanceId is null or empty
                if (string.IsNullOrEmpty(device.InstanceId))
                {
                    MessageBox.Show("Device InstanceId is null or empty. Cannot retrieve CPU affinity.");
                    return selectedCores; // Return an empty list if no valid InstanceId
                }

                // Find the registry path for the given device (without HKEY_LOCAL_MACHINE)
                string registryPath = FindDeviceRegistryPath(device.InstanceId);

                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: false))
                {
                    if (key != null)
                    {
                        var affinityPolicyKey = key.OpenSubKey(@"Device Parameters\Interrupt Management\Affinity Policy");

                        if (affinityPolicyKey != null)
                        {
                            // Read the AssignmentSetOverride value (REG_BINARY)
                            byte[]? affinityMaskBytes = affinityPolicyKey.GetValue("AssignmentSetOverride") as byte[];

                            if (affinityMaskBytes != null && affinityMaskBytes.Length == 8)
                            {
                                // Convert the byte array to a ulong (since it's 64-bit)
                                ulong affinityMask = BitConverter.ToUInt64(affinityMaskBytes, 0);

                                // Extract the selected CPU cores based on the bitmask
                                for (int i = 0; i < 64; i++)  // Max 64 cores
                                {
                                    if ((affinityMask & (1UL << i)) != 0)
                                    {
                                        selectedCores.Add(i);  // Add core to the list if its bit is set
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading CPU Affinity: {ex.Message}");
            }

            return selectedCores;
        }



        private List<string> GetAvailableCpuCores()
        {
            var cpuCores = new List<string>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                cpuCores.Add($"CPU{i}");
            }
            return cpuCores;
        }

        private string FindDeviceRegistryPath(string partialInstanceId)
        {
            string baseRegistryPath = @"SYSTEM\CurrentControlSet\Enum\PCI"; // This part is relative

            // Open the base registry key safely
            using (var baseKey = Registry.LocalMachine.OpenSubKey(baseRegistryPath))
            {
                if (baseKey == null)
                {
                    throw new InvalidOperationException($"Registry path '{baseRegistryPath}' not found.");
                }

                // Iterate over the subkeys of the base registry path
                foreach (var subKeyName in baseKey.GetSubKeyNames())
                {
                    using (var subKey = baseKey.OpenSubKey(subKeyName))
                    {
                        if (subKey != null)
                        {
                            // Full path of the current device's registry key
                            string fullPath = subKey.Name;

                            // Recursively look for the InstanceId in deeper subkeys (like device instance directories)
                            foreach (var deviceSubKeyName in subKey.GetSubKeyNames())
                            {
                                using (var deviceSubKey = subKey.OpenSubKey(deviceSubKeyName))
                                {
                                    if (deviceSubKey != null)
                                    {
                                        // Full path of the current device instance registry key
                                        string deviceFullPath = deviceSubKey.Name;

                                        // Check if the instance ID is part of the full path
                                        if (deviceFullPath.Contains(partialInstanceId))
                                        {
                                            // Once found, return the full registry path, excluding HKEY_LOCAL_MACHINE
                                            return deviceFullPath.Substring(deviceFullPath.IndexOf("SYSTEM"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // If no matching path is found, throw an exception
            throw new InvalidOperationException($"Device registry path for '{partialInstanceId}' not found.");
        }














        private void UpdateCpuAffinityInRegistry(PciDevice device, List<int> selectedCores)
        {
            if (string.IsNullOrEmpty(device.InstanceId))
            {
                MessageBox.Show("Device InstanceId is null or empty. Cannot update CPU affinity.");
                return;  // Exit early if InstanceId is not valid
            }

            // Find the registry path for the given device (without HKEY_LOCAL_MACHINE)
            string registryPath = FindDeviceRegistryPath(device.InstanceId);

            try
            {
                // Open the registry key under Registry.LocalMachine, passing the path without HKEY_LOCAL_MACHINE
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath, writable: true))
                {
                    if (key == null)
                    {
                        MessageBox.Show($"Registry path not found: {registryPath}");
                        return;
                    }

                    // Access or create the "Affinity Policy" registry key under Device Parameters
                    var affinityPolicyKey = key.OpenSubKey(@"Device Parameters\Interrupt Management\Affinity Policy", writable: true);
                    if (affinityPolicyKey == null)
                    {
                        // Create the key if it doesn't exist
                        affinityPolicyKey = key.CreateSubKey(@"Device Parameters\Interrupt Management\Affinity Policy");
                    }

                    if (affinityPolicyKey != null)
                    {
                        // If no CPU cores are selected, remove the registry values
                        if (selectedCores.Count == 0)
                        {
                            // Remove both the "AssignmentSetOverride" and "DevicePolicy" values
                            affinityPolicyKey.DeleteValue("AssignmentSetOverride", false); // false means don't throw an error if it doesn't exist
                            affinityPolicyKey.DeleteValue("DevicePolicy", false);

                            MessageBox.Show("CPU Affinity removed. Both 'AssignmentSetOverride' and 'DevicePolicy' have been deleted.");
                            return;
                        }

                        // Prepare the bitmask for the selected cores
                        ulong affinityMask = 0;

                        // Create the bitmask by setting the corresponding bit for each selected core
                        foreach (var core in selectedCores)
                        {
                            affinityMask |= (1UL << core);  // Set the bit for each selected core (1UL for 64-bit)
                        }

                        // Convert the affinity mask to a byte array (REG_BINARY)
                        byte[] affinityMaskBytes = BitConverter.GetBytes(affinityMask);

                        // Set the "AssignmentSetOverride" registry value as REG_BINARY
                        affinityPolicyKey.SetValue("AssignmentSetOverride", affinityMaskBytes, RegistryValueKind.Binary);

                        // Set the "DevicePolicy" registry value as DWORD with value 4 (Hex: 4)
                        affinityPolicyKey.SetValue("DevicePolicy", 4, RegistryValueKind.DWord);

                        // Show the selected CPU affinity in binary format (or a user-friendly format)
                        string selectedCoresStr = string.Join(", ", selectedCores);
                        MessageBox.Show($"CPU Affinity for device '{device.DeviceName}' has been set to cores: {selectedCoresStr}.\n" +
                                        $"DevicePolicy set to 0x4 (DWORD).");
                    }
                    else
                    {
                        MessageBox.Show("Unable to access or create 'Affinity Policy' key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Unauthorized access: please run as administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating CPU Affinity: {ex.Message}");
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

            var modes = "LB";  // Default mode
            if (supportsMsix)
            {
                modes += ", MSI-X";  // If MSI-X is supported, only show MSI-X
            }
            else if (supportsMsi)
            {
                modes += ", MSI";  // If MSI-X is not supported but MSI is, show MSI
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

                if (device != null && int.TryParse(device.MessageNumberLimit, out int newLimit) && newLimit >= 1 && newLimit <= 900000000)
                {
                    device.UpdateLimitInRegistry(newLimit);
                }
                else
                {
                    MessageBox.Show("Please enter a valid limit value (1 - 9000000).");
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


