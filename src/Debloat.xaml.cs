using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;


namespace NZTS_App.Views
{
    public partial class Debloat : UserControl
    {
        private MainWindow mainWindow;

        public Debloat(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Telemetry";

            // Temporarily detach events to avoid premature triggering during initialization
            WsqmconsToggle.Click -= WsqmconsToggle_Click;
            CompattelToggle.Click -= CompattelToggle_Click;
            ElevationServiceToggle.Click -= ElevationServiceToggle_Click;
            DeviceCensusToggle.Click -= DeviceCensusToggle_Click;
            GameBarPresenceToggle.Click -= GameBarPresenceToggle_Click;
            MusNotificationToggle.Click -= MusNotificationToggle_Click;
            WwahostToggle.Click -= WwahostToggle_Click;
            // Detach new events
            MspmsnsvToggle.Click -= MspmsnsvToggle_Click;
            WMIADAPToggle.Click -= WMIADAPToggle_Click;
            SecurityHealthToggle.Click -= SecurityHealthToggle_Click;
            NisSrvToggle.Click -= NisSrvToggle_Click;
            WscSvcToggle.Click -= WscSvcToggle_Click;
            MsAntiMalwareServiceToggle.Click -= MsAntiMalwareServiceToggle_Click;
            CscServiceToggle.Click -= CscServiceToggle_Click;
            // Detach new events for additional processes
            TaskHostwToggle.Click -= TaskHostwToggle_Click;
            MoUsoCoreWorkerToggle.Click -= MoUsoCoreWorkerToggle_Click;
            WidgetsToggle.Click -= WidgetsToggle_Click;
            WidgetServiceToggle.Click -= WidgetServiceToggle_Click;
            PerfWatson2Toggle.Click -= PerfWatson2Toggle_Click;
            StandardCollectorServiceToggle.Click -= StandardCollectorServiceToggle_Click;
            AggregatorHostToggle.Click -= AggregatorHostToggle_Click;

            // Detach event handlers for additional processes
           
            PhoneExperienceHostToggle.Click -= PhoneExperienceHostToggle_Click;
            CrossDeviceServiceToggle.Click -= CrossDeviceServiceToggle_Click;
            BackgroundTaskHostToggle.Click -= BackgroundTaskHostToggle_Click;
            SpeechRuntimeToggle.Click -= SpeechRuntimeToggle_Click;
            SecurityHealthSystrayToggle.Click -= SecurityHealthSystrayToggle_Click;
            // Detach new events for the additional processes
            WMIRegistrationServiceToggle.Click -= WMIRegistrationServiceToggle_Click;
            JhiServiceToggle.Click -= JhiServiceToggle_Click;
            
            IpfsvcToggle.Click -= IpfsvcToggle_Click;
            IpfHelperToggle.Click -= IpfHelperToggle_Click;



            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            WsqmconsToggle.Click += WsqmconsToggle_Click;
            CompattelToggle.Click += CompattelToggle_Click;
            ElevationServiceToggle.Click += ElevationServiceToggle_Click;
            DeviceCensusToggle.Click += DeviceCensusToggle_Click;
            GameBarPresenceToggle.Click += GameBarPresenceToggle_Click;
            MusNotificationToggle.Click += MusNotificationToggle_Click;
            WwahostToggle.Click += WwahostToggle_Click;
            // Attach new events
            MspmsnsvToggle.Click += MspmsnsvToggle_Click;
            WMIADAPToggle.Click += WMIADAPToggle_Click;
            SecurityHealthToggle.Click += SecurityHealthToggle_Click;
            NisSrvToggle.Click += NisSrvToggle_Click;
            WscSvcToggle.Click += WscSvcToggle_Click;
            MsAntiMalwareServiceToggle.Click += MsAntiMalwareServiceToggle_Click;
            CscServiceToggle.Click += CscServiceToggle_Click;
            // Attach new events for additional processes
            TaskHostwToggle.Click += TaskHostwToggle_Click;
            MoUsoCoreWorkerToggle.Click += MoUsoCoreWorkerToggle_Click;
            WidgetsToggle.Click += WidgetsToggle_Click;
            WidgetServiceToggle.Click += WidgetServiceToggle_Click;
            PerfWatson2Toggle.Click += PerfWatson2Toggle_Click;
            StandardCollectorServiceToggle.Click += StandardCollectorServiceToggle_Click;
            AggregatorHostToggle.Click += AggregatorHostToggle_Click;
            PhoneExperienceHostToggle.Click += PhoneExperienceHostToggle_Click;
            CrossDeviceServiceToggle.Click += CrossDeviceServiceToggle_Click;
            BackgroundTaskHostToggle.Click += BackgroundTaskHostToggle_Click;
            SpeechRuntimeToggle.Click += SpeechRuntimeToggle_Click;
            SecurityHealthSystrayToggle.Click += SecurityHealthSystrayToggle_Click;

            // Attach new event handlers for the additional processes
            WMIRegistrationServiceToggle.Click += WMIRegistrationServiceToggle_Click;
            JhiServiceToggle.Click += JhiServiceToggle_Click;
            IpfsvcToggle.Click += IpfsvcToggle_Click;
            IpfHelperToggle.Click += IpfHelperToggle_Click;


        }

