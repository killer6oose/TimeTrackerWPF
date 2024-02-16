using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TimeTrackerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer hourlyTimer;
        private readonly string ProjectsFolderPath;
        private readonly DispatcherTimer timer;
        private string pendingNotes = string.Empty;
        private string SelectedFilePath;
        private DateTime StartTime;
        // Variable to store pending notes

        public MainWindow()
        {
            InitializeComponent();

            // Set the Text property of the RichTextBox to empty string and ReadOnly
            Notes.Document.Blocks.Clear();
            Notes.IsReadOnly = true;

            ProjectsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".TrackYourTasks");
            if (!Directory.Exists(ProjectsFolderPath))
            {
                Directory.CreateDirectory(ProjectsFolderPath);
            }

            LoadProjectFiles();

            hourlyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(1)
            };
            hourlyTimer.Tick += HourlyTimer_Tick;
            hourlyTimer.Start();

            // Set the default text of the button to "START"
            StartStopBtn.Name = "START";
            // Disable the start button by default
            StartStopBtn.IsEnabled = false;
            // Initialize the timer
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // 1 second
            };
            timer.Tick += Timer_Tick;
            IsTimerRunning = false;
        }

        public bool IsTimerRunning { get; private set; }

        private void CreateNewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the project name from the TextBox
            string projectName = NewProjectNameTextBox.Text;

            // Proceed with creating the project file and updating the UI
            if (!string.IsNullOrWhiteSpace(projectName))
            {
                string newProjectFileName = Path.Combine(ProjectsFolderPath, $"{projectName}_{DateTime.Now:yyyyMMdd}.txt");

                // Create the new project file
                File.WriteAllText(newProjectFileName, "");

                // Add the new project file to the dropdown
                ProjectList.Items.Add(Path.GetFileName(newProjectFileName));

                // Select the new project file in the dropdown
                ProjectList.SelectedItem = Path.GetFileName(newProjectFileName);

                // Close the NewProjectPopup
                NewProjectPopup.IsOpen = false;
            }
            else
            {
                MessageBox.Show("Please enter a valid project name.");
            }
        }

        private void HourlyTimer_Tick(object sender, EventArgs e)
        {
        }

        private void LoadProjectFiles()
        {
            ProjectList.Items.Clear();
            string[] projectFiles = Directory.GetFiles(ProjectsFolderPath, "*.txt");
            foreach (string projectFile in projectFiles)
            {
                _ = ProjectList.Items.Add(Path.GetFileName(projectFile));
            }
        }

        private void NewProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open the NewProjectPopup when NewProjectBtn is clicked
            NewProjectPopup.IsOpen = true;
        }

        private void NotesOnLostFocus(object sender, RoutedEventArgs e)
        {
            // Ensure that the Notes field is not empty
            if (Notes.Document.Blocks.Count > 0)
            {
                // Get the text from the Notes field
                TextRange textRange = new TextRange(Notes.Document.ContentStart, Notes.Document.ContentEnd);
                pendingNotes = textRange.Text.Trim(); // Store the pending notes
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            QuickActionWindow quickActionWindow = new TimeTrackerWPF.QuickActionWindow();
            quickActionWindow.ShowDialog();
        }

        private void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectList.SelectedItem != null)
            {
                // Enable the "Start Timer" button when a project is selected
                StartStopBtn.IsEnabled = true;

                // Clear the Notes field
                Notes.Document.Blocks.Clear();

                // Load selected project file into text editor
                string selectedProjectFile = Path.Combine(ProjectsFolderPath, ProjectList.SelectedItem.ToString());

                if (File.Exists(selectedProjectFile))
                {
                    // Set the selected file path
                    SelectedFilePath = selectedProjectFile;
                }
            }
            else
            {
                // Disable the "Start Timer" button if no project is selected
                StartStopBtn.IsEnabled = false;

                // Clear text box
                Notes.Document.Blocks.Clear();
            }
        }

        // Close the NewProjectWindow NewProjectNameBox.Close(); }
        private void SaveNotesToFile()
        {
            // Check if a project is selected
            if (!string.IsNullOrEmpty(SelectedFilePath))
            {
                // Output the notes to the selected file
                using (StreamWriter writer = new StreamWriter(SelectedFilePath, true))
                {
                    // Add notes to the file
                    TextRange textRange = new TextRange(Notes.Document.ContentStart, Notes.Document.ContentEnd);
                    if (!string.IsNullOrWhiteSpace(textRange.Text))
                    {
                        writer.WriteLine($"Notes: {textRange.Text}");
                    }
                }
            }
        }

        private void StartStopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedFilePath))
            {
                MessageBox.Show("Please select a file or create a new project first.");
                return;
            }

            if (IsTimerRunning)
            {
                // End the timer
                timer.Stop();
                IsTimerRunning = false;

                // Set the Notes RichTextBox back to read-only
                Notes.IsReadOnly = true;

                // Calculate the elapsed time
                TimeSpan elapsedTime = DateTime.Now - StartTime;

                // Calculate the total minutes rounded to the nearest quarter hour
                int totalMinutes = (int)Math.Round(elapsedTime.TotalMinutes / 15.0) * 15;

                // Convert total minutes to hours and minutes
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;

                // Output the elapsed time to the selected file
                using (StreamWriter writer = new StreamWriter(SelectedFilePath, true))
                {
                    writer.WriteLine($"Start Time: {StartTime}");
                    writer.WriteLine($"End Time: {DateTime.Now}");
                    writer.WriteLine($"Elapsed Time: {hours} hours and {minutes} minutes");

                    if (!string.IsNullOrWhiteSpace(pendingNotes))
                    {
                        // Save pending notes to file
                        writer.WriteLine($"Notes: {pendingNotes}");
                    }

                    writer.WriteLine(new string('_', 50)); // Separator line
                }

                // Clear the Notes field after appending notes to the file
                Notes.Document.Blocks.Clear();

                // Reset pending notes
                pendingNotes = string.Empty;

                // Save notes to file
                SaveNotesToFile();
            }
            else
            {
                // Start the timer
                StartTime = DateTime.Now;
                timer.Start();
                IsTimerRunning = true;

                // Set the Notes RichTextBox to editable
                Notes.IsReadOnly = false;
            }

            // Update the button text
            UpdateButtonText();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Hour == 12)
            {
                this.Topmost = true;
                this.Topmost = false;
            }
        }

        private void UpdateButtonText()
        {
            // Update the button text based on the timer state
            if (IsTimerRunning)
            {
                StartStopBtn.Content = "STOP";
            }
            else
            {
                StartStopBtn.Content = "START";
            }
        }
    }
}