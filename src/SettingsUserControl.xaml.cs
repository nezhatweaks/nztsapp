using Newtonsoft.Json.Linq; // Ensure you have this NuGet package installed
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class SettingsUserControl : UserControl
    {
        private MainWindow mainWindow;
        public SettingsUserControl(MainWindow window)
        {
            InitializeComponent();
            DisplayCurrentVersion();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Settings";
        }

        // GitHub API URL for the latest release
        private const string LatestReleaseUrl = "https://api.github.com/repos/nezhatweaks/nztsapp/releases/latest";
        private const string NewZipUrlTemplate = "https://github.com/nezhatweaks/nztsapp/releases/download/v{0}/NZTS_APP_v{0}.zip";

        // Check for updates when the button is clicked
        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates();
        }

        // Display current version of the app
        private void DisplayCurrentVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentVersion = assembly?.GetName().Version?.ToString() ?? "unknown version";
            VersionTextBlock.Text = $"v{currentVersion}";
        }

        // Check for available updates
        private async Task CheckForUpdates()
        {
            try
            {
                string? latestVersionStr = await GetLatestVersion();
                if (string.IsNullOrEmpty(latestVersionStr))
                {
                    ShowErrorMessage("Failed to retrieve the latest version.");
                    return;
                }

                // Remove the 'v' prefix if present
                latestVersionStr = latestVersionStr.TrimStart('v');

                // Get the current version of the app
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string currentVersionStr = assembly.GetName().Version?.ToString() ?? "0.0.0.0";

                // Compare versions
                if (new Version(currentVersionStr) < new Version(latestVersionStr))
                {
                    // Prompt user for an update
                    if (MessageBox.Show($"A new version (v{latestVersionStr}) is available! Would you like to download and update now?",
                                        "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        await DownloadAndApplyUpdate(latestVersionStr);
                    }
                }
                else
                {
                    MessageBox.Show("You are using the latest version.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error checking for updates: {ex.Message}");
            }
        }

        // Get the latest version from GitHub API
        private async Task<string?> GetLatestVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "NZTS_App"); // Add user-agent for GitHub API

                try
                {
                    // Send a GET request to the GitHub API
                    HttpResponseMessage response = await client.GetAsync(LatestReleaseUrl);

                    // Check if the response is not successful (i.e., status code is not in the range 200-299)
                    if (!response.IsSuccessStatusCode)
                    {
                        ShowErrorMessage($"Failed to retrieve latest release. Status code: {response.StatusCode}");
                        return null; // Return null if the status code indicates failure
                    }

                    // Read the response content as a string
                    var json = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to extract the release information
                    var release = JObject.Parse(json);

                    // Return the "tag_name" or null if it doesn't exist
                    return release["tag_name"]?.ToString();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that might occur (e.g., network issues)
                    ShowErrorMessage($"An error occurred while checking for updates: {ex.Message}");
                    return null; // Return null if there was an exception
                }
            }
        }


        // Download the update and apply it after closing the app
        private async Task DownloadAndApplyUpdate(string latestVersionStr)
        {
            try
            {
                string zipUrl = string.Format(NewZipUrlTemplate, latestVersionStr.TrimStart('v'));
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"NZTS_APP_v{latestVersionStr}.zip");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response = await client.GetAsync(zipUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        ShowErrorMessage($"Failed to download update.\nStatus code: {response.StatusCode}\nReason: {response.ReasonPhrase}");
                        return;
                    }
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                // Notify the user and prepare to close the application
                MessageBox.Show("The application will now close to apply the update. Please restart it afterward.", "Updating...", MessageBoxButton.OK, MessageBoxImage.Information);

                // Launch the extractor and pass the path to the ZIP file as an argument
                string updatesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updates");
                string extractorPath = Path.Combine(updatesFolderPath, "UpdateExtractor.exe");

                var extractionProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = extractorPath,
                        Arguments = tempFilePath,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                extractionProcess.Start();
                Environment.Exit(0); // Close the app to allow extraction to take place
            }
            catch (HttpRequestException httpEx)
            {
                ShowErrorMessage($"HTTP Error: {httpEx.Message}");
            }
            catch (IOException ioEx)
            {
                ShowErrorMessage($"File Error: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error applying the update: {ex.Message}");
            }
        }


        // Display an error message in a MessageBox
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
