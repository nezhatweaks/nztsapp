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
using System.Diagnostics;






namespace NZTS_App
{

    

    public partial class MainWindow : Window
    {
        private bool settingsApplied = false; // Existing flag for settings
        private bool isClosing = false; // New flag to prevent re-entrance
        
        public enum ValueType
        {
            DWord,
            String
        }

        Dictionary<string, List<Tuple<string, string, ValueType, bool, string>>> registryTweaks = new Dictionary<string, List<Tuple<string, string, ValueType, bool, string>>>()
{
    {"Win32PrioritySeparation", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "00fa332a", ValueType.DWord, false, "00000002") // default is 2
    }},
    {"ContextSwitchDeadband", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, true, "00000000") // default is none
    }},
    {"LatencySensitivityHint", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, true, "00000000") // default is none
    }},
    {"SystemResponsiveness", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "00000000", ValueType.DWord, false, "00000002")
    }},
    {"NoLazyMode", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"LazyModeTimeout", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "0098967f", ValueType.DWord, false, "00005000")
    }},
    {"DisableDynamicPstate", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000001", ValueType.DWord, false, "00000000") // default is 0
    }},
    {"EnableUlps", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001") // default is 1
    }},
    {"PP_ThermalAutoThrottlingEnable", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001") // default is 1
    }},
    {"Start", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MMCSS", "00000002", ValueType.DWord, false, "00000002")
    }},
    {"VsyncIdleTimeout", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler", "00000000", ValueType.DWord, false, "00000001")
    }},
    {"MonitorLatencyTolerance", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"MonitorRefreshLatencyTolerance", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisablePagingExecutive", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisableHeapCoalesceOnFree", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"AllowMaxPerf", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\nvlddmkm", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"Enabled", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "00000000", ValueType.DWord, false, "00000000")
        
    }},
    {"DisableMshybridNvsrSwitch", new List<Tuple<string, string, ValueType, bool, string>>()
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\nvlddmkm", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisableDMACopy", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisableBlockWrite", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001")
    }},
    {"StutterMode", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001")
    }},
    {"PP_SclkDeepSleepDisable", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisableDrmdmaPowerGating", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000001", ValueType.DWord, false, "00000000")
    }},
    {"DisableFBCForFullScreenApp", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "0", ValueType.String, false, "1")
    }},
    {"DisableFBCSupport", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001")
    }},
    {"EnableAspmL0s", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001")
    }},
    {"EnableAspmL1", new List<Tuple<string, string, ValueType, bool, string>>
    {
        new Tuple<string, string, ValueType, bool, string>(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "00000000", ValueType.DWord, false, "00000001")
    }}
};







        private TranslateTransform translateTransform;


        public MainWindow()
        {
            InitializeComponent();
            translateTransform = new TranslateTransform();
            VerifiedContent.RenderTransform = translateTransform;
            ExperimentalContent.RenderTransform = translateTransform;
            ResourcesContent.RenderTransform = translateTransform;

            // Set default title if needed
            TitleTextBlock.Content = "Home";
            
        }


        public MainWindow(MainWindow window)
        {
            InitializeComponent();
            translateTransform = new TranslateTransform();
            VerifiedContent.RenderTransform = translateTransform;
            ExperimentalContent.RenderTransform = translateTransform;
            ResourcesContent.RenderTransform = translateTransform;

            TitleTextBlock.Content = window.TitleTextBlock.Content; // Example usage

            this.SizeChanged += MainWindow_SizeChanged; // Subscribe to size changed event
            this.Closing += Window_Closing;

            
            var welcomeControl = new WelcomeUserControl(this);
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
                var tweakDataList = registryTweaks[key];

                foreach (var tweakData in tweakDataList)
                {
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
                            App.changelogUserControl?.AddLog("Applied", $"Changed the {key} setting to {value} at path {path}.");
                        }
                        else if (valueType == ValueType.String)
                        {
                            Registry.SetValue(path, key, value);
                            App.changelogUserControl?.AddLog("Applied", $"Changed the {key} setting to '{value}' at path {path}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error applying {key} at path {path}: {ex.Message}");
                        App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting at path {path}.");
                    }
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
                var tweakDataList = registryTweaks[key];

                foreach (var tweakData in tweakDataList)
                {
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
                                    App.changelogUserControl?.AddLog("Applied", $"Deleted the {key} setting.");
                                }
                                else
                                {
                                    Console.WriteLine($"Key {key} does not exist; nothing to delete.");
                                    App.changelogUserControl?.AddLog("Failed", $"Key {key} does not exist; nothing to delete.");
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
                                App.changelogUserControl?.AddLog("Restored", $"Changed the {key} setting back to {defaultValue}.");
                            }
                            else if (valueType == ValueType.String)
                            {
                                Registry.SetValue(fullPath, key, defaultValue, RegistryValueKind.String);
                                Console.WriteLine($"Successfully restored {key} to {defaultValue} at {fullPath}.");
                                App.changelogUserControl?.AddLog("Restored", $"Changed the {key} setting back to {defaultValue}.");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Access denied for {key} at {fullPath}. Check permissions.");
                        App.changelogUserControl?.AddLog("Failed", $"Access denied for {key}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error resetting {key}: {ex.Message}");
                        App.changelogUserControl?.AddLog("Failed", $"Error resetting {key}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: {key} not found in registryTweaks.");
                App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting; not found.");
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
                            App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting.");
                        }
                        else
                        {
                            if (value != null)
                            {
                                regKey.DeleteValue(key, false);
                                MessageBox.Show($"Successfully deleted {key} from {path}.");
                                App.changelogUserControl?.AddLog("Deleted", $"Deleted the {key} setting.");
                            }
                            else
                            {
                                MessageBox.Show($"Key {key} does not exist; nothing to delete.");
                                App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: Registry path not found: {path}");
                        App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Deletion not performed as the condition was false.");
                App.changelogUserControl?.AddLog("Failed", $"Unable to change the {key} setting.");
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
                    App.changelogUserControl?.AddLog("Restored", $"Restored the {key} to {defaultValue} setting.");
                }
                else if (valueType == ValueType.String)
                {
                    Registry.SetValue(path, key, defaultValue, RegistryValueKind.String);
                    Console.WriteLine($"Successfully restored {key} to {defaultValue} at {path}.");
                    App.changelogUserControl?.AddLog("Restored", $"Restored the {key} to {defaultValue} setting.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine($"Error: Default value '{defaultValue}' is not in the correct format for {valueType}.");
                App.changelogUserControl?.AddLog("Failed", $"Unable to restore the {key} to {defaultValue} setting.");
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return; // Use pattern matching for safety

            Border? border = FindVisualChild<Border>(button); // Use nullable Border type
            if (border == null) return; // Ensure border is not null

            // Create a new LinearGradientBrush for the border
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(1, 1)
            };

            // Create the color stops for the gradient
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Red, 0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.2));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.4));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Green, 0.6));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.8));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 1));

            // Set the gradient brush as the border's border brush
            border.BorderBrush = gradientBrush;

            // Create and start the color animation
            var animation = new ColorAnimation
            {
                From = Colors.Red,
                To = Colors.Purple,
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            // Apply the animation to the border's background
            if (border.Background is SolidColorBrush solidColorBrush)
            {
                solidColorBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else
            {
                // Create a new SolidColorBrush if the background is not a SolidColorBrush
                var newBrush = new SolidColorBrush(Colors.Transparent);
                border.Background = newBrush;
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
        }

        // Helper method to find child elements
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null; // Ensure parent is not null

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childType)
                {
                    return childType; // Return the child if it matches the type
                }

                // Recursively search for children
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null; // Return null if no child of type T is found
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
            
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Show placeholder if the TextBox is empty when losing focus
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                
            }
        }




        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosing) return; // Prevent re-entrance

            isClosing = true; // Set the flag

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
            if (!settingsApplied) // Check if settings have not been applied yet
            {
                settingsApplied = true;
                
            }
        }

        private bool isDragging = false;
        private System.Windows.Point startPoint;
        private double initialOffsetX;
        private const double swipeThreshold = 50; // Threshold for switching content
        


        private void VerifiedContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);

                // Get the initial offset
                var transform = element.RenderTransform as TranslateTransform;
                initialOffsetX = transform?.X ?? 0;

                Mouse.Capture(element);
                Cursor = Cursors.Hand; // Change cursor to hand
            }
        }

        private void VerifiedContent_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                Vector offset = currentPosition - startPoint;

                // Calculate the new offset
                double newOffsetX = initialOffsetX + offset.X;

                // Implement gradual resistance
                double resistance = 0.5; // Adjust this value for more or less resistance
                double maxDrag = VerifiedContent.ActualWidth * 0.5; // Allow some drag
                double effectiveOffsetX;

                if (newOffsetX < -maxDrag) // If dragging left beyond max
                {
                    effectiveOffsetX = -maxDrag + (newOffsetX + maxDrag) * resistance;
                }
                else if (newOffsetX > maxDrag) // If dragging right beyond max
                {
                    effectiveOffsetX = maxDrag + (newOffsetX - maxDrag) * resistance;
                }
                else
                {
                    effectiveOffsetX = newOffsetX; // Within bounds
                }

                // Apply the transform to move the content smoothly
                var transform = new TranslateTransform(effectiveOffsetX, 0);
                VerifiedContent.RenderTransform = transform;
            }
        }


        private void VerifiedContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            Mouse.Capture(null); // Release mouse capture
            Cursor = Cursors.Arrow; // Reset cursor to default

            double offsetX = ((TranslateTransform)VerifiedContent.RenderTransform).X;

            if (offsetX < -swipeThreshold) // Swipe left
            {
                AnimateTransition(VerifiedContent, ExperimentalContent, ExperimentalButton);
            }
            else
            {
                AnimateBackToOriginalPosition(VerifiedContent);
            }
        }

        private void AnimateTransition(StackPanel currentContent, StackPanel nextContent, Button activeButton)
        {
            // Set the next content's initial state to collapsed
            nextContent.Visibility = Visibility.Collapsed; // Hide it initially
            nextContent.Opacity = 0; // Ensure it's transparent
            nextContent.RenderTransform = new TranslateTransform(0, 0); // Start at original position

            var storyboard = new Storyboard();

            // Fade out the current content
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(fadeOutAnimation, currentContent);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));

            // Translate the current content out to the left
            var translateOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = -currentContent.ActualWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(translateOutAnimation, currentContent.RenderTransform);
            Storyboard.SetTargetProperty(translateOutAnimation, new PropertyPath("(TranslateTransform.X)"));

            // Fade in the next content
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(fadeInAnimation, nextContent);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));

            // Translate the next content in from the right
            var translateInAnimation = new DoubleAnimation
            {
                From = nextContent.ActualWidth, // Start from off-screen to the right
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(translateInAnimation, nextContent.RenderTransform);
            Storyboard.SetTargetProperty(translateInAnimation, new PropertyPath("(TranslateTransform.X)"));

            storyboard.Children.Add(fadeOutAnimation);
            storyboard.Children.Add(translateOutAnimation);
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(translateInAnimation);

            storyboard.Completed += (s, e) =>
            {
                // Collapse the current content after fading out
                currentContent.Visibility = Visibility.Collapsed;
                currentContent.RenderTransform = new TranslateTransform(0, 0); // Reset transform
                ResetContentVisibility(currentContent); // Reset visibility of child elements

                // Set next content to visible after animation completes
                nextContent.Visibility = Visibility.Visible;
                nextContent.Opacity = 1; // Ensure it's fully visible after animation

                // Update the active button based on the next content
                if (nextContent == VerifiedContent)
                    SetActiveButton(VerifiedButton);
                else if (nextContent == ExperimentalContent)
                    SetActiveButton(ExperimentalButton);
                else if (nextContent == ResourcesContent)
                    SetActiveButton(ResourcesButton);
            };

            storyboard.Begin(); // Start the storyboard to run all animations
        }







        private void ExperimentalContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                var transform = element.RenderTransform as TranslateTransform;
                initialOffsetX = transform?.X ?? 0;
                Mouse.Capture(element);
                Cursor = Cursors.Hand; // Change cursor to hand
            }
        }

        private void ExperimentalContent_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                Vector offset = currentPosition - startPoint;

                double newOffsetX = initialOffsetX + offset.X;

                double resistance = 0.5; // Adjust for more or less resistance
                double maxDrag = ExperimentalContent.ActualWidth * 0.5;
                double effectiveOffsetX;

                if (newOffsetX < -maxDrag)
                {
                    effectiveOffsetX = -maxDrag + (newOffsetX + maxDrag) * resistance;
                }
                else if (newOffsetX > maxDrag)
                {
                    effectiveOffsetX = maxDrag + (newOffsetX - maxDrag) * resistance;
                }
                else
                {
                    effectiveOffsetX = newOffsetX;
                }

                var transform = new TranslateTransform(effectiveOffsetX, 0);
                ExperimentalContent.RenderTransform = transform;
            }
        }

        private void ExperimentalContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            Mouse.Capture(null); // Release mouse capture
            Cursor = Cursors.Arrow; // Reset cursor to default
            double offsetX = ((TranslateTransform)ExperimentalContent.RenderTransform).X;

            if (offsetX > swipeThreshold) // Swipe right to Verified
            {
                AnimateTransition(ExperimentalContent, VerifiedContent, VerifiedButton);
            }
            else if (offsetX < -swipeThreshold) // Swipe left to Resources
            {
                AnimateTransition(ExperimentalContent, ResourcesContent, ResourcesButton);
            }
            else
            {
                AnimateBackToOriginalPosition(ExperimentalContent);
            }
        }

        private void ResourcesContent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                isDragging = true;
                startPoint = e.GetPosition(null);
                var transform = element.RenderTransform as TranslateTransform;
                initialOffsetX = transform?.X ?? 0;
                Mouse.Capture(element);
                Cursor = Cursors.Hand; // Change cursor to hand
            }
        }

        private void ResourcesContent_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Windows.Point currentPosition = e.GetPosition(null);
                Vector offset = currentPosition - startPoint;

                double newOffsetX = initialOffsetX + offset.X;

                double resistance = 0.5; // Adjust for more or less resistance
                double maxDrag = ResourcesContent.ActualWidth * 0.5;
                double effectiveOffsetX;

                if (newOffsetX < -maxDrag)
                {
                    effectiveOffsetX = -maxDrag + (newOffsetX + maxDrag) * resistance;
                }
                else if (newOffsetX > maxDrag)
                {
                    effectiveOffsetX = maxDrag + (newOffsetX - maxDrag) * resistance;
                }
                else
                {
                    effectiveOffsetX = newOffsetX;
                }

                var transform = new TranslateTransform(effectiveOffsetX, 0);
                ResourcesContent.RenderTransform = transform;
            }
        }

        private void ResourcesContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            Mouse.Capture(null); // Release mouse capture
            Cursor = Cursors.Arrow; // Reset cursor to default
            double offsetX = ((TranslateTransform)ResourcesContent.RenderTransform).X;

            if (offsetX > swipeThreshold) // Swipe right to Experimental
            {
                AnimateTransition(ResourcesContent, ExperimentalContent, ExperimentalButton); // Pass the active button
            }
            else
            {
                AnimateBackToOriginalPosition(ResourcesContent);
            }
        }

        private void AnimateBackToOriginalPosition(StackPanel content)
        {
            var storyboard = new Storyboard();

            // Create a translate animation back to the original position
            DoubleAnimation translateAnimation = new DoubleAnimation
            {
                From = ((TranslateTransform)content.RenderTransform).X,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(translateAnimation, content);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));

            // Reset opacity to ensure it's visible during the animation
            content.Opacity = 1;

            storyboard.Children.Add(translateAnimation);
            storyboard.Completed += (s, e) =>
            {
                // Ensure final state after the animation completes
                content.RenderTransform = new TranslateTransform(0, 0); // Reset transform
            };

            storyboard.Begin();
        }

        private void ResetContentPosition(StackPanel content)
        {
            // Reset transform and ensure visibility
            content.RenderTransform = new TranslateTransform(0, 0); // Reset position
            content.Opacity = 1; // Ensure it's fully visible
            content.Visibility = Visibility.Visible; // Ensure it's visible

            ResetContentVisibility(content); // Reset child element visibility
        }

        private void ResetContentVisibility(StackPanel content)
        {
            // Reset Opacity and Visibility for all TextBlocks in the specified content
            foreach (var child in content.Children)
            {
                if (child is TextBlock textBlock)
                {
                    textBlock.Opacity = 1; // Ensure text block is visible
                    textBlock.Visibility = Visibility.Visible; // Ensure text block is visible
                }
            }
        }

        // Helper method to set the active button
        private void SetActiveButton(Button activeButton)
        {
            VerifiedButton.Tag = activeButton == VerifiedButton ? "Active" : "Inactive";
            ExperimentalButton.Tag = activeButton == ExperimentalButton ? "Active" : "Inactive";
            ResourcesButton.Tag = activeButton == ResourcesButton ? "Active" : "Inactive";
        }





        private void ShowContent(StackPanel content, bool show)
        {
            content.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SwitchContent(StackPanel newContent, Button activeButton)
        {
            if (VerifiedContent.Visibility == Visibility.Visible)
                AnimateTransition(VerifiedContent, newContent, activeButton);
            else if (ExperimentalContent.Visibility == Visibility.Visible)
                AnimateTransition(ExperimentalContent, newContent, activeButton);
            else if (ResourcesContent.Visibility == Visibility.Visible)
                AnimateTransition(ResourcesContent, newContent, activeButton);
        }

        private void SwitchContentImmediately(StackPanel newContent)
        {
            // Reset all contents to be invisible
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Collapsed;
            ResourcesContent.Visibility = Visibility.Collapsed;

            // Set the new content to visible
            newContent.Visibility = Visibility.Visible;

            // Reset transformations
            ResetContentTransforms();
        }

        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            SwitchContentImmediately(VerifiedContent);
            SetActiveButton(VerifiedButton);
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            SwitchContentImmediately(ExperimentalContent);
            SetActiveButton(ExperimentalButton);
        }

        private void SwitchToResourcesTab(object sender, RoutedEventArgs e)
        {
            SwitchContentImmediately(ResourcesContent);
            SetActiveButton(ResourcesButton);
        }


        private void ResetContentTransforms()
        {
            foreach (var content in new[] { VerifiedContent, ExperimentalContent, ResourcesContent })
            {
                content.RenderTransform = new TranslateTransform(0, 0); // Reset position
                content.Opacity = 1; // Ensure it’s fully visible
            }
        }















        private void Win32PrioritySeparation_Click(object sender, RoutedEventArgs e)
        {
            var win32Control = new Views.Win32PrioritySeparation(this);
            ShowContentWithAnimation(win32Control); // Call the method without animation
            
        }

        private void Storage_Click(object sender, RoutedEventArgs e)
        {
            var storageControl = new Views.Storage(this);
            ShowContentWithAnimation(storageControl); // Call the method without animation

        }

        private void Windows_Click(object sender, RoutedEventArgs e)
        {
            var windowsControl = new WindowsUserControl(this);
            ShowContentWithAnimation(windowsControl); // Call the method to display with animation
        }

        private void Debloat_Click(object sender, RoutedEventArgs e)
        {
            var debloatControl = new Debloat(this);
            ShowContentWithAnimation(debloatControl); // Call the method to display with animation
        }

        private void Supercache_Click(object sender, RoutedEventArgs e)
        {
            var superCacheControl = new SuperCacheUserControl(this);
            ShowContentWithAnimation(superCacheControl); // Call the method to display with animation
        }


        private void PowerPlan_Click(object sender, RoutedEventArgs e)
        {
            var powerPlanControl = new PowerPlan(this);
            ShowContentWithAnimation(powerPlanControl); // Call the method to display with animation
        }

        private void Security_Click(object sender, RoutedEventArgs e)
        {
            var securityControl = new SecurityUserControl(this);
            ShowContentWithAnimation(securityControl); // Call the method to display with animation
        }

        private void MemoryMan_Click(object sender, RoutedEventArgs e)
        {
            var memmanControl = new MemoryMan(this);
            ShowContentWithAnimation(memmanControl); // Call the method to display with animation
        }

        private void MMCSS_Click(object sender, RoutedEventArgs e)
        {
            var mmcssControl = new MMCSS(this);
            ShowContentWithAnimation(mmcssControl); // Call the method to display with animation
        }

        private void Nvidia_Click(object sender, RoutedEventArgs e)
        {
            var nvidiaControl = new NVIDIAUserControl(this);
            ShowContentWithAnimation(nvidiaControl); // Call the method to display with animation
        }

        private void Monitor_Click(object sender, RoutedEventArgs e)
        {
            var monitorControl = new MonitorUserControl(this);
            ShowContentWithAnimation(monitorControl); // Call the method to display with animation
        }

        private void AMD_Click(object sender, RoutedEventArgs e)
        {
            var amdControl = new AMD(this);
            ShowContentWithAnimation(amdControl); // Call the method to display with animation
        }

        private void CPUPriority_Click(object sender, RoutedEventArgs e)
        {
            var cpuPriorityControl = new CPUPriorityControl(this);
            ShowContentWithAnimation(cpuPriorityControl); // Call the method to display with animation
        }

        private void MSI_Click(object sender, RoutedEventArgs e)
        {
            var msiControl = new MSI(this);
            ShowContentWithAnimation(msiControl); // Call the method to display with animation
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsControl = new SettingsUserControl(this); // Use the correct class name
            ShowContentWithAnimation(settingsControl); // Call the method to display with animation
        }

        private void Changelog_Click(object sender, RoutedEventArgs e)
        {
            var changeLog = new ChangelogUserControl(this); // Use the correct class name
            ShowContentWithAnimation(changeLog); // Call the method to display with animation
        }



        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Show the welcome page
            var welcomeControl = new WelcomeUserControl(this); // Create a new instance of the Welcome UserControl
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



        private Stack<UserControl> navigationStack = new Stack<UserControl>();


        private void ShowContentWithAnimation(UserControl newContent)
        {
            // Hide the main window content
            Main.Visibility = Visibility.Collapsed;

            // Optionally hide other content areas if needed
            ContentOther.Visibility = Visibility.Collapsed;
            ContentArea.Visibility = Visibility.Visible;

            // Set the new content
            ContentArea.Content = newContent;

            // Make the new content fully visible immediately
            newContent.Opacity = 1; // Set to fully visible
        }



        private void GoBack()
        {
            if (navigationStack.Count > 0)
            {
                UserControl previousContent = navigationStack.Pop(); // Get the last user control
                ShowContentWithAnimation(previousContent); // Show it with animation
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }


        private void ShowHomePage()
        {
            // Hide any user controls
            ContentArea.Content = null; // Clear previous content

            // Show the main window content
            Main.Visibility = Visibility.Visible;

            TitleTextBlock.Content = "Home"; // Set the title when showing the home page
        }


        private void GoToHomePageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHomePage();
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

        private bool isResizing = false;
        private System.Windows.Point lastMousePosition; // Specify the namespace
        private const int ResizeBorderThickness = 5; // Define thickness for resizing

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // Use LeftButton instead of ButtonState
            {
                lastMousePosition = e.GetPosition(this);
                isResizing = true;
                Mouse.Capture((UIElement)sender); // Capture mouse to the border
            }
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                System.Windows.Point currentMousePosition = e.GetPosition(this); // Specify the namespace
                double widthChange = currentMousePosition.X - lastMousePosition.X;
                double heightChange = currentMousePosition.Y - lastMousePosition.Y;

                this.Width = Math.Max(300, this.Width + widthChange);  // Minimum width
                this.Height = Math.Max(200, this.Height + heightChange); // Minimum height

                lastMousePosition = currentMousePosition; // Update last mouse position
            }
            else
            {
                // Change cursor style based on position
                System.Windows.Point mousePos = e.GetPosition(this); // Specify the namespace
                if (mousePos.X < ResizeBorderThickness || mousePos.X > this.ActualWidth - ResizeBorderThickness ||
                    mousePos.Y < ResizeBorderThickness || mousePos.Y > this.ActualHeight - ResizeBorderThickness)
                {
                    this.Cursor = Cursors.SizeAll; // Change cursor for resizing
                }
                else
                {
                    this.Cursor = Cursors.Arrow; // Default cursor
                }
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
            {
                isResizing = false;
                Mouse.Capture(null); // Release mouse capture
            }
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

        







    }
}









 