        private void LoadCurrentSettings()
        {
            try
            {
                WsqmconsToggle.IsChecked = CheckProcessEnabled("wsqmcons.exe");
                CompattelToggle.IsChecked = CheckProcessEnabled("compattelrunner.exe");
                ElevationServiceToggle.IsChecked = CheckProcessEnabled("elevation_service.exe");
                DeviceCensusToggle.IsChecked = CheckProcessEnabled("devicecensus.exe");
                GameBarPresenceToggle.IsChecked = CheckProcessEnabled("gamebarpresencewriter.exe");
                MusNotificationToggle.IsChecked = CheckProcessEnabled("MusNotification.exe");
                WwahostToggle.IsChecked = CheckProcessEnabled("wwahost.exe");

                // Load new settings
                MspmsnsvToggle.IsChecked = CheckProcessEnabled("mspmsnsv.exe");
                WMIADAPToggle.IsChecked = CheckProcessEnabled("WMIADAP.exe");

                // New toggles
                SecurityHealthToggle.IsChecked = CheckProcessEnabled("SecurityHealth.exe");
                NisSrvToggle.IsChecked = CheckProcessEnabled("NisSrv.exe");
                WscSvcToggle.IsChecked = CheckProcessEnabled("WscSvc.exe");
                MsAntiMalwareServiceToggle.IsChecked = CheckProcessEnabled("MsAntiMalwareService.exe");
                CscServiceToggle.IsChecked = CheckProcessEnabled("CscService.exe");
                // Additional processes
                TaskHostwToggle.IsChecked = CheckProcessEnabled("taskhostw.exe");
                MoUsoCoreWorkerToggle.IsChecked = CheckProcessEnabled("MoUsoCoreWorker.exe");
                WidgetsToggle.IsChecked = CheckProcessEnabled("Widgets.exe");
                PerfWatson2Toggle.IsChecked = CheckProcessEnabled("PerfWatson2.exe");
                WidgetServiceToggle.IsChecked = CheckProcessEnabled("WidgetService.exe");
                StandardCollectorServiceToggle.IsChecked = CheckProcessEnabled("StandardCollector.Service.exe");
                AggregatorHostToggle.IsChecked = CheckProcessEnabled("AggregatorHost.exe");

                // Set IsChecked property based on process status
                
                PhoneExperienceHostToggle.IsChecked = CheckProcessEnabled("PhoneExperienceHost.exe");
                CrossDeviceServiceToggle.IsChecked = CheckProcessEnabled("CrossDeviceService.exe");
                BackgroundTaskHostToggle.IsChecked = CheckProcessEnabled("backgroundTaskHost.exe");
                SpeechRuntimeToggle.IsChecked = CheckProcessEnabled("SpeechRuntime.exe");
                SecurityHealthSystrayToggle.IsChecked = CheckProcessEnabled("SecurityHealthSystray.exe");

                // New settings for additional processes
                WMIRegistrationServiceToggle.IsChecked = CheckProcessEnabled("WMIRegistrationService.exe");
                JhiServiceToggle.IsChecked = CheckProcessEnabled("jhi_service.exe");
                
                IpfsvcToggle.IsChecked = CheckProcessEnabled("ipfsvc.exe");
                IpfHelperToggle.IsChecked = CheckProcessEnabled("ipf_helper.exe");



            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error loading settings: {ex.Message}");
            }
        }

        private bool CheckProcessEnabled(string processName)
        {
            string registryKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{processName}";
#pragma warning disable CA1416 // This call is platform-dependent
            using (var key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                // If the registry key exists, check for the debugger
                if (key != null)
                {
                    var debuggerValue = key.GetValue("Debugger");
                    return debuggerValue != null; // Process is enabled if a debugger is set
                }
            }

            return false; // Process is disabled if the registry key doesn't exist
        }



