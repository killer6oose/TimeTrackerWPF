using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TimeTrackerWPF.Views
{
    /// <summary>
    /// Interaction logic for QuickActionWindow.xaml
    /// </summary>
    public partial class QuickActionWindow : Window
    {
        private readonly string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".TrackYourTasks", "data.json");
        private readonly string projectsFolderPath;
        private List<Entry> entries;

        public QuickActionWindow(string projectsFolderPath)
        {
            InitializeComponent();
            EnsureJsonFileExists();
            LoadJsonData();
            GenerateButtonsFromJSON();
            Closing += QuickActionWindow_Closing;
            this.projectsFolderPath = projectsFolderPath;
        }

        private void BeforePrjEmailOnClick(object sender, RoutedEventArgs e)
        {
            {
                EmailPromptWindow promptWindow = new EmailPromptWindow();
                if (promptWindow.ShowDialog() == true)
                {
                    // Retrieve the input values from EmailPromptWindow
                    string sendToEmails = promptWindow.SendToEmails;
                    string internalPMEmail = promptWindow.InternalPMEmail;
                    string corpNameShort = promptWindow.CorpNameShort;
                    string corpNameLong = promptWindow.CorpNameLong;

                    // Construct the mailto link with the user input
                    string subject = Uri.EscapeDataString($"{corpNameShort} / Info Before Engagement Begins");
                    string body = Uri.EscapeDataString($"Hello {corpNameLong} team!\n" +
                        "I look forward to starting work with you all here soon!\n" +
                        "Before we begin, I do need to get some information from you:\n\n" +
                        "• Access\n" +
                        "    If I am to access your servers directly, please provide method\n" +
                        "    RDP, Screenconnect, Bomgar, etc\n" +
                        "• Username\n" +
                        "• Password\n" +
                        "• 2FA(?)\n" +
                        "    If I am to work Over-The-Shoulder with one of you, please let me know if there’s a specific meeting client you prefer/have better luck with so that I may make sure I have the client installed and up to date.\n" +
                        "    I’ve had some people have issues screen sharing/giving control through teams\n" +
                        "• Server List/topography\n" +
                        "    List of ISM App servers\n" +
                        "    List of ISM SQL servers\n" +
                        "    Any load balancers we should be aware of?\n" +
                        "    Example topography (optional, but helpful)");

                    string mailtoLink = $"mailto:{sendToEmails}?cc={internalPMEmail}&subject={subject}&body={body}";

                    try
                    {
                        // Open the default mail client with the constructed mailto link
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailtoLink) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open email client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the link/location from the popup text field
            string link = (sender as Button).Tag.ToString();

            // Navigate to the link/location
            System.Diagnostics.Process.Start(link);
        }

        private void CreateRecordOnClick(object sender, RoutedEventArgs e)
        {
            // Create a new entry based on the input fields
            Entry newEntry = new Entry
            {
                Link = LinkTextBox.Text,
                FriendlyName = FriendlyNameTextBox.Text
            };

            // Add the new entry to the list of entries
            entries.Add(newEntry);

            // Serialize the list of entries to JSON
            string json = JsonConvert.SerializeObject(entries);

            // Write the JSON to the file
            File.WriteAllText(jsonFilePath, json);

            // Clear existing buttons
            ButtonsPanel.Children.Clear();

            // Generate buttons from updated JSON data
            GenerateButtonsFromJSON();

            // Close the popup window
            NewEntryPopup.IsOpen = false;
        }

        private void EnsureJsonFileExists()
        {
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(jsonFilePath);
            Directory.CreateDirectory(directory);

            // Create JSON file if it doesn't exist
            if (!File.Exists(jsonFilePath))
            {
                File.WriteAllText(jsonFilePath, "[]");
            }
        }

        private void FriendlyNameOnTextChange(object sender, TextChangedEventArgs e)
        {
        }

        private void GenerateButtonsFromJSON()
        {
            // Generate buttons
            foreach (var entry in entries)
            {
                // Create action button
                Button actionButton = new Button
                {
                    Content = entry.FriendlyName,
                    Width = 150, // Adjust width as needed
                    Tag = entry.Link // Store link/location as tag for later use
                };

                // Create delete button ('x')
                Button deleteButton = new Button
                {
                    Content = "x",
                    Width = 20, // Set fixed width
                    Tag = actionButton // Store reference to associated action button
                };
                deleteButton.Click += DeleteButton_Click; // Attach event handler

                // Create panel to hold both buttons
                StackPanel buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };
                buttonPanel.Children.Add(actionButton);
                buttonPanel.Children.Add(deleteButton);

                ButtonsPanel.Children.Add(buttonPanel); // Add button panel to parent panel
                ButtonsPanel.Children.Add(new Separator { Height = 10 });
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the reference to the associated action button
            Button associatedButton = (Button)((Button)sender).Tag;

            // Remove the associated dynamically created button and its delete button from the panel
            ButtonsPanel.Children.Remove(((StackPanel)associatedButton.Parent).Parent as UIElement); // Remove the button panel
        }


        private void LoadJsonData()
        {
            // Read JSON data from file
            string jsonText = File.ReadAllText(jsonFilePath);
            entries = JsonConvert.DeserializeObject<List<Entry>>(jsonText);
            if (entries == null)
            {
                entries = new List<Entry>();
            }
        }

        private void NewEntryPopupOnClick(object sender, RoutedEventArgs e)
        {
            // Creates popup for net-new entries
            NewEntryPopup.IsOpen = true;

            // After new entry is added, update the generated buttons
            UpdateDynamicButtons();
        }

        private string PromptForInput(string v)
        {
            throw new NotImplementedException();
        }

        private void QAAboutLinkOnClick(object sender, RoutedEventArgs e)
        {
            string message = "Author: Andrew Hatton d.b.a. CronoTech Consulting\n" +
                             "Version: 1.3.5\n" +
                             "Description: With its simple interface, you can easily log your tasks and keep tabs on how you spend your time. Whether you're juggling multiple projects or just want to stay organized, my time tracking tool has got you covered. It is constantly evolving with semi-regular updates, so you can expect even more features and improvements down the line.";
            string caption = "About Time Tracker";
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void QABrowseFilesOnClick(object sender, RoutedEventArgs e)
        {
            // Open File Explorer to the specified folder
            _ = Process.Start("explorer.exe", projectsFolderPath);
        }

        private void QAOpenUrlInBrowser(string url)
        {
            try
            {
                _ = Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickActionWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent the QuickActionWindow from closing when the popup is open
            e.Cancel = NewEntryPopup.IsOpen;
        }

        private void UpdateDynamicButtons()
        {
        }

        private void ViewSourceOnClick(object sender, RoutedEventArgs e)
        {
            // Open the source URL in the default web browser
            QAOpenUrlInBrowser("https://github.com/killer6oose/TimeTrackerWPF");
        }

        public class Entry
        {
            public string FriendlyName { get; set; }
            public string Link { get; set; }
        }
    }
}