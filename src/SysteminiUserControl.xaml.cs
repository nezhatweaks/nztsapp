using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class SysteminiUserControl : UserControl
    {
        private readonly MainWindow mainWindow;
        private const string SystemIniPath = @"C:\Windows\System.ini";  // Path to the system.ini file

        // Default profiles
        private string defaultProfile = @"
; for 16-bit app support
[386Enh]
woafont=dosapp.fon
EGA80WOA.FON=EGA80WOA.FON
EGA40WOA.FON=EGA40WOA.FON
CGA80WOA.FON=CGA80WOA.FON
CGA40WOA.FON=CGA40WOA.FON

[drivers]
wave=mmdrv.dll
timer=timer.drv

[mci]";

        private string profile1 = @"

; for 16-bit app support
[386Enh]
MinTimeSlice=1
AvgTimeSlice=1
MaxTimeSlice=1
WinTimeSlice=1,1
NetAsyncTimeout=0
SyncTimeDivisor=1
TimeWindowMinutes=0
Latency=1
SampleRate=1
UseHWTimeStamp=1
Auto-Detect-CPU=TRUE
CpuSnooze=0
MaxBiosPipes=128
MinBiosPipes=128
DoubleBuffer=0
UseHWTimeStamp=1
Chunksize=5000000
LoadTop=0
SystemReg=0
FastBlt=1
woafont=dosapp.fon
EGA80WOA.FON=EGA80WOA.FON
EGA40WOA.FON=EGA40WOA.FON
CGA80WOA.FON=CGA80WOA.FON
CGA40WOA.FON=CGA40WOA.FON

[drivers]
wave=mmdrv.dll
timer=timer.drv

[mci]
mciwave=mmsystem.dll

[timer]
TimeSliceUpdateTickCount=1

[NonWindowsApp]
MouseExclusive=1  ";

        public SysteminiUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "System.ini";
            LoadSystemIniContents(); // Load the system.ini contents when the UserControl is initialized
                                     // Set the Apply Profile button to be hidden initially if the Current tab is visible
            ApplyProfileButton.Visibility = Visibility.Collapsed;
        }

        // Method to load the contents of the system.ini file
        private void LoadSystemIniContents()
        {
            try
            {
                // Check if the system.ini file exists
                if (File.Exists(SystemIniPath))
                {
                    // Read the current contents of the system.ini file
                    string fileContent = File.ReadAllText(SystemIniPath);
                    CurrentTextBox.Text = fileContent; // Display the contents in the "Current" TextBox
                }
                else
                {
                    MessageBox.Show("System.ini file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentTextBox.Text = defaultProfile; // Display default profile if the file is not found
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading system.ini: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentTextBox.Text = defaultProfile; // Default profile if error occurs
            }
        }

        // Method to switch to the Current tab (loads the actual system.ini content)
        private void SwitchToCurrentTab(object sender, RoutedEventArgs e)
        {
            CurrentButton.Tag = "Active";
            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Inactive";
            CurrentContent.Visibility = Visibility.Visible;
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            // Hide the Apply Profile button when the Current tab is visible
            ApplyProfileButton.Visibility = Visibility.Collapsed;

            LoadSystemIniContents(); // Load the actual system.ini content in the Current tab
        }

        // Method to switch to the Verified tab (loads the default profile)
        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            CurrentButton.Tag = "Inactive";
            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
            CurrentContent.Visibility = Visibility.Collapsed;
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            // Show the Apply Profile button when the Verified tab is visible
            ApplyProfileButton.Visibility = Visibility.Visible;

            SystemIniTextBox.Text = defaultProfile; // Default profile loaded in Verified tab
        }

        // Method to switch to the Experimental tab (loads profile1)
        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            CurrentButton.Tag = "Inactive";
            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Active";
            CurrentContent.Visibility = Visibility.Collapsed;
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            // Show the Apply Profile button when the Experimental tab is visible
            ApplyProfileButton.Visibility = Visibility.Visible;

            ExperimentalTextBox.Text = profile1; // Experimental profile loaded in Experimental tab
        }



        // Method to apply the profile (write the content back to system.ini)
        private void ApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            string profileApplied = string.Empty; // Variable to hold the name of the profile applied
            try
            {
                // Determine which profile is being applied
                if (VerifiedContent.Visibility == Visibility.Visible)
                {
                    File.WriteAllText(SystemIniPath, SystemIniTextBox.Text); // Write to system.ini from the Verified tab
                    profileApplied = "Default Profile";
                }
                else if (ExperimentalContent.Visibility == Visibility.Visible)
                {
                    File.WriteAllText(SystemIniPath, ExperimentalTextBox.Text); // Write to system.ini from the Experimental tab
                    profileApplied = "Tweaked Profile";
                }
                else
                {
                    File.WriteAllText(SystemIniPath, CurrentTextBox.Text); // Write to system.ini from the Current tab
                    profileApplied = "Current Profile";
                }

                MessageBox.Show("Profile applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Log the applied profile
                App.changelogUserControl?.AddLog("Applied", $"{profileApplied} has been modified.");
                mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the System.ini file. Please run the application as an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log the failure with profile name
                App.changelogUserControl?.AddLog("Failed", $"{profileApplied} failed to be modified.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to system.ini: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log the failure with profile name
                App.changelogUserControl?.AddLog("Failed", $"{profileApplied} failed to be modified.");
            }
        }

    }
}
