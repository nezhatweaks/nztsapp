using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NZTS_App.Properties;

namespace NZTS_App.Views
{
    public partial class SettingsUserControl : UserControl
    {
        

        public SettingsUserControl()
        {
            InitializeComponent();
            DisplayCurrentVersion(); // Display the current version when the control is initialized

        }

        private const string LatestVersionUrl = "https://raw.githubusercontent.com/nezhatweaks/nztsapp/refs/heads/main/Assets/latest_version.txt"; // Replace with your actual URL
        

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates();
        }

        private void DisplayCurrentVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentVersion = assembly?.GetName().Version?.ToString() ?? "unknown version";
            VersionTextBlock.Text = $"NZTS App v{currentVersion}"; // Set the version text
        }


        private async Task CheckForUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Get the latest version number from the text file
                    string latestVersion = await client.GetStringAsync(LatestVersionUrl);
                    latestVersion = latestVersion.Trim(); // Remove any whitespace/newline

                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var currentVersion = assembly?.GetName().Version?.ToString() ?? "unknown version";

                    if (currentVersion != latestVersion)
                    {
                        var result = MessageBox.Show($"A new version {latestVersion} is available! Would you like to download it?",
                                                      "Update Available",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Dynamically create the download URL based on the latest version
                            string downloadUrl = $"https://github.com/nezhatweaks/nztsapp/releases/download/v{latestVersion}/NZTS_App_v{latestVersion}.zip";

                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = downloadUrl,
                                UseShellExecute = true // This will open the default browser to download the installer
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("You are using the latest version.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




    }
}
