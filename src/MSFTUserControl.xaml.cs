using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class MSFTUserControl : UserControl 
    {
        private const string MSFTKeyPath = @"SOFTWARE\Microsoft\Provisioning\CSPs\ConfigSourceCspFilter\MSFT";
        private MainWindow mainWindow;

        public MSFTUserControl(MainWindow window) 
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "MSFT";

            // Temporarily detach event to avoid premature triggering during initialization
            MSFTToggle.Click -= MSFTToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            MSFTToggle.Click += MSFTToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load MSFT Enabled value
                using (var key = Registry.LocalMachine.CreateSubKey(MSFTKeyPath))
                {
                    if (key != null)
                    {
                        var enabledValue = key.GetValue("MSFTEnabled");
                        if (enabledValue == null) // Key does not exist
                        {
                            key.SetValue("MSFTEnabled", 0, RegistryValueKind.DWord); // Default value
                            MSFTToggle.IsChecked = false; // Default state
                        }
                        else
                        {
                            MSFTToggle.IsChecked = (enabledValue is int enabledInt && enabledInt == 1);
                        }
                    }
                    else
                    {
                        ShowError("Failed to access MSFT registry key.");
                        App.changelogUserControl?.AddLog("Failed", "MSFT registry key not found.");
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

        private void MSFTToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(MSFTKeyPath))
                {
                    if (key != null)
                    {
                        // Logic for enabling or disabling
                        if (MSFTToggle.IsChecked == true)
                        {
                            // When toggled on, set the required registry values to enable features
                            key.SetValue("MSFTEnabled", 1, RegistryValueKind.DWord);
                            key.SetValue("accounts", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("albr", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("alljoynmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("appinstall", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("applocker", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("appmetadata", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("assignedaccess", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("backupmigraterestore", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("certificatestore", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("clientcertificateinstall", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("cmpolicyenterprise", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("customdeviceui", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("declaredconfiguration", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("defender", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("deviceinstanceservice", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicelock", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicemanageability", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicepackagesinfo", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicestatus", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("deviceupdate", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("diagnosticlog", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("dmclient", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("dmsessionactions", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseapn", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseassignedaccess", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisedataprotection", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisedesktopappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseext", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseextfilesystem", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisemodernappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("filesystem", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("firewall", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("globalexperience", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("healthattestation", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("inputlexicons", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("internetexplorer_bmr", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("knobs", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("maps", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("networkproxy", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("networkqospolicy", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("nodecache", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("passportforwork", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("policy", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("policymanager", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("printerprovisioning", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("provisioning", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("reboot", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotefind", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotelock", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotering", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotewipe", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("reporting", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("rootcatrustedcertificates", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("secureassessment", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("sharedpc", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("storage", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("surfacehub", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("unifiedwritefilter", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("update", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("vpn", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("vpnv2", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("wifi", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("win32appinventory", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowsadvancedthreatprotection", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowslicensing", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowssecurityauditing", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("wirednetwork", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            

                            App.changelogUserControl?.AddLog("Applied", "MSFT settings enabled.");
                        }
                        else
                        {
                            key.SetValue("MSFTEnabled", 0, RegistryValueKind.DWord);
                            key.SetValue("accounts", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("albr", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("alljoynmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("appinstall", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("applocker", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("appmetadata", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("assignedaccess", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("backupmigraterestore", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("certificatestore", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("clientcertificateinstall", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("cmpolicyenterprise", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("customdeviceui", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("declaredconfiguration", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("defender", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("deviceinstanceservice", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicelock", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicemanageability", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicepackagesinfo", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("devicestatus", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("deviceupdate", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("diagnosticlog", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("dmclient", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("dmsessionactions", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseapn", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseassignedaccess", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisedataprotection", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisedesktopappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseext", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterpriseextfilesystem", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("enterprisemodernappmanagement", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("filesystem", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("firewall", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("globalexperience", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("healthattestation", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("inputlexicons", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("internetexplorer_bmr", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("knobs", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("maps", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("networkproxy", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("networkqospolicy", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("nodecache", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("passportforwork", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("policy", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("policymanager", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("printerprovisioning", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("provisioning", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("reboot", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotefind", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotelock", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotering", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("remotewipe", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("reporting", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("rootcatrustedcertificates", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("secureassessment", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("sharedpc", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("storage", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("surfacehub", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("unifiedwritefilter", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("update", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("vpn", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("vpnv2", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("wifi", new byte[] { 0x20, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("win32appinventory", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowsadvancedthreatprotection", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowslicensing", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("windowssecurityauditing", new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                            key.SetValue("wirednetwork", new byte[] { 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);

                            App.changelogUserControl?.AddLog("Applied", "MSFT settings disabled.");
                        }

                        mainWindow?.MarkSettingsApplied();
                    }
                    else
                    {
                        ShowError("Failed to access Hypervisor registry key.");
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
                ShowError($"Error updating MSFT settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating MSFT settings: {ex.Message}");
            }
        }






        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
