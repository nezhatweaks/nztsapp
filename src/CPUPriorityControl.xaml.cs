using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace NZTS_App
{
    public partial class CPUPriorityControl : UserControl
    {
        private ObservableCollection<Game> games;
        private MainWindow mainWindow;
        private ObservableCollection<Card> cards; // Define the collection


        public CPUPriorityControl(MainWindow window)
        {
            InitializeComponent();
            cards = new ObservableCollection<Card>(); // Initialize the collection here
            InitializeCards(); // Populate the cards collection
            DataContext = this; // Set DataContext to the current instance
            games = new ObservableCollection<Game>();
            GameListView.ItemsSource = games;
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "EXE";
            InitializePriorityOptions(); // Add this line to initialize priority options
            InitializeGPUSchedulingOptions(); // Add this line to initialize GPU scheduling options
            InitializeIOPriorityOptions(); // Initialize I/O priority options

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

        private void InitializeIOPriorityOptions()
        {
            IOPriorityComboBox.Items.Add(new ComboBoxItem { Content = "High" });
            IOPriorityComboBox.Items.Add(new ComboBoxItem { Content = "Normal" });
            IOPriorityComboBox.Items.Add(new ComboBoxItem { Content = "Low" });
            IOPriorityComboBox.Items.Add(new ComboBoxItem { Content = "Very Low" });
        }




        public class Game : INotifyPropertyChanged
        {
            private string? name;
            private string? priority;
            private string? gpuScheduling;
            private string? ioPriority;


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

            public required string IOPriority
            {
                get => ioPriority ?? "Normal";  // Default to "Normal" if not set
                set
                {
                    if (ioPriority != value)
                    {
                        ioPriority = value;
                        OnPropertyChanged(nameof(IOPriority));
                    }
                }
            }

            private string GetIOPriorityString(int priorityValue)
            {
                return priorityValue switch
                {
                    0 => "Very Low",
                    1 => "Low",
                    2 => "Normal",
                    3 => "High",
                    _ => "Normal"
                };
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

                                var ioPriorityValue = perfOptionsKey.GetValue("IOPriority");
                                IOPriority = ioPriorityValue != null && ioPriorityValue is int ioPriorityInt
                                    ? GetIOPriorityString(ioPriorityInt)
                                    : "Normal";  // Default to "Normal" if no value is found

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
                        int gpuValue = GPUScheduling == "High Performance" ? 1 : 0; // Update based on the property value
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

        private void BrowseExeButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe",
                Title = "Select an Executable"
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                string exePath = openFileDialog.FileName;

                ExePathTextBox.Text = exePath;
                FullscreenOptToggle.IsEnabled = true;
                UpdateToggleState(exePath);
            }
        }


        private void UpdateToggleState(string exePath)
        {
            const string regPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath, false);

            if (key?.GetValue(exePath) is string value)
            {
                FullscreenOptToggle.IsChecked = value.Contains("DISABLEDXMAXIMIZEDWINDOWEDMODE");
            }
            else
            {
                FullscreenOptToggle.IsChecked = false;
            }
        }


        private void FullscreenOptToggle_Checked(object sender, RoutedEventArgs e)
        {
            string exePath = ExePathTextBox.Text;

            if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
            {
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                key.SetValue(exePath, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE", RegistryValueKind.String);
                App.changelogUserControl?.AddLog("Applied", $"You have disabled Fullscreen Optimizations for {ExePathTextBox.Text}.");
            }
            

        }


        private void FullscreenOptToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            string exePath = ExePathTextBox.Text;

            if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                if (key?.GetValue(exePath) != null)
                {
                    key.DeleteValue(exePath);
                    App.changelogUserControl?.AddLog("Reverted", $"You have enabled Fullscreen Optimizations for {ExePathTextBox.Text}.");
                }
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

                var newGame = new Game { Name = gameName, Priority = "Normal", GPUScheduling = "Default", IOPriority = "Normal" };
                newGame.SetPriorityFromRegistry();
                games.Add(newGame);
                GameListView.SelectedItem = newGame;
                UpdateUIForSelectedGame(newGame);
                GameListView.Items.Refresh();
            }
            else
            {
                Console.Write("Please enter a valid executable name ending with .exe.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            DisableHeapCoalesceToggle.IsChecked = false;

            HackFlagsToggle.IsChecked = false;

            LargePageSizeInBytesToggle.IsChecked = false;

            PriorityComboBox.SelectedIndex = -1;
            GPUSchedulingComboBox.SelectedIndex = -1;
            IOPriorityComboBox.SelectedIndex = -1;

        }

        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Active";
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
            InitializeHackFlagsToggle(selectedGame);
            InitializeLargePageSizeInBytesToggle(selectedGame);
            UpdateGPUSchedulingComboBoxForSelectedGame(selectedGame); // Update GPU scheduling UI
            UpdateIOPriorityComboBoxForSelectedGame(selectedGame); // Update GPU scheduling UI
        }

        private bool isUpdatingComboBox = false;

        private void UpdateComboBoxForSelectedGame(Game selectedGame)
        {
            if (isUpdatingComboBox || PriorityComboBox.Items.Count == 0) return;

            isUpdatingComboBox = true;

            var matchingItem = PriorityComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.Priority);

            PriorityComboBox.SelectedItem = matchingItem ?? PriorityComboBox.Items[2]; // Default to "Normal"

            isUpdatingComboBox = false;
        }

        private void UpdateGPUSchedulingComboBoxForSelectedGame(Game selectedGame)
        {
            if (GPUSchedulingComboBox.Items.Count == 0) return;

            var matchingItem = GPUSchedulingComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.GPUScheduling);

            GPUSchedulingComboBox.SelectedItem = matchingItem ?? GPUSchedulingComboBox.Items[0]; // Default to "Default"
        }


        private void UpdateIOPriorityComboBoxForSelectedGame(Game selectedGame)
        {
            if (IOPriorityComboBox.Items.Count == 0) return;

            // Find the matching ComboBoxItem based on the IOPriority of the selected game
            var matchingItem = IOPriorityComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content?.ToString() == selectedGame.IOPriority);

            // Set the selected item of the ComboBox or default to the first item if no match is found
            IOPriorityComboBox.SelectedItem = matchingItem ?? IOPriorityComboBox.Items[0]; // Default to "Normal"
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

        private void IOPriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IOPriorityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedPriority = selectedItem.Content.ToString() ?? "Normal";
                // Update the selected game's I/O priority (assuming you have the currently selected game)
                if (GameListView.SelectedItem is Game selectedGame)
                {
                    selectedGame.IOPriority = selectedPriority;
                }
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
                _ => 0
            };
        }






        public class Card
        {
            public string? Title { get; set; }
            public string? ExecutableName { get; set; }
            public string? Description { get; set; }
            public string? ImageSource { get; set; } // Add this property
        }


        private void InitializeCards()
        {
            cards = new ObservableCollection<Card>
    {
        new Card { Title = "CS2", ExecutableName = "cs2.exe", Description = "", ImageSource = "pack://application:,,,/Images/cs2.png" },
        new Card { Title = "Valorant", ExecutableName = "VALORANT-Win64-Shipping.exe", Description = "", ImageSource = "pack://application:,,,/Images/valorant.png" },
        new Card { Title = "R6S (DX12)", ExecutableName = "RainbowSix_BE.exe", Description = "", ImageSource = "pack://application:,,,/Images/r6s.png" },
        new Card { Title = "GTAV", ExecutableName = "GTA5.exe", Description = "", ImageSource = "pack://application:,,,/Images/gtav.png" },
        new Card { Title = "FiveM", ExecutableName = "fivem.exe", Description = "", ImageSource = "pack://application:,,,/Images/fivem.png" },
        new Card { Title = "PB", ExecutableName = "PointBlank.exe", Description = "", ImageSource = "pack://application:,,,/Images/pb.png" },
        new Card { Title = "SCUM", ExecutableName = "SCUM.exe", Description = "", ImageSource = "pack://application:,,,/Images/scum.png" },
        new Card { Title = "EFT", ExecutableName = "EscapeFromTarkov.exe", Description = "", ImageSource = "pack://application:,,,/Images/eft.png" },
        new Card { Title = "The Finals", ExecutableName = "Discovery.exe", Description = "", ImageSource = "pack://application:,,,/Images/tfn.png" },
        new Card { Title = "NarakaBP", ExecutableName = "NarakaBladepoint.exe", Description = "", ImageSource = "pack://application:,,,/Images/nrkbp.png" },
        new Card { Title = "ดบดล", ExecutableName = "DeadByDaylight-Win64-Shipping.exe", Description = "", ImageSource = "pack://application:,,,/Images/dbd.png" },
        new Card { Title = "Fortnite", ExecutableName = "FortniteClient-Win64-Shipping.exe", Description = "", ImageSource = "pack://application:,,,/Images/fortnite.png" },
        new Card { Title = "Osu!", ExecutableName = "osu!.exe", Description = "", ImageSource = "pack://application:,,,/Images/osu.png" },
        new Card { Title = "MW3", ExecutableName = "cod23-cod.exe", Description = "", ImageSource = "pack://application:,,,/Images/mw3.png" },
        new Card { Title = "BO6", ExecutableName = "mp24-cod.exe", Description = "", ImageSource = "pack://application:,,,/Images/bo6.png" },
        new Card { Title = "Apex Legends", ExecutableName = "r5apex.exe", Description = "", ImageSource = "pack://application:,,,/Images/apex.png" },
        new Card { Title = "PUBG", ExecutableName = "TslGame.exe", Description = "", ImageSource = "pack://application:,,,/Images/pubg.png" },
        new Card { Title = "MC (Java)", ExecutableName = "javaw.exe", Description = "", ImageSource = "pack://application:,,,/Images/minecraft.png" },
        new Card { Title = "Roblox", ExecutableName = "RobloxPlayerBeta.exe", Description = "", ImageSource = "pack://application:,,,/Images/roblox.png" },



            };


        }
        public ObservableCollection<Card> Cards => cards; // Property to access the cards collection











        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.DataContext is Card currentCard)
            {
                var gameExecutable = currentCard.ExecutableName;

                if (!string.IsNullOrEmpty(gameExecutable))
                {
                    ApplySettings(gameExecutable);
                    MessageBox.Show($"You have applied settings for {gameExecutable}.");
                }
                else
                {
                    MessageBox.Show("Game executable is not available.");
                }
            }
        }

        private void ApplySettings(string gameExecutable)
        {
            try
            {
                // Path for PerfOptions
                var perfKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{gameExecutable}\PerfOptions";
                using (var perfKey = Registry.LocalMachine.CreateSubKey(perfKeyPath))
                {
                    if (perfKey != null)
                    {
                        perfKey.DeleteValue("UseLargePages", false);
                        perfKey.SetValue("CpuPriorityClass", 6);
                        perfKey.SetValue("IOPriority", 3);
                        perfKey.SetValue("DisableHeapCoalesceOnFree", 1);
                        perfKey.SetValue("GPUScheduling", 1);
                        perfKey.SetValue("HackFlags", 1);
                        perfKey.SetValue("LargePageSizeInBytes", 16);


                    }
                }

                // Path for Image File Execution Options
                var mainKeyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{gameExecutable}";
                using (var mainKey = Registry.LocalMachine.CreateSubKey(mainKeyPath))
                {
                    if (mainKey != null)
                    {
                        mainKey.DeleteValue("UseLargePages", false);
                        mainKey.SetValue("DisableHeapCoalesceOnFree", 1);
                        mainKey.SetValue("GPUScheduling", 1);
                        mainKey.SetValue("HackFlags", 1);
                        mainKey.SetValue("LargePageSizeInBytes", 16);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}");
            }
        }


        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton && clickedButton.DataContext is Card currentCard)
            {
                var gameExecutable = currentCard.ExecutableName;

                if (!string.IsNullOrEmpty(gameExecutable))
                {
                    RevertSettings(gameExecutable);
                    MessageBox.Show($"Settings for {gameExecutable} have been reverted.");
                }
                else
                {
                    MessageBox.Show("Game executable is not available.");
                }
            }
        }

        private void RevertSettings(string gameExecutable)
        {
            try
            {
                // Remove PerfOptions settings
                using (var perfKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{gameExecutable}\PerfOptions", true))
                {
                    if (perfKey != null)
                    {
                        perfKey.DeleteValue("CpuPriorityClass", false);
                        perfKey.DeleteValue("IOPriority", false);
                        perfKey.DeleteValue("DisableHeapCoalesceOnFree", false);
                        perfKey.DeleteValue("GPUScheduling", false);
                        perfKey.DeleteValue("UseLargePages", false);
                        perfKey.DeleteValue("HackFlags", false);
                        perfKey.DeleteValue("LargePageSizeInBytes", false);


                    }
                }

                // Remove main settings
                using (var mainKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{gameExecutable}", true))
                {
                    if (mainKey != null)
                    {
                        mainKey.DeleteValue("DisableHeapCoalesceOnFree", false);
                        mainKey.DeleteValue("GPUScheduling", false);
                        mainKey.DeleteValue("UseLargePages", false);
                        mainKey.DeleteValue("HackFlags", false);
                        mainKey.DeleteValue("LargePageSizeInBytes", false);


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reverting settings: {ex.Message}");
            }
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
                GPUScheduling = "Default", // Set this to a valid default value
                IOPriority = "Normal"
            };

            games.Add(newGame);



            // Set default value
            SetUseLargePagesForNewGame(newGame, false); // Default to false, change as needed
            SetDisableHeapCoalesceForNewGame(newGame, false); // Default to false
            SetHackFlagsForNewGame(newGame, false); // Default to false
            SetLargePageSizeInBytesForNewGame(newGame, false); // Default to false



            // Automatically select the newly added game
            GameListView.SelectedItem = newGame;
            UpdateComboBoxForSelectedGame(newGame);

            // Initialize Use Large Pages toggle
            InitializeUseLargePagesToggle(newGame);
            InitializeDisableHeapCoalesceToggle(newGame);
            InitializeLargePageSizeInBytesToggle(newGame);

        }


        private void SetLargePageSizeInBytesForNewGame(Game game, bool LargePageSizeInBytesEnabled)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{game.Name}";
            using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
            {
                if (gameKey != null)
                {
                    if (LargePageSizeInBytesEnabled)
                    {
                        gameKey.SetValue("LargePageSizeInBytes", 16, RegistryValueKind.DWord);
                    }
                    else
                    {
                        gameKey.DeleteValue("HackFlags", false); // Remove LargePageSizeInBytes if disabled
                    }
                }
            }
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

        private void SetHackFlagsForNewGame(Game game, bool hackFlagsEnabled)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{game.Name}";
            using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
            {
                if (gameKey != null)
                {
                    if (hackFlagsEnabled)
                    {
                        gameKey.SetValue("HackFlags", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        gameKey.DeleteValue("HackFlags", false); // Remove HackFlags if disabled
                    }
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

                // Display a warning if large pages are enabled
                if (useLargePages)
                {
                    var result = MessageBox.Show("Enabling 'Use Large Pages' may cause some games to not run properly or show errors. Do you still want to continue?",
                                                 "Warning",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        // If user selects No, reset the toggle and exit
                        UseLargePagesToggle.IsChecked = false;
                        return;
                    }
                }

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



        private void HackFlagsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}";

                try
                {
                    using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
                    {
                        if (gameKey != null)
                        {
                            if (HackFlagsToggle.IsChecked == true)
                            {
                                // Set the HackFlags DWORD to 1
                                gameKey.SetValue("HackFlags", 1, RegistryValueKind.DWord);
                                App.changelogUserControl?.AddLog("Applied", $"HackFlags for {selectedGame.Name} enabled.");
                            }
                            else
                            {
                                // Delete the HackFlags DWORD
                                gameKey.DeleteValue("HackFlags", false);
                                App.changelogUserControl?.AddLog("Applied", $"HackFlags for {selectedGame.Name} disabled.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating HackFlags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", $"Error updating HackFlags: {ex.Message}");
                }
            }
        }

        private void LargePageSizeInBytesToggle_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}";

                try
                {
                    using (var gameKey = Registry.LocalMachine.CreateSubKey(registryPath, true))
                    {
                        if (gameKey != null)
                        {
                            if (LargePageSizeInBytesToggle.IsChecked == true)
                            {
                                // Set the LargePageSizeInBytes DWORD to 16
                                gameKey.SetValue("LargePageSizeInBytes", 16, RegistryValueKind.DWord);
                                App.changelogUserControl?.AddLog("Applied", $"LargePageSizeInBytes for {selectedGame.Name} enabled.");
                            }
                            else
                            {
                                // Delete the LargePageSizeInBytes DWORD
                                gameKey.DeleteValue("LargePageSizeInBytes", false);
                                App.changelogUserControl?.AddLog("Applied", $"LargePageSizeInBytes for {selectedGame.Name} disabled.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating LargePageSizeInBytes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    App.changelogUserControl?.AddLog("Failed", $"Error updating LargePageSizeInBytes: {ex.Message}");
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

                        }
                        else
                        {
                            UseLargePagesToggle.IsChecked = false; // Default if value is not set

                        }
                    }
                    else
                    {
                        UseLargePagesToggle.IsChecked = false; // No key found

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

                        }
                        else
                        {
                            DisableHeapCoalesceToggle.IsChecked = false; // Default if value is not set

                        }
                    }
                    else
                    {
                        DisableHeapCoalesceToggle.IsChecked = false; // No key found

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

        private void InitializeHackFlagsToggle(Game selectedGame)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}";

            try
            {
                using (var gameKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (gameKey != null)
                    {
                        object? value = gameKey.GetValue("HackFlags");

                        // Check if the value is an integer and set the toggle accordingly
                        if (value is int intValue)
                        {
                            HackFlagsToggle.IsChecked = intValue == 1; // Set toggle based on registry value
                        }
                        else
                        {
                            HackFlagsToggle.IsChecked = false; // Default if value is not set
                        }
                    }
                    else
                    {
                        HackFlagsToggle.IsChecked = false; // No key found
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

        private void InitializeLargePageSizeInBytesToggle(Game selectedGame)
        {
            string registryPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{selectedGame.Name}";

            try
            {
                using (var gameKey = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (gameKey != null)
                    {
                        object? value = gameKey.GetValue("LargePageSizeInBytes");

                        // Check if the value is an integer and set the toggle accordingly
                        if (value is int intValue)
                        {
                            LargePageSizeInBytesToggle.IsChecked = intValue == 16; // Set toggle based on registry value
                        }
                        else
                        {
                            LargePageSizeInBytesToggle.IsChecked = false; // Default if value is not set
                        }
                    }
                    else
                    {
                        LargePageSizeInBytesToggle.IsChecked = false; // No key found
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







        private void ResetPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameListView.SelectedItem is Game selectedGame)
            {
                selectedGame.Priority = "Normal";
                selectedGame.GPUScheduling = "Default";
                selectedGame.IOPriority = "Normal";
                UseLargePagesToggle.IsChecked = false;
                DisableHeapCoalesceToggle.IsChecked = false;
                HackFlagsToggle.IsChecked = false;
                LargePageSizeInBytesToggle.IsChecked = false;

                GameListView.Items.Refresh();

                selectedGame.GPUScheduling = "Default"; // Set to "Default"

                // Explicitly set ComboBox selection to match the new GPUScheduling
                GPUSchedulingComboBox.SelectedItem = "Default";
                // Update ComboBox selection to reflect the reset
                UpdateComboBoxForSelectedGame(selectedGame);



            }
        }

        private void SavePriorityButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var game in games)
            {
                // Get the CPU and I/O priority values separately
                int cpuPriorityValue = GetPriorityValue(game.Priority);
                int ioPriorityValue = GetPriorityValue(game.IOPriority);

                // Validate CPU priority
                if (cpuPriorityValue == 0)
                {
                    App.changelogUserControl?.AddLog("Invalid", $"Invalid CPU priority for {game.Name}.");
                    MessageBox.Show($"Invalid CPU priority for {game.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                // Validate I/O priority
                if (ioPriorityValue == 0)
                {
                    App.changelogUserControl?.AddLog("Invalid", $"Invalid I/O priority for {game.Name}.");
                    MessageBox.Show($"Invalid I/O priority for {game.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    // Set CPU priority
                                    perfOptionsKey.SetValue("CpuPriorityClass", cpuPriorityValue, RegistryValueKind.DWord);

                                    // Set I/O priority
                                    perfOptionsKey.SetValue("IOPriority", ioPriorityValue, RegistryValueKind.DWord);

                                    // Save Use Large Pages setting for the game
                                    bool useLargePages = UseLargePagesToggle.IsChecked == true;
                                    perfOptionsKey.SetValue("UseLargePages", useLargePages ? 1 : 0, RegistryValueKind.DWord);

                                    // Save GPU Scheduling setting
                                    int gpuSchedulingValue = game.GPUScheduling == "High Performance" ? 1 : 0;
                                    perfOptionsKey.SetValue("GPUScheduling", gpuSchedulingValue, RegistryValueKind.DWord);

                                    // Log and show message for Use Large Pages
                                    string largePagesMessage = useLargePages ? "enabled" : "disabled";
                                    App.changelogUserControl?.AddLog("Applied", $"Use Large Pages for {game.Name} set to {largePagesMessage}");

                                    // Save Disable Heap Coalesce setting
                                    bool disableHeapCoalesce = DisableHeapCoalesceToggle.IsChecked == true;
                                    perfOptionsKey.SetValue("DisableHeapCoalesceOnFree", disableHeapCoalesce ? 1 : 0, RegistryValueKind.DWord);
                                    App.changelogUserControl?.AddLog("Applied", $"Disable Heap Coalesce on Free for {game.Name} set to {(disableHeapCoalesce ? "enabled" : "disabled")}");

                                    // Save HackFlags setting
                                    bool hackFlagsEnabled = HackFlagsToggle.IsChecked == true;
                                    if (hackFlagsEnabled)
                                    {
                                        perfOptionsKey.SetValue("HackFlags", 1, RegistryValueKind.DWord);
                                        App.changelogUserControl?.AddLog("Applied", $"HackFlags for {game.Name} set to enabled.");
                                    }
                                    else
                                    {
                                        perfOptionsKey.DeleteValue("HackFlags", false);
                                        App.changelogUserControl?.AddLog("Applied", $"HackFlags for {game.Name} set to disabled.");
                                    }

                                    // Save LargePageSizeInBytes setting
                                    bool LargePageSizeInBytesEnabled = LargePageSizeInBytesToggle.IsChecked == true;
                                    string LargePageSizeInBytesMessage = LargePageSizeInBytesEnabled ? "enabled" : "disabled";
                                    if (hackFlagsEnabled)
                                    {
                                        perfOptionsKey.SetValue("LargePageSizeInBytes", 16, RegistryValueKind.DWord);
                                        App.changelogUserControl?.AddLog("Applied", $"LargePageSizeInBytes for {game.Name} set to enabled.");
                                    }
                                    else
                                    {
                                        perfOptionsKey.DeleteValue("LargePageSizeInBytes", false);
                                        App.changelogUserControl?.AddLog("Applied", $"LargePageSizeInBytes for {game.Name} set to disabled.");
                                    }

                                    // Concatenate all the messages into one string for display
                                    string message = $"Disable Heap Coalesce on Free for {game.Name} has been {(disableHeapCoalesce ? "enabled" : "disabled")}.\n" +
                                                     $"GPU Scheduling for {game.Name} set to {(gpuSchedulingValue == 1 ? "High Performance" : "Default")}.\n" +
                                                     $"CPU Priority for {game.Name} set to {game.Priority}.\n" +
                                                     $"IO Priority for {game.Name} set to {game.IOPriority}.\n" +
                                                     $"LargePageSizeInBytes for {game.Name} set to {LargePageSizeInBytesMessage}.\n" +
                                                     $"Use Large Pages for {game.Name} has been {largePagesMessage}.";

                                    // Show all messages in one message box
                                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Log the changes for clarity
                                    App.changelogUserControl?.AddLog("Applied", $"CPU Priority for {game.Name} set to {game.Priority}.");
                                    App.changelogUserControl?.AddLog("Applied", $"IOPriority for {game.Name} set to {game.IOPriority}.");

                                    // Notify main window that settings have been applied
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
