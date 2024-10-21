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
            mainWindow.TitleTextBlock.Content = "Win32 Priority";
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

        // Event handler for when the preset selection changes
        private void PresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // You can add logic here if needed when the user changes the selection
            // For example, you might want to update the current value display or enable/disable buttons
        }

        // Apply the selected preset value
        private void ApplyPreset_Click(object sender, RoutedEventArgs e)
        {
            if (Win32PrioritySeparationComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int selectedValue = Convert.ToInt32(selectedItem.Tag.ToString(), 16);  // Convert hex string to int
                ApplyWin32PrioritySeparationValue(selectedValue);
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
    }
}
