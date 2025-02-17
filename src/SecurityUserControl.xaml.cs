using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;


namespace NZTS_App.Views
{
    public partial class SecurityUserControl : UserControl
    {
        private const string HypervisorKeyPath = @"SYSTEM\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";
        private MainWindow mainWindow;

        public SecurityUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Security";

            // Temporarily detach event to avoid premature triggering during initialization
            CoreIsolationToggle.Click -= CoreIsolationToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            CoreIsolationToggle.Click += CoreIsolationToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load HypervisorEnforcedCodeIntegrity Enabled value
                using (var key = Registry.LocalMachine.CreateSubKey(HypervisorKeyPath))
                {
                    if (key != null)
                    {
                        var enabledValue = key.GetValue("Enabled");
                        if (enabledValue == null) // Key does not exist
                        {
                            key.SetValue("Enabled", 0, RegistryValueKind.DWord); // Default value
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
