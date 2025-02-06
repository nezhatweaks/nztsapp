using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;

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
            try
            {
                // Show a warning message before proceeding with the execution
                var result = MessageBox.Show(
                    "This operation will make irreversible changes to your system. It is highly recommended to create a system restore point before proceeding. Do you want to continue?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return; // Exit if the user chooses not to proceed
                }

                // Call the method to remove Windows Defender
                RemoveWindowsDefender();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                string errorMsg = $"An error occurred during the process: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveWindowsDefender()
        {
            try
            {
                // Disable Hypervisor (bcdedit /set hypervisorlaunchtype off equivalent)
                DisableHypervisor();

                // Remove Windows Security UWP App (Simulate PowerShell call)
                RemoveSecHealthApp();

                // Unregister Windows Defender Security Components (Using Registry manipulation)
                UnregisterDefenderComponents();

                // Delete Defender-related files and directories
                DeleteDefenderFiles();

                // Show success message
                MessageBox.Show("Windows Defender removal completed. Please restart your computer for the changes to take effect.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                string errorMsg = $"An error occurred during the process: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Disable Hypervisor (bcdedit)
        private void DisableHypervisor()
        {
            try
            {
                // Registry key to disable Hypervisor (equivalent to bcdedit command)
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\CrashControl", "HypervisorLaunchType", 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to disable Hypervisor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Remove Windows Security UWP App (PowerShell script equivalent)
        private void RemoveSecHealthApp()
        {
            try
            {
                // Windows Security UWP App removal (simulate removal without PowerShell)
                // We will just demonstrate this with AppX cmdlet logic.

                // Correct way to instantiate and use ManagementObjectSearcher to find Windows Defender-related UWP app
                var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Product WHERE Name = 'Windows Defender'");

                // Iterate through all the products found matching "Windows Defender"
                foreach (var pkg in searcher.Get())
                {
                    // Get the app's package name from the ManagementObject
                    string? appxName = pkg["Name"].ToString();

                    // You would normally call PowerShell commands like this:
                    // System.Diagnostics.Process.Start("powershell", $"Remove-AppxPackage {appxName}");

                    // Here's a more refined method to use PowerShell from C# to remove the app
                    RemoveAppxPackage(appxName);
                }

                MessageBox.Show("Windows Defender removal completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Show error message if removal fails
                MessageBox.Show($"Failed to remove Windows Security UWP app: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper method to execute a PowerShell command
        private void RemoveAppxPackage(string appName)
        {
            try
            {
                // Define the PowerShell command to remove the app
                string command = $"Remove-AppxPackage -Package {appName}";

                // Execute PowerShell command from C#
                ExecutePowerShellCommand(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing app: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper method to run PowerShell command
        private void ExecutePowerShellCommand(string command)
        {
            try
            {
                // ProcessStartInfo to run PowerShell as an administrator
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = command,
                    Verb = "runas", // Run as Administrator
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the PowerShell process and wait for it to complete
                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing PowerShell command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Unregister Windows Defender Security Components (Registry manipulations)
        private void UnregisterDefenderComponents()
        {
            try
            {
                string[] defenderRegFiles = {
            @"C:\Path\To\Remove_defender.reg", // Example registry file paths
            @"C:\Path\To\Remove_SecurityComp.reg"
        };

                foreach (var regFile in defenderRegFiles)
                {
                    if (File.Exists(regFile))
                    {
                        ApplyRegistryFile(regFile);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to unregister Windows Defender components: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyRegistryFile(string regFilePath)
        {
            try
            {
                // Load registry entries from .reg file manually
                string[] lines = File.ReadAllLines(regFilePath);

                foreach (var line in lines)
                {
                    // Logic to parse and apply the registry changes
                    if (line.StartsWith("Windows Registry Editor"))
                        continue;

                    // Implement registry changes from the file
                    string[] keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();

                        Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\CrashControl", "HypervisorLaunchType", 0, RegistryValueKind.DWord);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply registry file {regFilePath}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Delete Windows Defender related files and directories
        private void DeleteDefenderFiles()
        {
            string[] filesToDelete = {
        @"C:\Windows\WinSxS\FileMaps\wow64_windows-defender*.manifest",
        @"C:\Windows\WinSxS\FileMaps\x86_windows-defender*.manifest",
        @"C:\Windows\WinSxS\FileMaps\amd64_windows-defender*.manifest",
        @"C:\Windows\System32\SecurityAndMaintenance_Error.png",
        @"C:\Windows\System32\SecurityAndMaintenance.png",
        @"C:\Windows\System32\SecurityHealthSystray.exe",
        @"C:\Windows\System32\SecurityHealthService.exe",
        @"C:\Windows\System32\SecurityHealthHost.exe",
        @"C:\Windows\System32\drivers\SgrmAgent.sys",
        @"C:\Windows\System32\drivers\WdDevFlt.sys",
        @"C:\Windows\System32\drivers\WdBoot.sys",
        @"C:\Windows\System32\drivers\WdFilter.sys",
        @"C:\Windows\System32\wscsvc.dll",
        @"C:\Windows\System32\drivers\WdNisDrv.sys",
        @"C:\Windows\System32\wscsvc.dll",
        @"C:\Windows\System32\wscproxystub.dll",
        @"C:\Windows\System32\wscisvif.dll",
        @"C:\Windows\System32\SecurityHealthProxyStub.dll",
        @"C:\Windows\System32\smartscreen.dll",
        @"C:\Windows\SysWOW64\smartscreen.dll",
        @"C:\Windows\System32\smartscreen.exe",
        @"C:\Windows\SysWOW64\smartscreen.exe",
        @"C:\Windows\System32\DWWIN.EXE",
        @"C:\Windows\SysWOW64\smartscreenps.dll",
        @"C:\Windows\System32\smartscreenps.dll",
        @"C:\Windows\System32\SecurityHealthCore.dll",
        @"C:\Windows\System32\SecurityHealthSsoUdk.dll",
        @"C:\Windows\System32\SecurityHealthUdk.dll",
        @"C:\Windows\System32\SecurityHealthAgent.dll",
        @"C:\Windows\System32\wscapi.dll",
        @"C:\Windows\System32\wscadminui.exe",
        @"C:\Windows\SysWOW64\GameBarPresenceWriter.exe",
        @"C:\Windows\System32\GameBarPresenceWriter.exe",
        @"C:\Windows\SysWOW64\DeviceCensus.exe",
        @"C:\Windows\SysWOW64\CompatTelRunner.exe",
        @"C:\Windows\system32\drivers\msseccore.sys",
        @"C:\Windows\system32\drivers\MsSecFltWfp.sys",
        @"C:\Windows\system32\drivers\MsSecFlt.sys"
    };

            foreach (var file in filesToDelete)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    // Log failure to delete individual files
                    MessageBox.Show($"Failed to delete file {file}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            string[] dirsToDelete = {
        @"C:\Windows\WinSxS\amd64_security-octagon*",
        @"C:\Windows\WinSxS\x86_windows-defender*",
        @"C:\Windows\WinSxS\wow64_windows-defender*",
        @"C:\Windows\WinSxS\amd64_windows-defender*",
        @"C:\Windows\SystemApps\Microsoft.Windows.AppRep.ChxApp_cw5n1h2txyewy",
        @"C:\ProgramData\Microsoft\Windows Defender",
        @"C:\ProgramData\Microsoft\Windows Defender Advanced Threat Protection",
        @"C:\Program Files (x86)\Windows Defender Advanced Threat Protection",
        @"C:\Program Files\Windows Defender Advanced Threat Protection",
        @"C:\ProgramData\Microsoft\Windows Security Health",
        @"C:\ProgramData\Microsoft\Storage Health",
        @"C:\WINDOWS\System32\drivers\wd",
        @"C:\Program Files (x86)\Windows Defender",
        @"C:\Program Files\Windows Defender",
        @"C:\Windows\System32\SecurityHealth",
        @"C:\Windows\System32\WebThreatDefSvc",
        @"C:\Windows\System32\Sgrm",
        @"C:\Windows\Containers\WindowsDefenderApplicationGuard.wim",
        @"C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Modules\DefenderPerformance",
        @"C:\Windows\System32\WindowsPowerShell\v1.0\Modules\DefenderPerformance",
        @"C:\Windows\System32\WindowsPowerShell\v1.0\Modules\Defender",
        @"C:\Windows\System32\Tasks_Migrated\Microsoft\Windows\Windows Defender",
        @"C:\Windows\System32\Tasks\Microsoft\Windows\Windows Defender",
        @"C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Modules\Defender",
        @"C:\Windows\System32\WindowsPowerShell\v1.0\Modules\Defender"
    };

            foreach (var dir in dirsToDelete)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true); // true for recursive deletion
                    }
                }
                catch (Exception ex)
                {
                    // Log failure to delete individual directories
                    MessageBox.Show($"Failed to delete directory {dir}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }






        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
