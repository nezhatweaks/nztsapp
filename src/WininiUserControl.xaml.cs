using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class WininiUserControl : UserControl
    {
        private readonly MainWindow mainWindow;
        private const string WinIniPath = @"C:\Windows\win.ini";  // Path to the win.ini file

        // Default profiles
        private string defaultProfile = @"
; for 16-bit app support
[fonts]
[extensions]
[mci extensions]
[files]
[Mail]
MAPI=1
";

        private string profile1 = @"
; for 16-bit app support
[386Enh]
MinTimeSlice=1
AvgTimeSlice=1
MaxTimeSlice=1
WinTimeSlice=1,1
NetAsyncTimeout=-1
SyncTimeDivisor=1
TimeWindowMinutes=0
Latency=1
SampleRate=1
UseHWTimeStamp=1
Chunksize=1000000
[fonts]
[extensions]
[mci extensions]
NoHWAccel=1
[files]
[Mail]
MAPI=1
[timer]
TimeSliceUpdateTickCount=1
[NonWindowsApp]
MouseExclusive=1  ";

        public WininiUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Win.ini";
            LoadWinIniContents(); // Load the win.ini contents when the UserControl is initialized
            ApplyProfileButton.Visibility = Visibility.Collapsed; // Initially hide Apply Profile button
        }

        // Method to load the contents of the win.ini file
        private void LoadWinIniContents()
        {
            try
            {
                // Check if the win.ini file exists
                if (File.Exists(WinIniPath))
                {
                    // Read the current contents of the win.ini file
                    string fileContent = File.ReadAllText(WinIniPath);
                    CurrentTextBox.Text = fileContent; // Display the contents in the "Current" TextBox
                }
                else
                {
                    MessageBox.Show("Win.ini file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentTextBox.Text = defaultProfile; // Display default profile if the file is not found
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading win.ini: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentTextBox.Text = defaultProfile; // Default profile if error occurs
            }
        }

        // Method to switch to the Current tab (loads the actual win.ini content)
        private void SwitchToCurrentTab(object sender, RoutedEventArgs e)
        {
            CurrentButton.Tag = "Active";
            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Inactive";
            CurrentContent.Visibility = Visibility.Visible;
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            ApplyProfileButton.Visibility = Visibility.Collapsed;

            LoadWinIniContents(); // Load the actual win.ini content in the Current tab
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

            ApplyProfileButton.Visibility = Visibility.Visible;

            ExperimentalTextBox.Text = profile1; // Experimental profile loaded in Experimental tab
        }

        // Method to apply the profile (write the content back to win.ini)
        private void ApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            string profileApplied = string.Empty; // Variable to hold the name of the profile applied
            try
            {
                // Determine which profile is being applied
                if (VerifiedContent.Visibility == Visibility.Visible)
                {
                    File.WriteAllText(WinIniPath, SystemIniTextBox.Text); // Write to win.ini from the Verified tab
                    profileApplied = "Default Profile";
                }
                else if (ExperimentalContent.Visibility == Visibility.Visible)
                {
                    File.WriteAllText(WinIniPath, ExperimentalTextBox.Text); // Write to win.ini from the Experimental tab
                    profileApplied = "Tweaked Profile";
                }
                else
                {
                    File.WriteAllText(WinIniPath, CurrentTextBox.Text); // Write to win.ini from the Current tab
                    profileApplied = "Current Profile";
                }

                MessageBox.Show("Profile applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Log the applied profile
                App.changelogUserControl?.AddLog("Applied", $"{profileApplied} has been modified.");
                mainWindow?.MarkSettingsApplied(); // Mark the setting as applied
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the Win.ini file. Please run the application as an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log the failure with profile name
                App.changelogUserControl?.AddLog("Failed", $"{profileApplied} failed to be modified.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to win.ini: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Log the failure with profile name
                App.changelogUserControl?.AddLog("Failed", $"{profileApplied} failed to be modified.");
            }
        }
    }
}
