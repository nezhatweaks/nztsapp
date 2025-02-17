using System.Windows;
using System.Windows.Controls;

namespace NZTS_App.Views
{
    /// <summary>
    /// Interaction logic for WelcomeUserControl.xaml
    /// </summary>
    public partial class WelcomeUserControl : UserControl
    {
        public event RoutedEventHandler? OptimizeAllClicked;
        public event RoutedEventHandler? RestoreAllClicked;
        public event RoutedEventHandler? OptimizeAllButtonRightClicked;

        private MainWindow mainWindow;

        public WelcomeUserControl(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            mainWindow.TitleTextBlock.Content = "1-Click";
        }

        private void OptimizeAllButtonRight_Click(object sender, RoutedEventArgs args)
        {
            // Raise the right-click event
            OptimizeAllButtonRightClicked?.Invoke(this, args);

            // Check if the ContextMenu is assigned before trying to open it
            if (OptimizeAllButton.ContextMenu != null)
            {
                OptimizeAllButton.ContextMenu.IsOpen = true; // Open the context menu
            }
            else
            {

            }
        }


        private void OptimizeAll_Click(object sender, RoutedEventArgs e)
        {
            OptimizeAllClicked?.Invoke(this, e); // Raise the OptimizeAllClicked event
        }

        private void RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            RestoreAllClicked?.Invoke(this, e); // Raise the RestoreAllClicked event
        }
    }
}
