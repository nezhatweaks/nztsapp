using System;
using System.Windows;
using System.Windows.Controls;

namespace NZTS_App
{
    /// <summary>
    /// Interaction logic for WelcomeUserControl.xaml
    /// </summary>
    public partial class WelcomeUserControl : UserControl
    {
        public event RoutedEventHandler? OptimizeAllClicked; // Nullable event
        public event RoutedEventHandler? RestoreAllClicked;  // Nullable event

        public WelcomeUserControl()
        {
            InitializeComponent();
            DisplayCurrentVersion(); // Display the current version when the control is initialized
        }

        private void DisplayCurrentVersion()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var currentVersion = assembly?.GetName().Version?.ToString() ?? "unknown version";
            VersionTextBlock.Text = $"v{currentVersion}"; // Set the version text
        }

        private void OptimizeAll_Click(object sender, RoutedEventArgs e)
        {
            OptimizeAllClicked?.Invoke(this, e);
        }

        private void RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            RestoreAllClicked?.Invoke(this, e);
        }
    }
}
