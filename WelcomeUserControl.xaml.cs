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
        private MainWindow mainWindow;
        public WelcomeUserControl(MainWindow window)
        {
            InitializeComponent();
            
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "Home";

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
