using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    /// <summary>
    /// Interaction logic for LatencySensitivityHint.xaml
    /// </summary>
    public partial class LatencySensitivityHint : UserControl
    {
        private MainWindow mainWindow; // Field to store reference to MainWindow

        // Constructor that accepts MainWindow reference
        public LatencySensitivityHint(MainWindow window)
        {
            InitializeComponent();  // This method initializes the XAML components
            mainWindow = window; // Store the reference
        }

        // Event handler for applying the registry tweak
        private void Optimize3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true);
                if (key != null)
                {
                    key.SetValue("LatencySensitivityHint", 1, RegistryValueKind.DWord);
                    key.Close();
                    MessageBox.Show("LatencySensitivityHint optimization applied!");
                    App.changelogUserControl?.AddLog("Applied", "Changed the LatencySensitivityHint setting.");

                    // Mark settings as applied
                    mainWindow?.MarkSettingsApplied(); // Use the stored MainWindow reference
                }
                else
                {
                    MessageBox.Show("Failed to open registry key for LatencySensitivityHint.");
                    App.changelogUserControl?.AddLog("Failed", "Unable to change the LatencySensitivityHint setting.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void DeleteLatencySensitivityHint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", true);
                if (key != null)
                {
                    key.DeleteValue("LatencySensitivityHint", false); // false = do not throw an exception if the value does not exist
                    key.Close();
                    MessageBox.Show("LatencySensitivityHint has been deleted!");
                    App.changelogUserControl?.AddLog("Deleted", "Deleted the LatencySensitivityHint setting.");
                }
                else
                {
                    MessageBox.Show("Failed to open registry key for LatencySensitivityHint.");
                    App.changelogUserControl?.AddLog("Failed", "Unable to delete the LatencySensitivityHint setting.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", "Unable to delete the LatencySensitivityHint setting.");
            }
        }
    }
}
