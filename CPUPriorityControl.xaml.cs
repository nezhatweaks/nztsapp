using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace NZTS_App
{
    public partial class CPUPriorityControl : UserControl
    {
        private ObservableCollection<Game> games;
        private bool settingsChanged = false;

        public CPUPriorityControl()
        {
            InitializeComponent();
            games = new ObservableCollection<Game>();
            GameListView.ItemsSource = games;

            // Initialize priority options
            InitializePriorityOptions();
        }

        private void InitializePriorityOptions()
        {
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "High" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Above Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Below Normal" });
            PriorityComboBox.Items.Add(new ComboBoxItem { Content = "Low" });
        }

        public class Game : INotifyPropertyChanged
        {
            private string? name;
            private string? priority;

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
                    if (priority != value) // Check for actual change
                    {
                        priority = value;
                        OnPropertyChanged(nameof(Priority));
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

                                if (value != null && value is int priorityValue)
                                {
                                    Priority = GetPriorityString(priorityValue);
                                }
                                else
                                {
                                    // If no priority is set in the registry, default to "Normal"
                                    Priority = "Normal";
                                }
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

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user to enter a single executable name
            var gameName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the executable name of the game (e.g., game.exe):",
                "Add Game",
                "");

            // Validate the input for proper format
            if (!string.IsNullOrWhiteSpace(gameName) && gameName.EndsWith(".exe"))
            {
                // Check for duplicates
                if (games.Any(g => g.Name.Equals(gameName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("This game is already in the list.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                    App.changelogUserControl?.AddLog("Duplicated", $"Unable to change the CPU Priority for {gameName} setting.");
                    return;
                }

                var newGame = new Game { Name = gameName, Priority = "Normal" }; // Initialize Priority to a default value
                newGame.SetPriorityFromRegistry(); // Optionally fetch the priority from the registry
                games.Add(newGame);
                settingsChanged = true;


                // Automatically select the newly added game
                GameListView.SelectedItem = newGame;
                UpdateComboBoxForSelectedGame(newGame);

                GameListView.Items.Refresh(); // Refresh the ListView to reflect the priority
            }
            else
            {
                MessageBox.Show("Please enter a valid executable name ending with .exe.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.changelogUserControl?.AddLog("Invalid", "Invalid executable name.");
            }
        }

        private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem != null)
            {
                games.Remove((Game)GameListView.SelectedItem);
                settingsChanged = true;
            }
        }

        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                // Ensure priority is fetched from the registry before updating UI
                selectedGame.SetPriorityFromRegistry();

                // Update ComboBox with the correct priority
                UpdateComboBoxForSelectedGame(selectedGame);

                // Refresh the ListView to reflect the selected game's priority in the UI
                GameListView.Items.Refresh();
            }
        }

        private bool isUpdatingComboBox = false; // Add a flag to prevent recursion

        private void UpdateComboBoxForSelectedGame(Game selectedGame)
        {
            if (isUpdatingComboBox) return; // Prevent recursive updates

            isUpdatingComboBox = true; // Set the flag to indicate we're updating the ComboBox

            // This should ensure the ComboBox reflects the correct priority
            var matchingItem = PriorityComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.Priority);

            // Set the selected item in the ComboBox
            if (matchingItem != null)
            {
                PriorityComboBox.SelectedItem = matchingItem;
            }
            else
            {
                // If no match is found, default to "Normal"
                PriorityComboBox.SelectedIndex = 2; // Assuming "Normal" is at index 2
            }

            isUpdatingComboBox = false; // Reset the flag after the update is done
        }

        private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingComboBox) return; // Prevent recursion during ComboBox updates

            if (GameListView.SelectedItem is Game selectedGame && PriorityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedPriority = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(selectedPriority))
                {
                    selectedGame.Priority = selectedPriority; // This should trigger PropertyChanged
                    settingsChanged = true;

                    // Refresh the ListView to reflect the new priority
                    GameListView.Items.Refresh();

                    // Update the ComboBox selection to reflect the change
                    UpdateComboBoxForSelectedGame(selectedGame); // Sync ComboBox with the new value
                }
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

        private void ResetPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                selectedGame.Priority = "Normal";
                GameListView.Items.Refresh();

                // Update ComboBox selection to reflect the reset
                UpdateComboBoxForSelectedGame(selectedGame);
                settingsChanged = true;
            }
        }

        private int GetPriorityValue(string priority)
        {
            return priority switch
            {
                "High" => 0x00000003,
                "Above Normal" => 0x00000006,
                "Normal" => 0x00000002,
                "Below Normal" => 0x00000005,
                "Low" => 0x00000001,
                _ => 0x00000002 // Default to Normal
            };
        }

        public bool CheckForChangesAndPromptRestart()
        {
            if (settingsChanged)
            {
                var result = MessageBox.Show("Changes were made. Do you want to restart your computer now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }
            return false;
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

            var newGame = new Game { Name = executableName, Priority = "Normal" };
            games.Add(newGame);
            settingsChanged = true;

            // Automatically select the newly added game
            GameListView.SelectedItem = newGame;
            UpdateComboBoxForSelectedGame(newGame);
        }

    }
}
