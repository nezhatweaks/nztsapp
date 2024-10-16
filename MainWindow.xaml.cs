using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NZTS_App.Views;
using System.IO;
using System.Drawing;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Globalization;





namespace NZTS_App
{

    
    public partial class MainWindow : Window
    {
        private bool settingsApplied = false;
        public enum ValueType
        {
            DWord,
            String
        }

        Dictionary<string, Tuple<string, string, ValueType, bool, string>> registryTweaks = new Dictionary<string, Tuple<string, string, ValueType, bool, string>>()
{
    {"Win32PrioritySeparation", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "00fa332a", ValueType.DWord, false, "00000002")}, // default is 2
    {"ContextSwitchDeadband", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, true, "00000000")}, // default is none
    {"LatencySensitivityHint", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, true, "00000000")}, // default is none
    {"SystemResponsiveness", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "00000000", ValueType.DWord, false, "00000002")},
    {"NoLazyMode", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "00000001", ValueType.DWord, false, "00000000")},
    {"LazyModeTimeout", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "0098967f", ValueType.DWord, false, "00005000")},
    {"Start", new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MMCSS", "00000002", ValueType.DWord, false, "00000002")}
};











        public MainWindow()
        {
            InitializeComponent();
            LoadGames();
            this.SizeChanged += MainWindow_SizeChanged; // Subscribe to size changed event
                                                        // Show the game content and hide other content on startup
            var welcomeControl = new WelcomeUserControl();
            welcomeControl.OptimizeAllClicked += WelcomeControl_OptimizeAllClicked;
            welcomeControl.RestoreAllClicked += WelcomeControl_RestoreAllClicked;

        }



        private void WelcomeControl_OptimizeAllClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var tweak in registryTweaks.Keys)
                {
                    ApplyTweak(tweak);
                }

                MessageBox.Show("All optimizations have been applied successfully!");

                // Prompt the user to restart the computer
                var result = MessageBox.Show("Changes have been applied. Would you like to restart your computer now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Restart the computer
                    System.Diagnostics.Process.Start("shutdown", "/r /t 0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during optimization: {ex.Message}");
            }
        }

        private void WelcomeControl_RestoreAllClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("RestoreAll_Click started.");

            foreach (var key in registryTweaks.Keys)
            {
                RestoreDefault(key);
                Console.WriteLine($"Processed {key}");
            }

            Console.WriteLine("RestoreAll_Click completed.");
            MessageBox.Show("All settings have been restored to default.");

            // Prompt the user to restart the computer
            var result = MessageBox.Show("Settings have been restored to default. Would you like to restart your computer now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Restart the computer
                System.Diagnostics.Process.Start("shutdown", "/r /t 0");
            }
        }

        private void OptimizeAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var tweak in registryTweaks.Keys)
                {
                    ApplyTweak(tweak);
                }

                MessageBox.Show("All optimizations have been applied successfully!");

                // Prompt the user to restart the computer
                var result = MessageBox.Show("Changes have been applied. Would you like to restart your computer now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Restart the computer
                    System.Diagnostics.Process.Start("shutdown", "/r /t 0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during optimization: {ex.Message}");
            }
        }

        private void RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("RestoreAll_Click started.");

            foreach (var key in registryTweaks.Keys)
            {
                RestoreDefault(key);
                Console.WriteLine($"Processed {key}");
            }

            Console.WriteLine("RestoreAll_Click completed.");
            MessageBox.Show("All settings have been restored to default.");

            // Prompt the user to restart the computer
            var result = MessageBox.Show("Settings have been restored to default. Would you like to restart your computer now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Restart the computer
                System.Diagnostics.Process.Start("shutdown", "/r /t 0");
            }
        }





        public void ApplyTweak(string key)
        {
            if (registryTweaks.ContainsKey(key))
            {
                var tweakData = registryTweaks[key];
                string path = tweakData.Item1;
                string value = tweakData.Item2;
                ValueType valueType = tweakData.Item3;

                try
                {
                    if (valueType == ValueType.DWord)
                    {
                        // Parse the value as a DWORD (integer)
                        int intValue = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
                        Registry.SetValue(path, key, intValue, RegistryValueKind.DWord);
                    }
                    else if (valueType == ValueType.String)
                    {
                        Registry.SetValue(path, key, value);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error applying {key}: {ex.Message}");
                }
            }
        }


        // Reset all tweaks
        public void ResetTweaks()
        {
            foreach (var tweak in registryTweaks.Keys)
            {
                RestoreDefault(tweak);
            }
        }



        


        public void RestoreDefault(string key)
        {
            if (registryTweaks.ContainsKey(key))
            {
                var tweakData = registryTweaks[key];
                string fullPath = tweakData.Item1;
                string defaultValue = tweakData.Item5; // Default value
                bool shouldDelete = tweakData.Item4; // Deletion flag
                ValueType valueType = tweakData.Item3; // Value type

                try
                {
                    if (shouldDelete)
                    {
                        // Use only the subkey part for deletion (without HKEY_LOCAL_MACHINE)
                        string subKeyPath = fullPath.Replace(@"HKEY_LOCAL_MACHINE\", "");

                        using (var regKey = Registry.LocalMachine.OpenSubKey(subKeyPath, true))
                        {
                            if (regKey != null && regKey.GetValue(key) != null)
                            {
                                regKey.DeleteValue(key, false); // Use false to suppress exception if it doesn't exist
                                Console.WriteLine($"Successfully deleted {key} from {subKeyPath}.");
                            }
                            else
                            {
                                Console.WriteLine($"Key {key} does not exist; nothing to delete.");
                            }
                        }
                    }
                    else
                    {
                        // Restore default value (uses full path)
                        if (valueType == ValueType.DWord)
                        {
                            uint uintDefaultValue = Convert.ToUInt32(defaultValue, 16); // Convert hex string to uint
                            Registry.SetValue(fullPath, key, uintDefaultValue, RegistryValueKind.DWord);
                            Console.WriteLine($"Successfully restored {key} to {defaultValue} at {fullPath}.");
                        }
                        else if (valueType == ValueType.String)
                        {
                            Registry.SetValue(fullPath, key, defaultValue, RegistryValueKind.String);
                            Console.WriteLine($"Successfully restored {key} to {defaultValue} at {fullPath}.");
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied for {key} at {fullPath}. Check permissions.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting {key}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {key} not found in registryTweaks.");
            }
        }



        private void DeleteRegistryKey(string path, string key, bool shouldDelete)
        {
            if (shouldDelete)
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey(path, true))
                {
                    if (regKey != null)
                    {
                        // Log existing values
                        MessageBox.Show($"Existing values in {path}:");
                        foreach (var valueName in regKey.GetValueNames())
                        {
                            Console.WriteLine($" - {valueName}: {regKey.GetValue(valueName)}");
                        }

                        // Retrieve the value and check if it exists
                        var value = regKey.GetValue(key);
                        if (value is bool boolValue && !boolValue)
                        {
                            MessageBox.Show($"Key {key} is false in the registry; nothing to delete.");
                        }
                        else
                        {
                            if (value != null)
                            {
                                regKey.DeleteValue(key, false);
                                MessageBox.Show($"Successfully deleted {key} from {path}.");
                            }
                            else
                            {
                                MessageBox.Show($"Key {key} does not exist; nothing to delete.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: Registry path not found: {path}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Deletion not performed as the condition was false.");
            }
        }




        private void RestoreRegistryValue(string path, string key, string defaultValue, ValueType valueType)
        {
            try
            {
                if (valueType == ValueType.DWord)
                {
                    uint uintDefaultValue = Convert.ToUInt32(defaultValue, 16);
                    Registry.SetValue(path, key, uintDefaultValue, RegistryValueKind.DWord);
                    Console.WriteLine($"Successfully restored {key} to {defaultValue} at {path}.");
                }
                else if (valueType == ValueType.String)
                {
                    Registry.SetValue(path, key, defaultValue, RegistryValueKind.String);
                    Console.WriteLine($"Successfully restored {key} to {defaultValue} at {path}.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine($"Error: Default value '{defaultValue}' is not in the correct format for {valueType}.");
            }
        }

























        public void RestoreAllDefaults()
        {
            foreach (var key in registryTweaks.Keys)
            {
                RestoreDefault(key);
            }
        }





        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            // Iterate through each button in the sidebar
            foreach (var child in Sidebar.Children)
            {
                if (child is Button button)
                {
                    // Safely get the button content
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var buttonContent = button.Content?.ToString().ToLower() ?? string.Empty;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    // Set button visibility based on search text
                    button.Visibility = buttonContent.Contains(searchText)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }

        
    













private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Placeholder.Visibility = Visibility.Collapsed; // Hide placeholder when focused
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Show placeholder if the TextBox is empty when losing focus
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                Placeholder.Visibility = Visibility.Visible;
            }
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingsApplied)
            {
                var result = MessageBox.Show("You have applied settings that require a restart. Do you want to restart now?",
                                              "Restart Required",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("shutdown", "/r /t 0");
                }
            }
        }

        // Method to mark that settings have been applied
        public void MarkSettingsApplied()
        {
            settingsApplied = true;
        }







        private void Win32PrioritySeparation_Click(object sender, RoutedEventArgs e)
        {
            var win32Control = new Views.Win32PrioritySeparation(this);
            ShowContentWithAnimation(win32Control); // Call the method without animation
            
        }

        private void ContextSwitchDeadband_Click(object sender, RoutedEventArgs e)
        {
            // Load ContextSwitchDeadband UserControl with MainWindow reference
            var deadbandControl = new ContextSwitchDeadband(this);  // Pass 'this' as the MainWindow reference
            ShowContentWithAnimation(deadbandControl);
        }


        private void LatencySensitivityHint_Click(object sender, RoutedEventArgs e)
        {
            // Load LatencySensitivityHint UserControl
            var latencyControl = new LatencySensitivityHint(this);  // Updated path
            ShowContentWithAnimation(latencyControl);
        }

        private void PowerPlan_Click(object sender, RoutedEventArgs e)
        {
            var powerPlanControl = new PowerPlan();
            ShowContentWithAnimation(powerPlanControl); // Call the method to display with animation
        }

        private void MMCSS_Click(object sender, RoutedEventArgs e)
        {
            var mmcssControl = new MMCSS(this);
            ShowContentWithAnimation(mmcssControl); // Call the method to display with animation
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsControl = new SettingsUserControl(); // Use the correct class name
            ShowContentWithAnimation(settingsControl); // Call the method to display with animation
        }



        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Show the welcome page
            var welcomeControl = new WelcomeUserControl(); // Create a new instance of the Welcome UserControl
            ShowContentWithAnimation(welcomeControl); // Call the method to show the content
            welcomeControl.OptimizeAllClicked += WelcomeControl_OptimizeAllClicked;
            welcomeControl.RestoreAllClicked += WelcomeControl_RestoreAllClicked;
            ContentArea.Visibility = Visibility.Visible;
            ContentOther.Visibility = Visibility.Collapsed;
        }

        
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Adjust the height of the horizontal thumb based on window size
            HorizontalResizeThumb.Width = this.ActualWidth; // Stretch across the bottom
            VerticalResizeThumb.Height = this.ActualHeight; // Stretch along the right
        }

        private bool isResizingHorizontal = false;
        private bool isResizingVertical = false;

        // MouseDown event to start resizing
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isResizingVertical)
                {
                    Mouse.Capture(this); // Capture mouse for vertical resizing
                }
                else if (isResizingHorizontal)
                {
                    Mouse.Capture(this); // Capture mouse for horizontal resizing
                }
            }
        }

        // MouseMove event to detect edge and initiate resizing
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            bool isInVerticalResizeZone = mousePos.X >= this.ActualWidth - 20 && mousePos.Y > 0 && mousePos.Y < this.ActualHeight;
            bool isInHorizontalResizeZone = mousePos.Y >= this.ActualHeight - 20 && mousePos.X > 0 && mousePos.X < this.ActualWidth;

            // Update cursor and resizing flags
            if (isInVerticalResizeZone)
            {
                Cursor = Cursors.SizeWE;
                isResizingVertical = true;
                isResizingHorizontal = false;
            }
            else if (isInHorizontalResizeZone)
            {
                Cursor = Cursors.SizeNS;
                isResizingHorizontal = true;
                isResizingVertical = false;
            }
            else
            {
                Cursor = Cursors.Arrow;
                isResizingHorizontal = false;
                isResizingVertical = false;
            }

            // Handle resizing if the mouse is captured
            if (Mouse.Captured == this)
            {
                if (isResizingVertical)
                {
                    double newWidth = this.Width + e.GetPosition(this).X - this.ActualWidth;
                    if (newWidth > 400) // Set a minimum width
                        this.Width = newWidth;
                }
                else if (isResizingHorizontal)
                {
                    double newHeight = this.Height + e.GetPosition(this).Y - this.ActualHeight;
                    if (newHeight > 300) // Set a minimum height
                        this.Height = newHeight;
                }
            }
        }


        // MouseUp event to release mouse capture
        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null); // Release mouse capture when done
            isResizingHorizontal = false;
            isResizingVertical = false;
        }

        // Thumbs DragDelta events for additional resizing
        private void VerticalResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            if (newWidth > 400) // minimum width
            {
                this.Width = newWidth;
            }
        }

        private void HorizontalResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = this.Height + e.VerticalChange;
            if (newHeight > 300) // minimum height
            {
                this.Height = newHeight;
            }
        }




        

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();  // Allow window dragging
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();  // Close the application
        }


        private void ToggleSidebar(object sender, RoutedEventArgs e)
        {
            if (Sidebar.Visibility == Visibility.Collapsed)
            {
                Sidebar.Visibility = Visibility.Visible;
                
            }
            else
            {
                Sidebar.Visibility = Visibility.Collapsed;
                
            }
        }




        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if the left mouse button is pressed
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }



        private void MainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Sidebar.Visibility == Visibility.Visible && !IsClickInsideSidebar(e))
            {
                Sidebar.Visibility = Visibility.Collapsed;
            }
        }


        private bool IsClickInsideSidebar(MouseButtonEventArgs e)
        {
            // Get the position of the click relative to the sidebar
            var mousePosition = e.GetPosition(Sidebar);

            // Check if the click is within the sidebar bounds
            return mousePosition.X >= 0 && mousePosition.X <= Sidebar.ActualWidth &&
                   mousePosition.Y >= 0 && mousePosition.Y <= Sidebar.ActualHeight;
        }

        private void Sidebar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Sidebar.Visibility == Visibility.Visible)
            {
                Sidebar.Visibility = Visibility.Collapsed;
            }
        }





        private void ShowContentWithAnimation(UserControl newContent)
        {
            // Directly switch the content without animation for testing
            ContentArea.Content = newContent;

            // Set opacity for the new content
            newContent.Opacity = 1; // Ensure it's fully visible
        }

        private void LoadGames()
        {
            var games = GetInstalledGames() ?? new List<string>(); // Ensure it's not null
            foreach (var game in games)
            {
                AddGameIcon(game, "path/to/your/icon.png"); // Adjust this path or method as necessary
            }
        }

        private void AddGameIcon(string name, string iconPath)
        {
            var border = new Border
            {
                Margin = new Thickness(10),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 63, 66)), // Specify the correct Color
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(5)
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var image = new System.Windows.Controls.Image // Specify the correct Image
            {
                Source = GetIconBitmapSource(iconPath),
                Width = 100,
                Height = 100
            };

            var textBlock = new TextBlock
            {
                Text = name,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.White, // Specify the correct Brushes
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);
            border.Child = stackPanel;
            
        }

        private BitmapSource GetIconBitmapSource(string path)
        {
            try
            {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                {
                    if (icon != null)
                    {
                        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found
                // Optionally log the error or return a default icon
                Console.WriteLine($"File not found: {path}");
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access denied exceptions
                Console.WriteLine($"Access denied: {path}");
            }
            catch (Exception ex)
            {
                // Catch other exceptions for robustness
                Console.WriteLine($"An error occurred while extracting icon: {ex.Message}");
            }

            return CreateDefaultIcon(); // Return a default icon if any errors occur
        }


        private BitmapSource CreateDefaultIcon()
        {
            var defaultIcon = new System.Drawing.Bitmap(100, 100);
            using (var graphics = System.Drawing.Graphics.FromImage(defaultIcon))
            {
                graphics.Clear(System.Drawing.Color.Gray); // Set the default icon color
            }

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                defaultIcon.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private List<string> GetInstalledGames()
        {
            List<string> games = new List<string>();

            // Check for 64-bit installed programs
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                // Get the display name safely
                                var displayName = subKey.GetValue("DisplayName") as string;
                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    games.Add(displayName);
                                }
                            }
                        }
                    }
                }
            }

            // Check for 32-bit installed programs on a 64-bit OS
            registryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

            using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            if (subKey != null)
                            {
                                // Get the display name safely
                                var displayName = subKey.GetValue("DisplayName") as string;
                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    games.Add(displayName);
                                }
                            }
                        }
                    }
                }
            }

            return games;
        }







    }
}









 

