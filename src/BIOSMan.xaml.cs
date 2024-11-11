using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hardware.Info;

namespace NZTS_App.Views
{
    public partial class BIOSMan : UserControl
    {
        private MainWindow mainWindow;
        private IHardwareInfo hardwareInfo;

        public BIOSMan(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            // Initialize the HardwareInfo instance
            hardwareInfo = new HardwareInfo();

            // Start monitoring hardware information asynchronously
            LoadHardwareInfoAsync();

            mainWindow.TitleTextBlock.Content = "HW";
        }

        private async void LoadHardwareInfoAsync()
        {
            try
            {
                // Show the loading indicator
                ShowLoadingIndicator(true);

                // Refresh hardware information asynchronously
                await Task.Run(() => hardwareInfo.RefreshAll());

                // Load all hardware information after refresh
                await LoadBIOSInfoAsync();
                await LoadProcessorInfoAsync();
                await LoadDRAMInfoAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing hardware information: {ex.Message}");
            }
            finally
            {
                // Hide the loading indicator when done
                ShowLoadingIndicator(false);
            }
        }

        private async Task LoadBIOSInfoAsync()
        {
            try
            {
                if (hardwareInfo.BiosList.Count > 0)
                {
                    var bios = hardwareInfo.BiosList[0];
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ManufacturerTextBlock.Text = bios.Manufacturer ?? "N/A";
                        VersionTextBlock.Text = bios.Version ?? "N/A";
                        SerialNumberTextBlock.Text = bios.SerialNumber ?? "N/A";
                        ReleaseDateTextBlock.Text = string.IsNullOrEmpty(bios.ReleaseDate)
                            ? "N/A"
                            : bios.ReleaseDate;
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing BIOS information: {ex.Message}");
            }
        }

        private async Task LoadProcessorInfoAsync()
        {
            try
            {
                if (hardwareInfo.CpuList.Count > 0)
                {
                    var cpu = hardwareInfo.CpuList[0];
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ProcessorSpeedTextBlock.Text = $"Base Speed: {cpu.MaxClockSpeed} MHz\nCurrent Speed: {cpu.CurrentClockSpeed} MHz";
                    });

                    // Loop through each CPU core (if needed)
                    foreach (var cpuCore in cpu.CpuCoreList)
                    {
                        // Handle individual CPU core data
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing CPU information: {ex.Message}");
            }
        }

        private async Task LoadDRAMInfoAsync()
        {
            try
            {
                if (hardwareInfo.MemoryList.Count > 0)
                {
                    var memory = hardwareInfo.MemoryList[0];

                    await Dispatcher.InvokeAsync(() =>
                    {
                        DRAMSpeedTextBlock.Text = $"Default Speed: {memory.Speed} MHz";

                        // Display additional memory information
                        if (!string.IsNullOrEmpty(memory.Manufacturer))
                            DRAMSpeedTextBlock.Text += $"\nManufacturer: {memory.Manufacturer}";

                        if (!string.IsNullOrEmpty(memory.PartNumber))
                            DRAMSpeedTextBlock.Text += $"\nPart Number: {memory.PartNumber}";

                        if (!string.IsNullOrEmpty(memory.SerialNumber))
                            DRAMSpeedTextBlock.Text += $"\nSerial Number: {memory.SerialNumber}";

                        if (!string.IsNullOrEmpty(memory.BankLabel))
                            DRAMSpeedTextBlock.Text += $"\nBank Label: {memory.BankLabel}";

                        if (memory.MinVoltage > 0 && memory.MaxVoltage > 0)
                            DRAMSpeedTextBlock.Text += $"\nVoltage: {memory.MinVoltage}V - {memory.MaxVoltage}V";
                    });
                }
                else
                {
                    DRAMSpeedTextBlock.Text = "No memory information available";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error accessing DRAM information: {ex.Message}");
            }
        }

        // Show or hide the loading indicator
        private void ShowLoadingIndicator(bool isVisible)
        {
            LoadingIndicator.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
