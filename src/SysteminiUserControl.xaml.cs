using System;
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
woafont=dosapp.fon
EGA80WOA.FON=EGA80WOA.FON
EGA40WOA.FON=EGA40WOA.FON
CGA80WOA.FON=CGA80WOA.FON
CGA40WOA.FON=CGA40WOA.FON

[drivers]
wave=mmdrv.dll
timer=timer.drv

[mci]";

        public SysteminiUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "System.ini";
            LoadSystemIniContents(); // Load the system.ini contents when the UserControl is initialized
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
            ExperimentalTextBox.Text = profile1; // Experimental profile loaded in Experimental tab
        }

        // Method to apply the profile (write the content back to system.ini)
        private void ApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Write the contents of the active TextBox to the system.ini file
                if (VerifiedContent.Visibility == Visibility.Visible)
                    File.WriteAllText(SystemIniPath, SystemIniTextBox.Text); // Write to system.ini from the Verified tab
                else if (ExperimentalContent.Visibility == Visibility.Visible)
                    File.WriteAllText(SystemIniPath, ExperimentalTextBox.Text); // Write to system.ini from the Experimental tab
                else
                    File.WriteAllText(SystemIniPath, CurrentTextBox.Text); // Write to system.ini from the Current tab

                MessageBox.Show("Profile applied successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the System.ini file. Please run the application as an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to system.ini: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
