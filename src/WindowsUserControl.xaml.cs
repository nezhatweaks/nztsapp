using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace NZTS_App.Views
{
    public partial class WindowsUserControl : UserControl
    {
        private const string GameModeKeyPath = @"Software\Microsoft\GameBar";
        private const string GameBarKeyPath = @"Software\Microsoft\Windows\CurrentVersion\GameDVR";
        private const string IntelKeyPath = @"SYSTEM\CurrentControlSet\Services\intelppm";
        private const string AmdKeyPath = @"SYSTEM\CurrentControlSet\Services\amdppm";
        private const string GamesKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";

        private MainWindow mainWindow;

        public WindowsUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Gaming";
            this.Loaded += OnWindowLoaded;

            // Temporarily detach event to avoid premature triggering during initialization
            GameModeToggle.Click -= GameModeToggle_Click;
            GameBarToggle.Click -= GameBarToggle_Click;
            MultimediaTweaksToggle.Click -= MultimediaTweaksToggle_Click;



            LoadCurrentSettings(); // Load current values on initialization

            // Reattach events after loading
            GameModeToggle.Click += GameModeToggle_Click;
            GameBarToggle.Click += GameBarToggle_Click;
            MultimediaTweaksToggle.Click += MultimediaTweaksToggle_Click;

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

                // Load current multimedia settings from the registry
                using (var key = Registry.LocalMachine.OpenSubKey(GamesKeyPath, writable: true))
                {
                    if (key != null)
                    {

                        ulong clockRate = GetRegistryValueAsUInt64(key, "Clock Rate", 4294967295);  
                        ulong gpuPriority = GetRegistryValueAsUInt64(key, "GPU Priority", 16);              
                        ulong priority = GetRegistryValueAsUInt64(key, "Priority", 597);                    

                        // Apply Multimedia Tweaks Toggle based on current values
                        if (clockRate == 4294967295)
                        {
                            
                            MultimediaTweaksToggle.IsChecked = true;
                        }
                        else
                        {
                            
                            MultimediaTweaksToggle.IsChecked = false;
                        }

                        // If GPU Priority or Priority is modified, enable multimedia tweaks
                        if (gpuPriority == 0x10 || priority == 0x255)
                        {
                            MultimediaTweaksToggle.IsChecked = true; // Enable if settings are different from defaults
                        }
                    }
                    else
                    {
                        // Handle case where the key doesn't exist
                        MultimediaTweaksToggle.IsChecked = false; // Disable it by default
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

        // Helper function to get registry values safely as UInt64
        private static ulong GetRegistryValueAsUInt64(RegistryKey key, string valueName, ulong defaultValue)
        {
            object value = key.GetValue(valueName, defaultValue); // Get value, default if missing
            if (value == null)
            {
                // If the value is null or not found, return the default value
                return defaultValue;
            }

            try
            {
                return Convert.ToUInt64(value); // Try converting the value to UInt64
            }
            catch
            {
                // If conversion fails (e.g., if the value isn't numeric), return the default
                return defaultValue;
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

        private void MultimediaTweaksToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the toggle is ON or OFF
                if (MultimediaTweaksToggle.IsChecked == true)
                {
                    // Apply multimedia tweaks 
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", writable: true))
                    {
                        if (key == null)
                        {
                            // Create the key if it doesn't exist
                            using (var newKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"))
                            {
                                // Set default values (tweaked) for Clock Rate, GPU Priority, and Priority
                                newKey?.SetValue("Clock Rate", 4294967295, RegistryValueKind.QWord); // Tweaked value for Clock Rate
                                newKey?.SetValue("GPU Priority", 16, RegistryValueKind.QWord);        // Tweaked value for GPU Priority
                                newKey?.SetValue("Priority", 597, RegistryValueKind.QWord);           // Tweaked value for Priority
                            }
                        }
                        else
                        {
                            // Apply tweaked values if the key already exists
                            key?.SetValue("Clock Rate", 4294967295, RegistryValueKind.QWord);
                            key?.SetValue("GPU Priority", 16, RegistryValueKind.QWord);
                            key?.SetValue("Priority", 597, RegistryValueKind.QWord);
                        }
                    }

                    // Log the action: Enabled multimedia tweaks
                    App.changelogUserControl?.AddLog("Applied", "Multimedia tweaks have been tweaked.");
                    
                }
                else
                {
                    // Revert to default settings when toggled off (set default values)
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", writable: true))
                    {
                        if (key != null)
                        {
                            // Revert to default values (set default values)
                            key?.SetValue("Clock Rate", 10000, RegistryValueKind.DWord);  // Default value for Clock Rate
                            key?.SetValue("GPU Priority", 8, RegistryValueKind.DWord);    // Default value for GPU Priority
                            key?.SetValue("Priority", 6, RegistryValueKind.DWord);        // Default value for Priority
                        }
                    }

                    // Log the action: Disabled multimedia tweaks
                    App.changelogUserControl?.AddLog("Applied", "Multimedia tweaks have been disabled and reverted to default values.");
                    
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error applying multimedia tweaks: {ex.Message}");
            }
        }





        private void Intel1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(IntelKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(IntelKeyPath))
                        {
                            newKey?.SetValue("Start", 1, RegistryValueKind.DWord); // Set to 1
                        }
                    }

                    key?.SetValue("Start", 1, RegistryValueKind.DWord);  // Set value to 1
                    
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "Intel Processor service has been changed to 1.");

                    // Highlight the appropriate button for Intel
                    HighlightIntelButton(1);  // Highlight Intel 1 Button

                    // Show message after the button click
                    MessageBox.Show("Intel Processor service has been changed to 1.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting Intel Processor setting: {ex.Message}");
            }
        }

        private void Intel4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(IntelKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(IntelKeyPath))
                        {
                            newKey?.SetValue("Start", 4, RegistryValueKind.DWord); // Set to 4
                        }
                    }
                    key?.SetValue("Start", 4, RegistryValueKind.DWord);  // Set value to 4
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "Intel Processor service has been changed to 4.");
                    // Highlight the appropriate button for Intel
                    HighlightIntelButton(4);  // Highlight Intel 4 Button
                    // Show message after the button click
                    MessageBox.Show("Intel Processor service has been changed to 4.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting Intel Processor setting: {ex.Message}");
            }
        }

        private void IntelReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(IntelKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(IntelKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Set to 3
                        }
                    }

                    key?.SetValue("Start", 3, RegistryValueKind.DWord);  // Set value to 3
                    
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "Intel Processor service has been reset.");

                    // Highlight the appropriate button for Intel
                    HighlightIntelButton(3);  // Highlight Intel Reset Button

                    // Show message after the button click
                    MessageBox.Show("Intel Processor service has been reset to default.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting Intel Processor setting: {ex.Message}");
            }
        }

        private void AmdReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AmdKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(AmdKeyPath))
                        {
                            newKey?.SetValue("Start", 3, RegistryValueKind.DWord); // Set to 3
                        }
                    }

                    key?.SetValue("Start", 3, RegistryValueKind.DWord);  // Set value to 3
                    
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "AMD Processor service has been reset.");

                    // Highlight the appropriate button for AMD
                    HighlightAmdButton(3);  // Highlight AMD Reset Button

                    // Show message after the button click
                    MessageBox.Show("AMD Processor service has been reset to default.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting AMD Processor setting: {ex.Message}");
            }
        }

        private void Amd1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AmdKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(AmdKeyPath))
                        {
                            newKey?.SetValue("Start", 1, RegistryValueKind.DWord); // Set to 1
                        }
                    }

                    key?.SetValue("Start", 1, RegistryValueKind.DWord);  // Set value to 1
                    
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "AMD Processor service has been changed to 1.");

                    // Highlight the appropriate button for AMD
                    HighlightAmdButton(1);  // Highlight AMD 1 Button

                    // Show message after the button click
                    MessageBox.Show("AMD Processor service has been changed to 1.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting AMD Processor setting: {ex.Message}");
            }
        }

        private void Amd4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AmdKeyPath, writable: true))
                {
                    if (key == null)
                    {
                        using (var newKey = Registry.LocalMachine.CreateSubKey(AmdKeyPath))
                        {
                            newKey?.SetValue("Start", 4, RegistryValueKind.DWord); // Set to 4
                        }
                    }

                    key?.SetValue("Start", 4, RegistryValueKind.DWord);  // Set value to 4
                    
                    mainWindow?.MarkSettingsApplied();
                    App.changelogUserControl?.AddLog("Applied", "AMD Processor service has been changed to 4.");

                    // Highlight the appropriate button for AMD
                    HighlightAmdButton(4);  // Highlight AMD 4 Button

                    // Show message after the button click
                    MessageBox.Show("AMD Processor service has been changed to 4.", "Operation Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting AMD Processor setting: {ex.Message}");
            }
        }


        private void HighlightIntelButton(int state)
        {
            // Reset button styles
            Intel1Button.ClearValue(Button.BackgroundProperty);
            Intel3Button.ClearValue(Button.BackgroundProperty);
            Intel4Button.ClearValue(Button.BackgroundProperty);

            // Highlight button according to state
            switch (state)
            {
                case 1:
                    Intel1Button.Background = Brushes.Green; // Highlight Intel1 Button
                    break;
                case 3:
                    Intel3Button.Background = Brushes.Green; // Highlight Intel3 Button
                    break;
                case 4:
                    Intel4Button.Background = Brushes.Green; // Highlight Intel4 Button
                    break;
            }
        }

        private void HighlightAmdButton(int state)
        {
            // Reset button styles
            Amd1Button.ClearValue(Button.BackgroundProperty);
            Amd3Button.ClearValue(Button.BackgroundProperty);
            Amd4Button.ClearValue(Button.BackgroundProperty);

            // Highlight button according to state
            switch (state)
            {
                case 1:
                    Amd1Button.Background = Brushes.Green; // Highlight Amd1 Button
                    break;
                case 3:
                    Amd3Button.Background = Brushes.Green; // Highlight Amd3 Button
                    break;
                case 4:
                    Amd4Button.Background = Brushes.Green; // Highlight Amd4 Button
                    break;
            }
        }

        // Method to read and highlight buttons based on registry value
        private void UpdateButtonHighlighting()
        {
            // Read current Intel setting
            int currentIntelValue = GetRegistryValue(IntelKeyPath);
            HighlightButtonForSetting(Intel1Button, currentIntelValue == 1);
            HighlightButtonForSetting(Intel3Button, currentIntelValue == 3);
            HighlightButtonForSetting(Intel4Button, currentIntelValue == 4);

            // Read current AMD setting
            int currentAmdValue = GetRegistryValue(AmdKeyPath);
            HighlightButtonForSetting(Amd1Button, currentAmdValue == 1);
            HighlightButtonForSetting(Amd3Button, currentAmdValue == 3);
            HighlightButtonForSetting(Amd4Button, currentAmdValue == 4);
        }

        // Helper function to read registry value (returns default 3 if not found)
        private int GetRegistryValue(string registryPath)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("Start", 3); // Default to 3 if not found
                        return Convert.ToInt32(value);
                    }
                    return 3; // Default value if registry key does not exist
                }
            }
            catch (Exception)
            {
                return 3; // Default value on error
            }
        }

        // Method to highlight the button (e.g., change background color or style)
        private void HighlightButtonForSetting(Button button, bool isHighlighted)
        {
            if (isHighlighted)
            {
                button.Background = Brushes.LightGreen;  // Highlight color
                button.Foreground = Brushes.White;       // Text color when highlighted
            }
            else
            {
                button.Background = Brushes.DimGray;  // Normal background
                button.Foreground = Brushes.White;         // Text color when not highlighted
            }
        }

        // Call this function when the UI is loaded or whenever registry values might change
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            UpdateButtonHighlighting();  // Update highlighting when the window loads
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