        // Add new event handlers
        private void MspmsnsvToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("mspmsnsv.exe", MspmsnsvToggle.IsChecked == true);
        }

        

        

        

        

        private void WMIADAPToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("WMIADAP.exe", WMIADAPToggle.IsChecked == true);
        }




        private void WsqmconsToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("wsqmcons.exe", WsqmconsToggle.IsChecked == true);
        }

        private void CompattelToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("compattelrunner.exe", CompattelToggle.IsChecked == true);
        }

        private void ElevationServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("elevation_service.exe", ElevationServiceToggle.IsChecked == true);
        }

        private void DeviceCensusToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("devicecensus.exe", DeviceCensusToggle.IsChecked == true);
        }

        private void GameBarPresenceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("gamebarpresencewriter.exe", GameBarPresenceToggle.IsChecked == true);
        }

        private void MusNotificationToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("MusNotification.exe", MusNotificationToggle.IsChecked == true);
        }

        private void WwahostToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("wwahost.exe", WwahostToggle.IsChecked == true);
        }

        

        private void SecurityHealthToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("SecurityHealth.exe", SecurityHealthToggle.IsChecked == true);
        }

        private void NisSrvToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("NisSrv.exe", NisSrvToggle.IsChecked == true);
        }

       

        private void WscSvcToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("WscSvc.exe", WscSvcToggle.IsChecked == true);
        }

        private void MsAntiMalwareServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("MsAntiMalwareService.exe", MsAntiMalwareServiceToggle.IsChecked == true);
        }

        

        private void CscServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("CscService.exe", CscServiceToggle.IsChecked == true);
        }

        // New event handlers for additional processes
        private void TaskHostwToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("taskhostw.exe", TaskHostwToggle.IsChecked == true);
        }

        private void MoUsoCoreWorkerToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("MoUsoCoreWorker.exe", MoUsoCoreWorkerToggle.IsChecked == true);
        }

        private void WidgetsToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("Widgets.exe", WidgetsToggle.IsChecked == true);
        }

        private void PerfWatson2Toggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("PerfWatson2.exe", PerfWatson2Toggle.IsChecked == true);
        }

        private void WidgetServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("WidgetService.exe", WidgetServiceToggle.IsChecked == true);
        }

        private void StandardCollectorServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("StandardCollector.Service.exe", StandardCollectorServiceToggle.IsChecked == true);
        }

        private void AggregatorHostToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("AggregatorHost.exe", AggregatorHostToggle.IsChecked == true);
        }

        

        private void PhoneExperienceHostToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("PhoneExperienceHost.exe", PhoneExperienceHostToggle.IsChecked == true);
        }

        private void CrossDeviceServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("CrossDeviceService.exe", CrossDeviceServiceToggle.IsChecked == true);
        }

        private void BackgroundTaskHostToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("backgroundTaskHost.exe", BackgroundTaskHostToggle.IsChecked == true);
        }

        private void SpeechRuntimeToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("SpeechRuntime.exe", SpeechRuntimeToggle.IsChecked == true);
        }

        private void SecurityHealthSystrayToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("SecurityHealthSystray.exe", SecurityHealthSystrayToggle.IsChecked == true);
        }

        private void WMIRegistrationServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("WMIRegistrationService.exe", WMIRegistrationServiceToggle.IsChecked == true);
        }

        private void JhiServiceToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("jhi_service.exe", JhiServiceToggle.IsChecked == true);
        }

        

        private void IpfsvcToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("ipfsvc.exe", IpfsvcToggle.IsChecked == true);
        }

        private void IpfHelperToggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleProcess("ipf_helper.exe", IpfHelperToggle.IsChecked == true);
        }

        




        private void ToggleProcess(string processName, bool isEnabled)
        {
            string registryKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{processName}";

            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(registryKeyPath))
                {
                    if (isEnabled) // When checked, add the debugger to disable the process
                    {
                        key?.SetValue("Debugger", @"C:\Windows\System32\systray.exe");
                        App.changelogUserControl?.AddLog("Applied", $"{processName} is now disabled.");
                        mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
                    }
                    else // When unchecked, remove the debugger to enable the process
                    {
                        if (key != null)
                        {
                            key.DeleteValue("Debugger", false);
                            App.changelogUserControl?.AddLog("Applied", $"{processName} is now enabled.");
                            mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access to modify {processName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error modifying {processName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error modifying {processName}: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
