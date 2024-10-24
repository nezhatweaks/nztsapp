using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class WindowsUserControl : UserControl
    {
        private const string GameModeKeyPath = @"Software\Microsoft\GameBar";
        private const string GameBarKeyPath = @"Software\Microsoft\Windows\CurrentVersion\GameDVR";        // Adjust path as needed
        private MainWindow mainWindow;

        public WindowsUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Gaming";

            // Temporarily detach event to avoid premature triggering during initialization
            GameModeToggle.Click -= GameModeToggle_Click;
            GameBarToggle.Click -= GameBarToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            GameModeToggle.Click += GameModeToggle_Click;
            GameBarToggle.Click += GameBarToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load Game Mode setting
                using (var key = Registry.CurrentUser.OpenSubKey(GameModeKeyPath))
                {
                    if (key != null)
                    {
                        var gameModeValue = key.GetValue("AutoGameModeEnabled");
                        GameModeToggle.IsChecked = (gameModeValue is int modeInt && modeInt == 1);
                        
                    }
                    else
                    {
                        ShowError("Failed to access Game Mode registry key.");
                    }
                }

                // Load Game Bar setting
                using (var key = Registry.CurrentUser.OpenSubKey(GameBarKeyPath))
                {
                    if (key != null)
                    {
                        var gameBarValue = key.GetValue("AppCaptureEnabled");
                        GameBarToggle.IsChecked = (gameBarValue is int barInt && barInt == 1);
                        
                    }
                    else
                    {
                        ShowError("Failed to access Game Bar registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
            }
        }


        private void GameModeToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GameModeKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        var newValue = GameModeToggle.IsChecked == true ? 1 : 0;
                        key.SetValue("GameModeEnabled", newValue, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"Game Mode has been {(newValue == 1 ? "Enabled" : "Disabled")}");
                    }
                    else
                    {
                        ShowError("Failed to access Game Mode registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Failed to change Game Mode setting.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Game Mode setting.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Game Mode setting: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Game Mode setting: {ex.Message}");
            }
        }

        private void GameBarToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(GameBarKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        var newValue = GameBarToggle.IsChecked == true ? 1 : 0;
                        key.SetValue("AppCaptureEnabled", newValue, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"Game Bar has been {(newValue == 1 ? "Enabled" : "Disabled")}");
                    }
                    else
                    {
                        ShowError("Failed to access Game Bar registry key.");
                        App.changelogUserControl?.AddLog("Failed", "Failed to change Game Bar setting.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Game Bar setting.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Game Bar setting: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Game Bar setting: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
