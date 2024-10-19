using System;
using System.IO;
using System.IO.Compression; // Ensure you have the System.IO.Compression reference
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

        private const string LatestVersionUrl = "https://raw.githubusercontent.com/nezhatweaks/nztsapp/refs/heads/main/Assets/latest_version.txt";
        private const string NewZipUrlTemplate = "https://github.com/nezhatweaks/nztsapp/releases/download/v{0}/NZTS_APP_v{0}.zip"; // Template for the versioned ZIP URL

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
                    string latestVersionStr = await client.GetStringAsync(LatestVersionUrl);
                    latestVersionStr = latestVersionStr.Trim();

                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var assemblyName = assembly.GetName();
                    string currentVersionStr = assemblyName?.Version?.ToString() ?? "0.0.0.0";

                    Version latestVersion = new Version(latestVersionStr);
                    Version currentVersion = new Version(currentVersionStr);

                    if (currentVersion < latestVersion)
                    {
                        string? appDirectory = Path.GetDirectoryName(assembly.Location);
                        if (appDirectory == null)
                        {
                            MessageBox.Show("Application directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Current executable name
                        string currentExecutablePath = Path.Combine(appDirectory, "NZTS APP.exe");
                        string newZipPath = Path.Combine(appDirectory, $"NZTS_APP_v{latestVersionStr}.zip");

                        if (File.Exists(currentExecutablePath))
                        {
                            string backupDirectory = Path.Combine(appDirectory, "Backup");
                            Directory.CreateDirectory(backupDirectory);

                            // Backup the current executable
                            string backupExecutablePath = Path.Combine(backupDirectory, $"NZTS_APP_backup_{DateTime.Now:yyyyMMddHHmmss}.exe");
                            File.Copy(currentExecutablePath, backupExecutablePath, true);

                            // Download the versioned ZIP file
                            byte[] zipFileData = await client.GetByteArrayAsync(string.Format(NewZipUrlTemplate, latestVersionStr));
                            File.WriteAllBytes(newZipPath, zipFileData);

                            // Extract the ZIP file
                            ZipFile.ExtractToDirectory(newZipPath, appDirectory, true); // true to overwrite existing files

                            // Delete the current version executable
                            File.Delete(currentExecutablePath); // Delete NZTS APP.exe

                            // Optionally delete the ZIP file after extraction
                            File.Delete(newZipPath);

                            MessageBox.Show("Update successful! Please restart the application.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            // Log or ignore the absence of the current executable
                            Console.WriteLine("Current version executable not found, proceeding without backup.");
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
