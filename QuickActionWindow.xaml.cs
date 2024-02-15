using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeTrackerWPF
{
    /// <summary>
    /// Interaction logic for QuickActionWindow.xaml
    /// </summary>
    public partial class QuickActionWindow : Window
    {
        public QuickActionWindow()
        {
            InitializeComponent();
        }

        private void CreateRecordOnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NewEntryPopupOnClick(object sender, RoutedEventArgs e)
        {
            // Creates popup for net-new entries
            NewEntryPopup.IsOpen = true;

            // After new entry is added, update the generated buttons
            UpdateDynamicButtons();
        }

        private void UpdateDynamicButtons()
        {
        }
    }
}