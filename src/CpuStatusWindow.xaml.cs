using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NZTS_App
{
    public partial class CpuStatusWindow : Window
    {
        public CpuStatusWindow()
        {
            InitializeComponent();
        }

        // Method to update the displayed status
        public void UpdateCpuStatus(string status)
        {
            // Ensure we're updating the UI thread
            Dispatcher.Invoke(() =>
            {
                StatusTextBlock.Text = status;  // Assuming StatusTextBlock is a TextBlock in the XAML
            });
        }
    }
}

