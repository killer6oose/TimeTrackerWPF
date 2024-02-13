using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TimeTrackerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TimeZoneInfo timeZoneInfo;

        private readonly string ProjectsFolderPath;
        private readonly DispatcherTimer hourlyTimer;
        private readonly DispatcherTimer timer;
        private readonly DateTime StartTime;
        private readonly bool isTimerRunning;
        private readonly string selectedFilePath;

        public TimeZoneInfo TimeZoneInfo { get; private set; }
        public bool IsTimerRunning { get; }

        public MainWindow()
        {
            {
                InitializeComponent();

                ProjectsFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".TrackYourTasks");
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

                // Populate the ComboBox with system time zones
                foreach (TimeZoneInfo timezone in TimeZoneInfo.GetSystemTimeZones())
                {
                    Timezones.Items.Add(timezone.DisplayName);
                }

                // Set default timezone to US Eastern Standard Time
                TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                // Set the default text of the button to "START TIMER"
                StartStopBtn.Text = "START TIMER";
                // Disable the start button by default
                StartStopBtn.Enabled = false;
                // Initialize the timer
                timer = new Timer
                {
                    Interval = 1000 // 1 second
                };
                timer.Tick += Timer_Tick;
                IsTimerRunning = false;
            }
            void HourlyTimer_Tick(object sender, EventArgs e)
            {
                // Your code for the HourlyTimer_Tick event handler goes here
            }

            void LoadProjectFiles()
            {
                ProjectList.Items.Clear();
                string[] projectFiles = Directory.GetFiles(ProjectsFolderPath, "*.txt");
                foreach (string projectFile in projectFiles)
                {
                    ProjectList.Items.Add(System.IO.Path.GetFileName(projectFile));
                }
            }

            void Timer_Tick(object sender, EventArgs e)
            {
                if (DateTime.Now.Hour == 12)
                {
                    this.Topmost = true;
                    this.Topmost = false;
                }
            }

            void StartStopBtn_Click(object sender, RoutedEventArgs e)
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

                        // Append notes from the Notes field
                        if (!string.IsNullOrWhiteSpace(Notes.Text))
                        {
                            writer.WriteLine($"Notes: {Notes.Text}");
                        }

                        writer.WriteLine(new string('_', 50)); // Separator line
                    }

                    // Clear the Notes field after appending notes to the file
                    Notes.Clear();
                }
                else
                {
                    // Start the timer
                    StartTime = DateTime.Now;
                    timer.Start();
                    IsTimerRunning = true;
                }

                // Update the button text
                UpdateButtonText();
            }

            void UpdateButtonText()
            {
                // Update the button text based on the timer state
                if (isTimerRunning)
                {
                    StartStopBtn.Text = "End Timer";
                }
                else
                {
                    StartStopBtn.Text = "Start Timer";
                }
            }
            void Notes_TextChanged(object sender, TextChangedEventArgs e)
            {
            }

            void MenuItem_Click(object sender, RoutedEventArgs e)
            {
            }

            void NewProjectBtn_Click(object sender, RoutedEventArgs e)
            {
                // Prompt the user for the project name
                string projectName = Microsoft.VisualBasic.Interaction.InputBox("Enter the project name:", "New Project", "");

                if (!string.IsNullOrWhiteSpace(projectName))
                {
                    // Generate a new project file name
                    string newProjectFileName = Path.Combine(ProjectsFolderPath, $"{projectName}_{DateTime.Now:yyyyMMdd}.txt");

                    // Create the new project file
                    File.WriteAllText(newProjectFileName, "");

                    // Add the new project file to the dropdown
                    ProjectList.Items.Add(Path.GetFileName(newProjectFileName));

                    // Select the new project file in the dropdown
                    ProjectList.SelectedItem = Path.GetFileName(newProjectFileName);

                    // Set the selected file path
                    NewMethod(newProjectFileName);
                }
            }

            void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (ProjectList.SelectedItem != null)
                {
                    // Enable the "Start Timer" button when a project is selected
                    StartStopBtn.IsEnabled = true;

                    // Load selected project file into text editor
                    string selectedProjectFile = Path.Combine(ProjectsFolderPath, ProjectList.SelectedItem.ToString());

                    if (File.Exists(selectedProjectFile))
                    {
                        Notes.Text = File.ReadAllText(selectedProjectFile);

                        // Set the selected file path
                        SelectedFilePath = selectedProjectFile;
                    }
                    else
                    {
                        Notes.TextChanged = ""; // Clear text box if file doesn't exist
                    }
                }
                else
                {
                    // Disable the "Start Timer" button if no project is selected
                    StartStopBtn.Enabled = false;

                    // Clear text box
                    Notes.TextInput = "";
                }
            }

            void Timezone_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                string selectedTimezone = Timezone.SelectedItem.ToString();
                TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(selectedTimezone);
            }
        }

        private static void NewMethod(string newProjectFileName)
        {
            SelectedFilePath = newProjectFileName;
        }
    }
}