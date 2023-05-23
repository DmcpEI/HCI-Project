using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace EmailApplication
{
    public partial class ComposeWindow : Window
    {

        private Dictionary<string, Folder> folders;
        public ComposeWindow(Dictionary<string, Folder> folders)
        {
            InitializeComponent();
            this.folders = folders;
        }

        private void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Multimedia Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    Attachments.Items.Add(Path.GetFileName(fileName));
                }
            }
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string recipients = Recipients.Text;
            string copies = Copies.Text;
            string subject = Subject.Text;
            string content = Content.Text;
            List<string> attachments = Attachments.Items.Cast<string>().ToList();

            if (string.IsNullOrEmpty(recipients) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please enter recipients and subject.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a new Email instance
            Email email = new Email
            {
                Subject = subject,
                Sender = "diogo.castro@student.um.si",
                Content = content,
                Recipients = recipients.Split(',').Select(recipient => recipient.Trim()).ToList(),
                Copies = copies.Split(',').Select(copie => copie.Trim()).ToList(),
                Attachments = attachments,
            };

            // Prompt the user to confirm the deletion
            MessageBoxResult result = MessageBox.Show("Do you really want to send this message?", "Confirm Send", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Move the selected email to the "Sent" folder
                folders["Sent"].Emails.Add(email); // Add to the Sent folder
            }

            MessageBox.Show("Email Sent");

            // Close the ComposeWindow
            Close();
        }

    }
}
