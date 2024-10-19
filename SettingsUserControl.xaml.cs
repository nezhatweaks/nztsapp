using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class SettingsUserControl : UserControl
    {
        public SettingsUserControl()
        {
            InitializeComponent();
            DisplayCurrentVersion();
        }

        // Corrected LatestVersion URL
        private const string LatestVersionUrl = "https://raw.githubusercontent.com/nezhatweaks/nztsapp/main/Assets/latest_version.txt";
        private const string NewZipUrlTemplate = "https://github.com/nezhatweaks/nztsapp/releases/download/v{0}/NZTS_APP_v{0}.zip";

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates();
        }

        private void DisplayCurrentVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentVersion = assembly?.GetName().Version?.ToString() ?? "unknown version";
            VersionTextBlock.Text = $"v{currentVersion}";
        }

        private async Task CheckForUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Get the latest version number from the text file
                    HttpResponseMessage response = await client.GetAsync(LatestVersionUrl);

                    // Check if the request was successful
                    if (!response.IsSuccessStatusCode)
                    {
                        // Log the exact error status for debugging
                        MessageBox.Show($"Failed to retrieve latest version. Status code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Read the response content and trim spaces
                    string latestVersionStr = await response.Content.ReadAsStringAsync();
                    latestVersionStr = latestVersionStr.Trim();

                    // Check if the latest version string is valid
                    if (string.IsNullOrEmpty(latestVersionStr))
                    {
                        MessageBox.Show("Failed to retrieve the latest version. The version information is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Get the current version of the running assembly
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var assemblyName = assembly.GetName();
                    string currentVersionStr = assemblyName?.Version?.ToString() ?? "0.0.0.0";

                    // Parse the versions
                    Version latestVersion = new Version(latestVersionStr);
                    Version currentVersion = new Version(currentVersionStr);

                    // Compare versions
                    if (currentVersion < latestVersion)
                    {
                        // Here you can place your logic to download and update
                        MessageBox.Show($"A new version (v{latestVersionStr}) is available! Please update.", "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("You are using the latest version.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Show detailed error for debugging
                MessageBox.Show($"Error checking for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
