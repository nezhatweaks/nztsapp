using System.Windows;

namespace NZTS_App
{
    public partial class BenchmarkResults : Window
    {
        public BenchmarkMetrics Metrics { get; set; }

        public BenchmarkResults(BenchmarkMetrics metrics)
        {
            InitializeComponent();
            this.Metrics = metrics;

            // Set the DataContext to bind to the UI
            this.DataContext = metrics;
        }

        // This method updates the CPU status text in the UI
        public void UpdateCpuStatus(string status)
        {
            CpuStatusTextBlock.Text = status;  // Assuming CpuStatusTextBlock is defined in XAML
        }
    }

}
