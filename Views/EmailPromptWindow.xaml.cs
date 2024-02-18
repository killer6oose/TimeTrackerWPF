using System.Windows;

namespace TimeTrackerWPF.Views
{
    /// <summary>
    /// Interaction logic for EmailPromptWindow.xaml
    /// </summary>
    public partial class EmailPromptWindow : Window
    {
        public EmailPromptWindow()
        {
            InitializeComponent();
        }

        public string CorpNameLong { get; private set; }
        public string CorpNameShort { get; private set; }
        public string InternalPMEmail { get; private set; }
        public string SendToEmails { get; private set; }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Indicate cancellation
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Save the inputs
            SendToEmails = txtSendToEmails.Text;
            InternalPMEmail = txtInternalPMEmail.Text;
            CorpNameShort = txtCorpNameShort.Text;
            CorpNameLong = txtCorpNameLong.Text;

            this.DialogResult = true; // Indicate success
        }
    }
}