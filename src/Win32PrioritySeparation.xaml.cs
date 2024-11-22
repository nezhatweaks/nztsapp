using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    public partial class Win32PrioritySeparation : UserControl
    {
        private MainWindow mainWindow; // Field to store reference to MainWindow

        // Constructor that accepts a MainWindow reference
        public Win32PrioritySeparation(MainWindow window)
        {
            InitializeComponent();
            LoadCurrentWin32PrioritySeparationValue();  // Load current value when the window loads
            mainWindow = window; // Store the reference
            mainWindow.TitleTextBlock.Content = "WinPriority";
        }

        // Load current registry value and display it
        private void LoadCurrentWin32PrioritySeparationValue()
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl");
                if (key != null)
                {
                    int currentValue = (int)key.GetValue("Win32PrioritySeparation", 0x00000002); // Default to 0x00000002 if not found
                    CurrentValueTextBlock.Text = $"Current Value: 0x{currentValue:X8}";
                    key.Close();
                }
                else
                {
                    string errorMsg = "Failed to access PriorityControl registry key.";
                    MessageBox.Show(errorMsg);
                    App.changelogUserControl?.AddLog("Failed", errorMsg);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading current value: {ex.Message}";
                MessageBox.Show(errorMsg);
                App.changelogUserControl?.AddLog("Failed", errorMsg);
            }
        }

        // Event handler for when any preset button is clicked
        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button presetButton)
            {
                // Ensure Tag is not null before accessing it
                if (presetButton.Tag is string tagValue && !string.IsNullOrEmpty(tagValue))
                {
                    int selectedValue;
                    // Try to parse the tagValue as a hex number
                    if (int.TryParse(tagValue, System.Globalization.NumberStyles.HexNumber, null, out selectedValue))
                    {
                        ApplyWin32PrioritySeparationValue(selectedValue);
                    }
                    else
                    {
                        MessageBox.Show("Invalid preset value.");
                    }
                }
                else
                {
                    MessageBox.Show("Preset button has no value assigned.");
                }
            }
        }



        // Apply the value to the registry
        private void ApplyWin32PrioritySeparationValue(int value)
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true);
                if (key != null)
                {
                    key.SetValue("Win32PrioritySeparation", value, RegistryValueKind.DWord);
                    key.Close();
                    string successMsg = $"Win32PrioritySeparation value set to 0x{value:X8}";
                    MessageBox.Show(successMsg);
                    App.changelogUserControl?.AddLog("Applied", successMsg); // Log success
                    LoadCurrentWin32PrioritySeparationValue();  // Update the displayed current value
                    mainWindow?.MarkSettingsApplied(); // Use the stored MainWindow reference
                }
                else
                {
                    string errorMsg = "Failed to access PriorityControl registry key.";
                    MessageBox.Show(errorMsg);
                    App.changelogUserControl?.AddLog("Failed", errorMsg); // Log failure
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error applying value: {ex.Message}";
                MessageBox.Show(errorMsg);
                App.changelogUserControl?.AddLog("Failed", errorMsg); // Log failure
            }
        }

        // Restore to default value (0x00000002)
        private void RestoreWin32PrioritySeparation_Click(object sender, RoutedEventArgs e)
        {
            ApplyWin32PrioritySeparationValue(0x00000002);  // Default value
        }

        // Switch to Verified tab
        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            // Change tab button state
            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";

            // Show Verified content and hide Experimental content
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;
        }

        // Switch to Experimental tab
        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            // Change tab button state
            VerifiedButton.Tag = "Inactive";
            ExperimentalButton.Tag = "Active";

            // Show Experimental content and hide Verified content
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;
        }
    }
}
