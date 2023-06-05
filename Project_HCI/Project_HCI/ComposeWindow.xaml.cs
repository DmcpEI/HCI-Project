using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace EmailApplication
{
    public partial class ComposeWindow : Window
    {
        // The collection of folders
        private ObservableCollection<Folder> folders;

        // Constructor
        public ComposeWindow(ObservableCollection<Folder> folders)
        {
            InitializeComponent();
            this.folders = folders;
        }

        // Event handler for the "Add Attachment" button click
        private void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Multimedia Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";

            // Display the file dialog and add selected attachments to the list
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    Attachments.Items.Add(Path.GetFileName(fileName));
                }
            }
        }

        // Event handler for the "Send" button click
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            // Get the input values from the UI elements
            string recipients = Recipients.Text;
            string copies = Copies.Text;
            string subject = Subject.Text;
            string content = Content.Text;
            List<string> attachments = Attachments.Items.Cast<string>().ToList();

            // Validate required fields
            if (string.IsNullOrEmpty(recipients) || string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("Please enter Recipients and Subject.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a new Email object
            Email email = new Email
            {
                Subject = subject,
                Sender = "diogo.castro@student.um.si",
                Content = content,
                Recipients = recipients.Split(',').Select(recipient => recipient.Trim()).ToList(),
                Copies = copies.Split(',').Select(copie => copie.Trim()).ToList(),
                Attachments = attachments,
            };

            // Display confirmation dialog and add email to the "Sent" folder
            MessageBoxResult result = MessageBox.Show("Do you really want to send this message?", "Confirm Send", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                folders.FirstOrDefault(f => f.Name == "Sent")?.Emails.Add(email);
            }

            MessageBox.Show("Email Sent");

            Close();
        }
    }
}
