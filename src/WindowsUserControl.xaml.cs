﻿using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class WindowsUserControl : UserControl
    {
        private const string GameModeKeyPath = @"Software\Microsoft\GameBar";
        private const string GameBarKeyPath = @"Software\Microsoft\Windows\CurrentVersion\GameDVR";
        private const string IntelKeyPath = @"SYSTEM\CurrentControlSet\Services\intelppm";
        private const string AmdKeyPath = @"SYSTEM\CurrentControlSet\Services\amdppm";

        private MainWindow mainWindow;

        public WindowsUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Gaming";

            // Temporarily detach event to avoid premature triggering during initialization
            GameModeToggle.Click -= GameModeToggle_Click;
            GameBarToggle.Click -= GameBarToggle_Click;
            IntelToggle.Click -= IntelToggle_Click;
            AmdToggle.Click -= AmdToggle_Click;

            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            GameModeToggle.Click += GameModeToggle_Click;
            GameBarToggle.Click += GameBarToggle_Click;
            IntelToggle.Click += IntelToggle_Click;
            AmdToggle.Click += AmdToggle_Click;
        }

        private void LoadCurrentSettings()
        {
            try
            {
                // Load Game Mode setting
                using (var key = Registry.CurrentUser.OpenSubKey(GameModeKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist, create it
                        using (var newKey = Registry.CurrentUser.CreateSubKey(GameModeKeyPath))
                        {
                            newKey?.SetValue("AutoGameModeEnabled", 0, RegistryValueKind.DWord); // Default to 0 (disabled)
                        }
                    }
                    else
                    {
                        var gameModeValue = key.GetValue("AutoGameModeEnabled", 0); // Default to 0 if not found
                        GameModeToggle.IsChecked = (gameModeValue is int modeInt && modeInt == 1);
                    }
                }

                // Load Game Bar setting
                using (var key = Registry.CurrentUser.OpenSubKey(GameBarKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist, create it
                        using (var newKey = Registry.CurrentUser.CreateSubKey(GameBarKeyPath))
                        {
                            newKey?.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord); // Default to 0 (disabled)
                        }
                    }
                    else
                    {
                        var gameBarValue = key.GetValue("AppCaptureEnabled", 0); // Default to 0 if not found
                        GameBarToggle.IsChecked = (gameBarValue is int barInt && barInt == 1);
                    }
                }

                // Load Intel processor setting
                using (var key = Registry.LocalMachine.OpenSubKey(IntelKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist, create it
                        using (var newKey = Registry.LocalMachine.CreateSubKey(IntelKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Default to 3 (disabled)
                        }
                    }
                    else
                    {
                        var intelValue = key.GetValue("Start", 3); // Default to 3 if not found
                        IntelToggle.IsChecked = (intelValue is int intelInt && intelInt == 1);
                    }
                }

                // Load AMD processor setting
                using (var key = Registry.LocalMachine.OpenSubKey(AmdKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist, create it
                        using (var newKey = Registry.LocalMachine.CreateSubKey(AmdKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Default to 3 (disabled)
                        }
                    }
                    else
                    {
                        var amdValue = key.GetValue("Start", 3); // Default to 3 if not found
                        AmdToggle.IsChecked = (amdValue is int amdInt && amdInt == 1);
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
                // Open the registry key for Game Mode (creating it if it doesn't exist)
                using (var key = Registry.CurrentUser.OpenSubKey(GameModeKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Create the key if it doesn't exist and set the default value
                        using (var newKey = Registry.CurrentUser.CreateSubKey(GameModeKeyPath))
                        {
                            newKey?.SetValue("AutoGameModeEnabled", 0, RegistryValueKind.DWord); // Default to 0 (disabled)
                        }
                    }

                    // Update the Game Mode setting
                    var newValue = GameModeToggle.IsChecked == true ? 1 : 0;
                    key?.SetValue("GameModeEnabled", newValue, RegistryValueKind.DWord);
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", $"Game Mode has been {(newValue == 1 ? "Enabled" : "Disabled")}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Game Mode setting: {ex.Message}");
            }
        }

        private void GameBarToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the registry key for Game Bar (creating it if it doesn't exist)
                using (var key = Registry.CurrentUser.OpenSubKey(GameBarKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Create the key if it doesn't exist and set the default value
                        using (var newKey = Registry.CurrentUser.CreateSubKey(GameBarKeyPath))
                        {
                            newKey?.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord); // Default to 0 (disabled)
                        }
                    }

                    // Update the Game Bar setting
                    var newValue = GameBarToggle.IsChecked == true ? 1 : 0;
                    key?.SetValue("AppCaptureEnabled", newValue, RegistryValueKind.DWord);
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", $"Game Bar has been {(newValue == 1 ? "Enabled" : "Disabled")}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Game Bar setting: {ex.Message}");
            }
        }

        private void IntelToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the registry key for Intel processor (creating it if it doesn't exist)
                using (var key = Registry.LocalMachine.OpenSubKey(IntelKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Create the key if it doesn't exist and set the default value
                        using (var newKey = Registry.LocalMachine.CreateSubKey(IntelKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Default to 3 (disabled)
                        }
                    }

                    // Update the Intel processor setting
                    var newValue = IntelToggle.IsChecked == true ? 1 : 3;
                    key?.SetValue("Start", newValue, RegistryValueKind.DWord);
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", $"Intel Processor service has been {(newValue == 1 ? "Tweaked" : "Restore")}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating Intel Processor setting: {ex.Message}");
            }
        }

        private void AmdToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the registry key for AMD processor (creating it if it doesn't exist)
                using (var key = Registry.LocalMachine.OpenSubKey(AmdKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        // Create the key if it doesn't exist and set the default value
                        using (var newKey = Registry.LocalMachine.CreateSubKey(AmdKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Default to 3 (disabled)
                        }
                    }

                    // Update the AMD processor setting
                    var newValue = AmdToggle.IsChecked == true ? 1 : 3;
                    key?.SetValue("Start", newValue, RegistryValueKind.DWord);
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", $"AMD Processor service has been {(newValue == 1 ? "Tweaked" : "Restore")}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating AMD Processor setting: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
