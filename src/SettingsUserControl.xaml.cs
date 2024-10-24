using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq; // Make sure to install Newtonsoft.Json via NuGet

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

        // GitHub API URL for latest release
        private const string LatestReleaseUrl = "https://api.github.com/repos/nezhatweaks/nztsapp/releases/latest";
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
                string? latestVersionStr = await GetLatestVersion();
                if (string.IsNullOrEmpty(latestVersionStr))
                {
                    ShowErrorMessage("Failed to retrieve the latest version.");
                    return;
                }

                // Remove the 'v' prefix if present
                latestVersionStr = latestVersionStr.TrimStart('v');

                // Get current version
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string currentVersionStr = assembly.GetName().Version?.ToString() ?? "0.0.0.0";

                // Compare versions
                if (new Version(currentVersionStr) < new Version(latestVersionStr))
                {
                    // Prompt user for update
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


        private async Task<string?> GetLatestVersion()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "NZTS_App"); // Required by GitHub API
                HttpResponseMessage response = await client.GetAsync(LatestReleaseUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ShowErrorMessage($"Failed to retrieve latest release.\nStatus code: {response.StatusCode}\nReason: {response.ReasonPhrase}");
                    return null; // Explicitly return null on error
                }

                var json = await response.Content.ReadAsStringAsync();
                var release = JObject.Parse(json);
                return release["tag_name"]?.ToString(); // This will return something like "v1.0.0"
            }
        }


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

                MessageBox.Show("The application will now close to apply the update. Please restart it afterward.", "Updating...", MessageBoxButton.OK, MessageBoxImage.Information);

                // Path to UpdateExtractor.exe in the base directory
                string extractorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateExtractor.exe");

                // Check if UpdateExtractor.exe exists
                if (!File.Exists(extractorPath))
                {
                    ShowErrorMessage($"UpdateExtractor.exe not found at: {extractorPath}");
                    return;
                }

                var extractionProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = extractorPath,
                        Arguments = $"{tempFilePath} \"{AppDomain.CurrentDomain.BaseDirectory}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory // Set working directory to the base directory
                    }
                };

                extractionProcess.Start();

                string output = await Task.Run(() => extractionProcess.StandardOutput.ReadToEnd());
                string errorOutput = await Task.Run(() => extractionProcess.StandardError.ReadToEnd());
                await Task.Run(() => extractionProcess.WaitForExit());

                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine(output);
                }
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Console.WriteLine($"Error: {errorOutput}");
                }

                File.Delete(tempFilePath);
                Environment.Exit(0);
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









        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
