using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TimeTrackerWPF
{
    /// <summary>
    /// Interaction logic for QuickActionWindow.xaml
    /// </summary>
    public partial class QuickActionWindow : Window
    {
        private readonly string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".TrackYourTasks", "data.json");
        private List<Entry> entries;

        public QuickActionWindow()
        {
            InitializeComponent();
            EnsureJsonFileExists();
            LoadJsonData();
            GenerateButtonsFromJSON();
            Closing += QuickActionWindow_Closing;
        }

        private void BeforePrjEmailOnClick(object sender, RoutedEventArgs e)
        {
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
                Button button = new Button
                {
                    Content = entry.FriendlyName,
                    Tag = entry.Link // Store link/location as tag for later use
                };
                button.Click += Button_Click; // Attach event handler
                ButtonsPanel.Children.Add(button); // Add button to panel
                ButtonsPanel.Children.Add(new Separator { Height = 10 });
            }
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

        private void QuickActionWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent the QuickActionWindow from closing when the popup is open
            e.Cancel = NewEntryPopup.IsOpen;
        }

        private void UpdateDynamicButtons()
        {
        }

        public class Entry
        {
            public string FriendlyName { get; set; }
            public string Link { get; set; }
        }
    }
}