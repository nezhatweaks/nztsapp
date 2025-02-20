using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;



namespace NZTS_App.Views
{
    public partial class SecurityUserControl : UserControl
    {
        private const string HypervisorKeyPath = @"SYSTEM\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";
        private const string MdmCommonKeyPath = @"SOFTWARE\Microsoft\MdmCommon\Internal";
        private const string MdmCommonKeyPathTrue = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MdmCommon\Internal";

        private MainWindow mainWindow;

        public SecurityUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Security";

            // Temporarily detach event to avoid premature triggering during initialization
            CoreIsolationToggle.Click -= CoreIsolationToggle_Click;
            ProtectionToggle.Click -= ProtectionToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            CoreIsolationToggle.Click += CoreIsolationToggle_Click;
            ProtectionToggle.Click += ProtectionToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load HypervisorEnforcedCodeIntegrity Enabled value (Core Isolation)
                using (var hypervisorKey = Registry.LocalMachine.CreateSubKey(HypervisorKeyPath))
                {
                    if (hypervisorKey != null)
                    {
                        var enabledValue = hypervisorKey.GetValue("Enabled");
                        if (enabledValue == null) // Key does not exist
                        {
                            hypervisorKey.SetValue("Enabled", 0, RegistryValueKind.DWord); // Default value
                            CoreIsolationToggle.IsChecked = false; // Default state
                        }
                        else
                        {
                            CoreIsolationToggle.IsChecked = (enabledValue is int enabledInt && enabledInt == 1);
                        }
                    }
                    else
                    {
                        ShowError("Failed to access Hypervisor registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Hypervisor registry key not found.");
                    }
                }

                // Load ProtectionSessionUpdateFrequencyInMinutes value (MdmCommon settings)
                using (var mdmCommonKey = Registry.LocalMachine.CreateSubKey(MdmCommonKeyPath))
                {
                    if (mdmCommonKey != null)
                    {
                        var sessionUpdateValue = mdmCommonKey.GetValue("ProtectionSessionUpdateFrequencyInMinutes");
                        if (sessionUpdateValue == null) // Key does not exist
                        {
                            mdmCommonKey.SetValue("ProtectionSessionUpdateFrequencyInMinutes", 60, RegistryValueKind.DWord); // Default value
                            ProtectionToggle.IsChecked = false; // Default state (assuming 60 minutes means toggled off)
                        }
                        else
                        {
                            // If the value is 99999999, it means the toggle is in "unlimited" mode
                            ProtectionToggle.IsChecked = (sessionUpdateValue is int sessionUpdateInt && sessionUpdateInt == 99999999);
                        }
                    }
                    else
                    {
                        ShowError("Failed to access MDM Common registry key.");
                        App.changelogUserControl?.AddLog("Failed", "MdmCommon registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                
                App.changelogUserControl?.AddLog("Warning", "Unauthorized access on ones of Security Tweaks.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error loading settings: {ex.Message}");
            }
        }

        private void OpenRegistryPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure the registry path is valid and properly formatted with the root key
                if (!string.IsNullOrEmpty(MdmCommonKeyPathTrue) && MdmCommonKeyPathTrue.StartsWith(@"HKEY"))
                {
                    // Copy the path to the clipboard
                    Clipboard.SetText(MdmCommonKeyPathTrue);
                    
                }
                else
                {
                    ShowError("Invalid registry path.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error copying to clipboard: {ex.Message}");
            }
        }






        private void CoreIsolationToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(HypervisorKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("Enabled", CoreIsolationToggle.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"Core Isolation set to {(CoreIsolationToggle.IsChecked == true ? "Enabled" : "Disabled")}");
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
                ShowError($"Error updating Core Isolation: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Core Isolation: {ex.Message}");
            }
        }

        private void ProtectionToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(MdmCommonKeyPath))
                {
                    if (key != null)
                    {
                        // Set the ProtectionSessionUpdateFrequencyInMinutes value in the registry
                        key.SetValue("ProtectionSessionUpdateFrequencyInMinutes", ProtectionToggle.IsChecked == true ? 99999999 : 60, RegistryValueKind.DWord);

                        // Mark settings as applied
                        mainWindow?.MarkSettingsApplied();

                        // Log the change with the correct description
                        App.changelogUserControl?.AddLog("Applied", $"Protection Session Update Frequency set to {(ProtectionToggle.IsChecked == true ? "Unlimited" : "60 minutes")}");
                    }
                    else
                    {
                        // Show error and log failure
                        ShowError("Failed to access Hypervisor registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Hypervisor registry key not found.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle permission error and log failure
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Protection Session Update Frequency.");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions and log the error
                ShowError($"Error updating Protection Session Update Frequency: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Protection Session Update Frequency: {ex.Message}");
            }
        }


        private void DefButton_Click(object sender, RoutedEventArgs e)
        {
            // PowerShell script to be copied to clipboard
            string script = @"
$remove_appx = @('SecHealthUI'); 
$provisioned = get-appxprovisionedpackage -online; 
$appxpackage = get-appxpackage -allusers; 
$eol = @()
$store = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore'
$users = @('S-1-5-18'); 
if (test-path $store) {
    $users += $((dir $store -ea 0 |where {$_ -like '*S-1-5-21*'}).PSChildName)
}
foreach ($choice in $remove_appx) { 
    if ('' -eq $choice.Trim()) {continue}
    foreach ($appx in $($provisioned |where {$_.PackageName -like '*$choice*'})) {
        $next = !1; 
        foreach ($no in $skip) { 
            if ($appx.PackageName -like '*$no*') {
                $next = !0
            } 
        } 
        if ($next) {continue}
        $PackageName = $appx.PackageName; 
        $PackageFamilyName = ($appxpackage |where {$_.Name -eq $appx.DisplayName}).PackageFamilyName 
        ni '$store\Deprovisioned\$PackageFamilyName' -force >''; $PackageFamilyName  
        foreach ($sid in $users) { 
            ni '$store\EndOfLife\$sid\$PackageName' -force >'' 
        } 
        $eol += $PackageName
        dism /online /set-nonremovableapppolicy /packagefamily:$PackageFamilyName /nonremovable:0 >''
        remove-appxprovisionedpackage -packagename $PackageName -online -allusers >''
    }
    foreach ($appx in $($appxpackage |where {$_.PackageFullName -like '*$choice*'})) {
        $next = !1; 
        foreach ($no in $skip) { 
            if ($appx.PackageFullName -like '*$no*') { 
                $next = !0
            } 
        } 
        if ($next) {continue}
        $PackageFullName = $appx.PackageFullName; 
        ni '$store\Deprovisioned\$appx.PackageFamilyName' -force >''; $PackageFullName
        foreach ($sid in $users) { 
            ni '$store\EndOfLife\$sid\$PackageFullName' -force >'' 
        } 
        $eol += $PackageFullName
        dism /online /set-nonremovableapppolicy /packagefamily:$PackageFamilyName /nonremovable:0 >''
        remove-appxpackage -package $PackageFullName -allusers >''
    }
}
";

            // Set the clipboard content
            Clipboard.SetText(script);
        }



        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
