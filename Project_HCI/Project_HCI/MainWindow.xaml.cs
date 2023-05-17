using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EmailApplication
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, List<Email>> folders;  // Dictionary to store folders and their emails

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the folders and emails
            folders = new Dictionary<string, List<Email>>
            {
                { "Inbox", new List<Email>
                    {
                        new Email { Subject = "Subject 1", Sender = "Sender 1", Content = "Email content 1", Recipients = new List<string> { "Recipient 1", "Recipient 2" }, Copies = new List<string> { "Copy 1" }, Attachments = new List<string> { "Attachment1.pdf", "Attachment2.docx" } },
                        new Email { Subject = "Subject 2", Sender = "Sender 2", Content = "Email content 2", Recipients = new List<string> { "Recipient 3" }, Copies = new List<string>(), Attachments = new List<string>() },
                        new Email { Subject = "Subject 3", Sender = "Sender 3", Content = "Email content 3", Recipients = new List<string> { "Recipient 4" }, Copies = new List<string>(), Attachments = new List<string>() }
                    }
                },
                { "Sent", new List<Email>() },
                { "Drafts", new List<Email>() },
                { "Trash", new List<Email>() }
            };

            // Set the initial EmailList items source to the Inbox folder
            EmailList.ItemsSource = folders["Inbox"];
        }

        private void EmailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection change event
            if (EmailList.SelectedItem != null)
            {
                Email selectedEmail = (Email)EmailList.SelectedItem;
                EmailContentTextBlock.Text = selectedEmail.Content;
            }
        }

        private void EmailList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Handle double-click event
            if (EmailList.SelectedItem != null)
            {
                Email selectedEmail = (Email)EmailList.SelectedItem;
                EmailContentTextBlock.Text = selectedEmail.Content;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Handle Exit menu item click event
            Close();
        }

        private void NewMessage_Click(object sender, RoutedEventArgs e)
        {
            // Handle New Message menu item click event
            // Add your logic here to open a new message window or perform other actions
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Prompt the user to confirm the deletion
                MessageBoxResult result = MessageBox.Show("Do you really want to delete this message?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Move the selected email to the "Trash" folder
                    Email selectedEmail = (Email)EmailList.SelectedItem;
                    folders["Inbox"].Remove(selectedEmail);  // Remove from the Inbox folder
                    folders["Trash"].Add(selectedEmail);  // Add to the Trash folder

                    // Refresh the view to update the UI
                    CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
                }
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedFolder = (TreeViewItem)e.NewValue;
            string folderName = selectedFolder.Tag.ToString();

            if (folders.ContainsKey(folderName))
            {
                List<Email> filteredEmails = folders[folderName];
                EmailList.ItemsSource = filteredEmails;
            }
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            // Handle Reply button click event
            // Add your logic here to reply to the selected email
        }

        private void ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            // Handle Reply All button click event
            // Add your logic here to reply to all recipients of the selected email
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            // Handle Forward button click event
            // Add your logic here to forward the selected email
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            // Handle Search button click event
            // Add your logic here to perform a search based on the entered text in SearchTextBox
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Handle SearchTextBox got focus event
            // Add your logic here to clear the SearchTextBox or provide user instructions if needed
        }
    }

    public class Email
    {
        public string Subject { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public List<string> Recipients { get; set; }
        public List<string> Copies { get; set; }
        public List<string> Attachments { get; set; }
        public Folder ParentFolder { get; set; }
    }

    public class Folder
    {
        public string Name { get; set; }
        public List<Email> Emails { get; set; }
    }
}
