using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NZTS_App.Views
{
    public partial class CpuAffinityDialog : Window
    {
        // Property to set available CPU cores
        public List<string> CpuCores { get; set; } = new List<string>(); // Initialized as an empty list

        public List<int> SelectedCores { get; private set; }

        public CpuAffinityDialog()
        {
            InitializeComponent();
            SelectedCores = new List<int>();
            // Position the window at specific coordinates (optional)
            this.Left = 500;  // X-coordinate
            this.Top = 300;   // Y-coordinate
        }

        // Populate the list of CPU cores
        
        public void InitializeDialog()
        {
            CpuCoreListBox.Items.Clear();
            for (int i = 0; i < CpuCores.Count; i++)
            {
                var cpuCoreItem = new CheckBox
                {
                    Content = CpuCores[i],
                    Tag = i,
                    IsChecked = false, // By default, no CPU core selected
                    Foreground = new SolidColorBrush(Colors.White) // Set the text color to white
                };
                cpuCoreItem.Checked += CpuCore_Checked;
                cpuCoreItem.Unchecked += CpuCore_Unchecked;
                CpuCoreListBox.Items.Add(cpuCoreItem);
            }
        }


        // Method to pre-select the cores based on the provided list
        public void PreSelectCores(List<int> selectedCores)
        {
            foreach (var coreIndex in selectedCores)
            {
                foreach (CheckBox checkBox in CpuCoreListBox.Items)
                {
                    if ((int)checkBox.Tag == coreIndex)
                    {
                        checkBox.IsChecked = true;
                        break;
                    }
                }
            }
        }

        // Handle when a CPU core checkbox is checked
        private void CpuCore_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag is int coreIndex)
            {
                // Add the core index to the selected list
                if (!SelectedCores.Contains(coreIndex))
                {
                    SelectedCores.Add(coreIndex);
                }
            }
        }

        // Handle when a CPU core checkbox is unchecked
        private void CpuCore_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag is int coreIndex)
            {
                // Remove the core index from the selected list
                SelectedCores.Remove(coreIndex);
            }
        }

        // OK button click
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult set to true if OK is clicked
            this.DialogResult = true;
            this.Close();
        }

        // Reset button click (untick all checkboxes)
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Untick all checkboxes
            foreach (CheckBox checkBox in CpuCoreListBox.Items)
            {
                checkBox.IsChecked = false;
            }

            // Clear the selected cores list
            SelectedCores.Clear();
        }
    }
}
