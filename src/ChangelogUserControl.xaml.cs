using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App
{
    public partial class ChangelogUserControl : UserControl
    {
        // ObservableCollection to hold log entries
        public ObservableCollection<ChangelogEntry> ChangelogEntries { get; set; } = new ObservableCollection<ChangelogEntry>();
        private MainWindow mainWindow;
        public ChangelogUserControl(MainWindow window)
        {
            InitializeComponent();
            // Use the static collection from App
            ChangeLogListView.ItemsSource = App.ChangelogEntries;
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Logs";
        }

        public void AddLog(string action, string description)
        {
            var entry = new ChangelogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Description = description
            };

            App.ChangelogEntries.Add(entry);
            App.SaveLogs();



        }
        public void ClearLogs()
        {
            App.ChangelogEntries.Clear();
            App.SaveLogs(); // Save the cleared state
        }

        public void ExportLogs(string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Timestamp,Action,Description");
                foreach (var entry in App.ChangelogEntries)
                {
                    writer.WriteLine($"{entry.Timestamp},{entry.Action},{entry.Description}");
                }
            }
        }
        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            // Prompt the user for confirmation
            var result = MessageBox.Show(
                "Are you sure you want to clear the log? This process cannot be undone.",
                "Confirm Clear Log",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ClearLogs(); // Clear logs and save state
            }
        }


        private void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "changelog.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExportLogs(saveFileDialog.FileName);
            }
        }



        // Class to represent each changelog entry
        public class ChangelogEntry
        {
            public DateTime Timestamp { get; set; }
            public string? Action { get; set; }
            public string? Description { get; set; }
        }


    }

}
