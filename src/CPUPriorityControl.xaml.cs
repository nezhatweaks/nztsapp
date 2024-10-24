using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Windows.Controls.Primitives;

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
                

                // Automatically select the newly added game
                GameListView.SelectedItem = newGame;

                // Call the method to initialize UI elements based on the new game
                UpdateUIForSelectedGame(newGame);

                GameListView.Items.Refresh(); // Refresh the ListView to reflect the priority
            }
            else
            {
                MessageBox.Show("Please enter a valid executable name ending with .exe.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                App.changelogUserControl?.AddLog("Cancelled", "Exit the Process 'Add By Name'.");
            }
        }



        private void RemoveGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                // Remove the game from the collection
                games.Remove(selectedGame);
                

                // Clear the toggle and priority UI elements
                UseLargePagesToggle.IsChecked = false; // Turn off the toggle
                UseLargePagesStatusTextBlock.Text = "Disabled"; // Update the status text

                // Clear the selection in the ComboBox
                PriorityComboBox.SelectedIndex = -1; // Clear the selection in the ComboBox
            }
        }



        private void GameListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                // Fetch the priority and large pages setting from the registry for the selected game
                selectedGame.SetPriorityFromRegistry();

                // Update the UI elements
                UpdateUIForSelectedGame(selectedGame);
            }
        }

        private void UpdateUIForSelectedGame(Game selectedGame)
        {
            // Update ComboBox selection
            UpdateComboBoxForSelectedGame(selectedGame);

            // Initialize Use Large Pages toggle
            InitializeUseLargePagesToggle(selectedGame);
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

                                    // Save Use Large Pages setting for the game
                                    bool useLargePages = UseLargePagesToggle.IsChecked == true;
                                    perfOptionsKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);

                                    // Log and show message for Use Large Pages
                                    string largePagesMessage = useLargePages ? "enabled" : "disabled";
                                    App.changelogUserControl?.AddLog("Applied", $"Use Large Pages for {game.Name} set to {largePagesMessage}.");
                                    MessageBox.Show($"Use Large Pages for {game.Name} has been {largePagesMessage}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

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
            

            // Set default UseLargePages value
            SetUseLargePagesForNewGame(newGame, false); // Default to false, change as needed

            // Automatically select the newly added game
            GameListView.SelectedItem = newGame;
            UpdateComboBoxForSelectedGame(newGame);

            // Initialize Use Large Pages toggle
            InitializeUseLargePagesToggle(newGame);
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









    }
}
