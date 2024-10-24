using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App
{
    public partial class CPUPriorityControl : UserControl
    {
        private ObservableCollection<Game> games;
        private MainWindow mainWindow;

        public CPUPriorityControl(MainWindow window)
        {
            InitializeComponent();
            games = new ObservableCollection<Game>();
            GameListView.ItemsSource = games;
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Process";

            // Initialize priority and GPU scheduling options
            InitializePriorityOptions();
            InitializeGPUSchedulingOptions();
        }

        private void InitializePriorityOptions()
        {
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "High" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Above Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Below Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Low" });
        }

        private void InitializeGPUSchedulingOptions()
        {
            GPUSchedulingComboBox.Items.Add(new ComboBoxItem { Content = "Default" });
            GPUSchedulingComboBox.Items.Add(new ComboBoxItem { Content = "High Performance" });
        }

        public class Game : INotifyPropertyChanged
        {
            private string? name;
            private string? priority;
            private string? gpuScheduling;

            public required string Name
            {
                get => name ?? "Unknown Game";
                set
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            public required string Priority
            {
                get => priority ?? "Normal";
                set
                {
                    if (priority != value)
                    {
                        priority = value;
                        OnPropertyChanged(nameof(Priority));
                    }
                }
            }

            public required string GPUScheduling
            {
                get => gpuScheduling ?? "Default";
                set
                {
                    if (gpuScheduling != value)
                    {
                        gpuScheduling = value;
                        OnPropertyChanged(nameof(GPUScheduling));
                    }
                }
            }



            public void SetPriorityFromRegistry()
            {
                string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{Name}";
                using (var gameKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (gameKey != null)
                    {
                        using (var perfOptionsKey = gameKey.OpenSubKey("PerfOptions"))
                        {
                            if (perfOptionsKey != null)
                            {
                                var value = perfOptionsKey.GetValue("CpuPriorityClass");
                                if (value is int priorityValue)
                                {
                                    Priority = GetPriorityString(priorityValue);
                                }
                                else
                                {
                                    Priority = "Normal";
                                }

                                var gpuValue = perfOptionsKey.GetValue("GPUScheduling");
                                GPUScheduling = gpuValue != null && gpuValue is int gpuSchedulingValue
                                    ? gpuSchedulingValue == 1 ? "High Performance" : "Default"
                                    : "Default";
                            }
                        }
                    }
                }
            }

            private string GetPriorityString(int priorityValue)
            {
                return priorityValue switch
                {
                    0x00000003 => "High",
                    0x00000006 => "Above Normal",
                    0x00000002 => "Normal",
                    0x00000005 => "Below Normal",
                    0x00000001 => "Low",
                    _ => "Normal"
                };
            }

            public void SetGPUSchedulingInRegistry()
            {
                string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{Name}";
                using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
                {
                    if (gameKey != null)
                    {
                        int gpuValue = GPUScheduling == "High Performance" ? 1 : 0;
                        gameKey.SetValue("GPUScheduling", gpuValue, RegistryValueKind.DWord);
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the executable name of the game (e.g., game.exe):", "Add Game", "");

            if (!string.IsNullOrWhiteSpace(gameName) && gameName.EndsWith(".exe"))
            {
                if (games.Any(g => g.Name.Equals(gameName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("This game is already in the list.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newGame = new Game { Name = gameName, Priority = "Normal", GPUScheduling = "Default" };
                newGame.SetPriorityFromRegistry();
                games.Add(newGame);
                GameListView.SelectedItem = newGame;
                UpdateUIForSelectedGame(newGame);
                GameListView.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Please enter a valid executable name ending with .exe.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                games.Remove(selectedGame);
                ResetGameUI();
            }
        }

        private void ResetGameUI()
        {
            UseLargePagesToggle.IsChecked = false;
            UseLargePagesStatusTextBlock.Text = "Disabled";
            DisableHeapCoalesceToggle.IsChecked = false;
            DisableHeapCoalesceStatusTextBlock.Text = "Disabled";
            PriorityComboBox.SelectedIndex = -1;
            GPUSchedulingComboBox.SelectedIndex = -1;
        }



        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                selectedGame.SetPriorityFromRegistry();
                UpdateUIForSelectedGame(selectedGame);
            }
        }

        private void UpdateUIForSelectedGame(Game selectedGame)
        {
            UpdateComboBoxForSelectedGame(selectedGame);
            InitializeUseLargePagesToggle(selectedGame);
            InitializeDisableHeapCoalesceToggle(selectedGame);
            UpdateGPUSchedulingComboBoxForSelectedGame(selectedGame); // Update GPU scheduling UI
        }

        private bool isUpdatingComboBox = false;

        private void UpdateComboBoxForSelectedGame(Game selectedGame)
        {
            if (isUpdatingComboBox) return;
            isUpdatingComboBox = true;

            var matchingItem = PriorityComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.Priority);

            PriorityComboBox.SelectedItem = matchingItem ?? PriorityComboBox.Items[2]; // Default to "Normal"

            isUpdatingComboBox = false;
        }

        private void UpdateGPUSchedulingComboBoxForSelectedGame(Game selectedGame)
        {
            var matchingItem = GPUSchedulingComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.GPUScheduling);

            GPUSchedulingComboBox.SelectedItem = matchingItem ?? GPUSchedulingComboBox.Items[0]; // Default to "Default"
        }

        private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingComboBox) return;

            if (GameListView.SelectedItem is Game selectedGame && PriorityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedPriority = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedPriority))
                {
                    selectedGame.Priority = selectedPriority;
                    GameListView.Items.Refresh();
                    UpdateComboBoxForSelectedGame(selectedGame);
                }
            }
        }

        private void GPUSchedulingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame && GPUSchedulingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedScheduling = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedScheduling))
                {
                    selectedGame.GPUScheduling = selectedScheduling;
                    selectedGame.SetGPUSchedulingInRegistry(); // Save the setting to the registry
                }
            }
        }

        

        private int GetPriorityValue(string priority)
        {
            return priority switch
            {
                "High" => 0x00000003,
                "Above Normal" => 0x00000002,
                "Normal" => 0x00000001,
                "Below Normal" => 0x00000000,
                "Low" => 0x00000000,
                _ => 0
            };
        }

        private void AddActiveProcessButton_Click(object sender, RoutedEventArgs e)
        {
            var activeProcessesWindow = new ActiveProcessesWindow(this); // Pass the reference to the current control
            activeProcessesWindow.ShowDialog();
        }


        public void AddGame(string executableName)
        {
            // Check for duplicates
            if (games.Any(g => g.Name.Equals(executableName, StringComparison.OrdinalIgnoreCase)))
            {
                App.changelogUserControl?.AddLog("Duplicated", "This game is already in the list.");
                MessageBox.Show("This game is already in the list.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newGame = new Game
            {
                Name = executableName,
                Priority = "Normal",
                GPUScheduling = "Default" // Set this to a valid default value
            };

            games.Add(newGame);



            // Set default UseLargePages value
            SetUseLargePagesForNewGame(newGame, false); // Default to false, change as needed
            SetDisableHeapCoalesceForNewGame(newGame, false); // Default to false


            // Automatically select the newly added game
            GameListView.SelectedItem = newGame;
            UpdateComboBoxForSelectedGame(newGame);

            // Initialize Use Large Pages toggle
            InitializeUseLargePagesToggle(newGame);
            InitializeDisableHeapCoalesceToggle(newGame);
        }


        private void SetDisableHeapCoalesceForNewGame(Game game, bool disableHeapCoalesce)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{game.Name}";
            using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
            {
                if (gameKey != null)
                {
                    gameKey.SetValue("DisableHeapCoalesceOnFree", disableHeapCoalesce ? 1 : 0, RegistryValueKind.DWord);
                }
            }
        }


        private void SetDisableHeapCoalesce(bool disableHeapCoalesce)
        {
            // This method updates the DisableHeapCoalesceOnFree registry value for all games.
            // Note: This setting should not be saved in PerfOptions.
            string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";
            foreach (var game in games)
            {
                using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath + game.Name, true))
                {
                    if (gameKey != null)
                    {
                        gameKey.SetValue("DisableHeapCoalesceOnFree", disableHeapCoalesce ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
        }




        private void SetUseLargePagesForNewGame(Game game, bool useLargePages)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{game.Name}";
            using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
            {
                if (gameKey != null)
                {
                    gameKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);
                }
            }
        }


        private void UseLargePagesToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool useLargePages = UseLargePagesToggle.IsChecked == true;
                UseLargePagesStatusTextBlock.Text = useLargePages ? "Enabled" : "Disabled";

                // Set the value in the registry
                string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";
                foreach (var game in games)
                {
                    using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath + game.Name, true))
                    {
                        if (gameKey != null)
                        {
                            gameKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);
                            App.changelogUserControl?.AddLog("Applied", $"Use Large Pages for {game.Name} set to {(useLargePages ? "enabled" : "disabled")}");
                        }
                        else
                        {
                            MessageBox.Show($"Failed to access the registry key for {game.Name}.");
                            App.changelogUserControl?.AddLog("Failed", $"Registry key for {game.Name} not found.");
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Use Large Pages.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Use Large Pages: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Use Large Pages: {ex.Message}");
            }
        }


        private void SetUseLargePages(bool useLargePages)
        {
            string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";
            foreach (var game in games)
            {
                using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath + game.Name, true))
                {
                    if (gameKey != null)
                    {
                        gameKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
        }


        private void InitializeUseLargePagesToggle(Game selectedGame)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}\PerfOptions";

            try
            {
                using (var gameKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (gameKey != null)
                    {
                        object? value = gameKey.GetValue("UseLargePages");

                        // Check if the value is an integer and set the toggle accordingly
                        if (value is int intValue)
                        {
                            UseLargePagesToggle.IsChecked = intValue == 1; // Set toggle based on registry value
                            UseLargePagesStatusTextBlock.Text = intValue == 1 ? "Enabled" : "Disabled";
                        }
                        else
                        {
                            UseLargePagesToggle.IsChecked = false; // Default if value is not set
                            UseLargePagesStatusTextBlock.Text = "Disabled";
                        }
                    }
                    else
                    {
                        UseLargePagesToggle.IsChecked = false; // No key found
                        UseLargePagesStatusTextBlock.Text = "Disabled";
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the registry: {ex.Message}");
            }
        }

        private void InitializeDisableHeapCoalesceToggle(Game selectedGame)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}\PerfOptions";

            try
            {
                using (var gameKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (gameKey != null)
                    {
                        object? value = gameKey.GetValue("DisableHeapCoalesceOnFree");

                        // Set toggle based on registry value
                        if (value is int intValue)
                        {
                            DisableHeapCoalesceToggle.IsChecked = intValue == 1; // Set toggle based on registry value
                            DisableHeapCoalesceStatusTextBlock.Text = intValue == 1 ? "Enabled" : "Disabled";
                        }
                        else
                        {
                            DisableHeapCoalesceToggle.IsChecked = false; // Default if value is not set
                            DisableHeapCoalesceStatusTextBlock.Text = "Disabled";
                        }
                    }
                    else
                    {
                        DisableHeapCoalesceToggle.IsChecked = false; // No key found
                        DisableHeapCoalesceStatusTextBlock.Text = "Disabled";
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the registry: {ex.Message}");
            }
        }

        private void DisableHeapCoalesceToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool disableHeapCoalesce = DisableHeapCoalesceToggle.IsChecked == true;
                DisableHeapCoalesceStatusTextBlock.Text = disableHeapCoalesce ? "Enabled" : "Disabled";

                string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";
                foreach (var game in games)
                {
                    using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath + game.Name, true))
                    {
                        if (gameKey != null)
                        {
                            gameKey.SetValue("DisableHeapCoalesceOnFree", disableHeapCoalesce ? 1 : 0, RegistryValueKind.DWord);
                            App.changelogUserControl?.AddLog("Applied", $"Disable Heap Coalesce on Free for {game.Name} set to {(disableHeapCoalesce ? "enabled" : "disabled")}");
                        }
                        else
                        {
                            MessageBox.Show($"Failed to access the registry key for {game.Name}.");
                            App.changelogUserControl?.AddLog("Failed", $"Registry key for {game.Name} not found.");
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", "Unauthorized access to modify Disable Heap Coalesce on Free.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating Disable Heap Coalesce on Free: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating Disable Heap Coalesce on Free: {ex.Message}");
            }
        }



        private void ResetPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                selectedGame.Priority = "Normal";
                GameListView.Items.Refresh();

                // Update ComboBox selection to reflect the reset
                UpdateComboBoxForSelectedGame(selectedGame);

            }
        }

        private void SavePriorityButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var game in games)
            {
                int priorityValue = GetPriorityValue(game.Priority);
                if (priorityValue == 0)
                {
                    App.changelogUserControl?.AddLog("Invalid", $"Invalid priority for {game.Name}.");
                    MessageBox.Show($"Invalid priority for {game.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{game.Name}";

                try
                {
                    using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
                    {
                        if (gameKey != null)
                        {
                            using (var perfOptionsKey = gameKey.CreateSubKey("PerfOptions", true))
                            {
                                if (perfOptionsKey != null)
                                {
                                    perfOptionsKey.SetValue("CpuPriorityClass", priorityValue, RegistryValueKind.DWord);

                                    // Save Use Large Pages setting for the game
                                    bool useLargePages = UseLargePagesToggle.IsChecked == true;
                                    perfOptionsKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);

                                    // Save GPU Scheduling setting
                                    int gpuSchedulingValue = game.GPUScheduling == "High Performance" ? 1 : 0; // Use string comparison
                                    perfOptionsKey.SetValue("GPUScheduling", gpuSchedulingValue, RegistryValueKind.DWord);

                                    // Log and show message for Use Large Pages
                                    string largePagesMessage = useLargePages ? "enabled" : "disabled";
                                    App.changelogUserControl?.AddLog("Applied", $"Use Large Pages for {game.Name} set to {largePagesMessage}.");
                                    MessageBox.Show($"Use Large Pages for {game.Name} has been {largePagesMessage}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Save Disable Heap Coalesce setting
                                    bool disableHeapCoalesce = DisableHeapCoalesceToggle.IsChecked == true;
                                    perfOptionsKey.SetValue("DisableHeapCoalesceOnFree", disableHeapCoalesce ? 1 : 0, RegistryValueKind.DWord);
                                    App.changelogUserControl?.AddLog("Applied", $"Disable Heap Coalesce on Free for {game.Name} set to {(disableHeapCoalesce ? "enabled" : "disabled")}");

                                    // Show message for Disable Heap Coalesce
                                    MessageBox.Show($"Disable Heap Coalesce on Free for {game.Name} has been {(disableHeapCoalesce ? "enabled" : "disabled")}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Show message for GPU Scheduling
                                    string gpuSchedulingMessage = gpuSchedulingValue == 1 ? "High Performance" : "Default";
                                    MessageBox.Show($"GPU Scheduling for {game.Name} set to {gpuSchedulingMessage}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Show message for Priority
                                    MessageBox.Show($"Priority for {game.Name} set to {game.Priority}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    App.changelogUserControl?.AddLog("Applied", $"Priority for {game.Name} set to {game.Priority}.");

                                    var mainWindow = Application.Current.MainWindow as MainWindow;
                                    mainWindow?.MarkSettingsApplied();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to set priority for {game.Name}. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", $"Failed to set priority for {game.Name}.");
                }
            }
        }






    }
}
