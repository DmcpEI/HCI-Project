using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace EmailApplication
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<string, Folder> folders;  // Dictionary to store folders and their emails
        private string selectedEmailContent;

        public string SelectedEmailContent
        {
            get { return selectedEmailContent; }
            set
            {
                if (selectedEmailContent != value)
                {
                    selectedEmailContent = value;
                    OnPropertyChanged("SelectedEmailContent");
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the folders and emails
            folders = new Dictionary<string, Folder>
            {
                { "Inbox", new Folder
                    {
                        Emails = new ObservableCollection<Email>
                        {
                            new Email { Subject = "Subject 1", Sender = "diogo.castro@student.um.si", Content = "Email content 1", Recipients = new List<string> { "Recipient 1", "Recipient 2" }, Copies = new List<string> { "Copy 1" }, Attachments = new List<string>()},
                            new Email { Subject = "Subject 2", Sender = "diogo.castro@student.um.si", Content = "Email content 2", Recipients = new List<string> { "Recipient 3" }, Copies = new List<string>(), Attachments = new List<string>()},
                            new Email { Subject = "Subject 3", Sender = "diogo.castro@student.um.si", Content = "Email content 3", Recipients = new List<string> { "Recipient 4" }, Copies = new List<string>(), Attachments = new List<string>()}
                        }
                    }
                },
                { "Sent", new Folder () },
                { "Drafts", new Folder () },
                { "Trash", new Folder () }
            };

            // Set the initial EmailList items source to the Inbox folder
            EmailList.ItemsSource = folders["Inbox"].Emails;

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void EmailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection change event
            if (EmailList.SelectedItem != null)
            {
                Email selectedEmail = (Email)EmailList.SelectedItem;
                SelectedEmailContent = selectedEmail.Content;
            }
        }

        private void EmailList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected message from the ListView
            Email selectedEmail = (Email)EmailList.SelectedItem;

            if (selectedEmail != null)
            {
                // Create a new instance of the ComposeWindow
                ComposeWindow viewMessageWindow = new ComposeWindow(folders);

                viewMessageWindow.DataContext = selectedEmail;

                // Set the text fields with the converted strings
                viewMessageWindow.Sender.Text = selectedEmail.Sender;
                viewMessageWindow.Subject.Text = selectedEmail.Subject;
                viewMessageWindow.Content.Text = selectedEmail.Content;
                viewMessageWindow.Recipients.Text = string.Join(", ", selectedEmail.Recipients);

                // Add Copies if available
                if (selectedEmail.Copies != null && selectedEmail.Copies.Count > 0)
                {
                    viewMessageWindow.Copies.Text = string.Join(", ", selectedEmail.Copies);
                }

                // Add Attachments if available
                if (selectedEmail.Attachments != null && selectedEmail.Attachments.Count > 0)
                {
                    foreach (string attachment in selectedEmail.Attachments)
                    {
                        viewMessageWindow.Attachments.Items.Add(attachment);
                    }
                }

                // Disable the send button and make the input fields read-only
                viewMessageWindow.SendButton.IsEnabled = false;
                viewMessageWindow.AttachmentsButton.IsEnabled = false;
                viewMessageWindow.Recipients.IsReadOnly = true;
                viewMessageWindow.Copies.IsReadOnly = true;
                viewMessageWindow.Subject.IsReadOnly = true;
                viewMessageWindow.Content.IsReadOnly = true;

                // Show the ComposeWindow
                viewMessageWindow.ShowDialog();
            }
        }



        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Handle Exit menu item click event
            Close();
        }

        private void NewMessage_Click(object sender, RoutedEventArgs e)
        {
            ComposeWindow composeWindow = new ComposeWindow(folders);
            composeWindow.ShowDialog();
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
                    Folder currentFolder = GetFolderOfEmail(selectedEmail); // Get the current folder of the selected email
                    if (currentFolder != null)
                    {
                        currentFolder.Emails.Remove(selectedEmail); // Remove from the current folder
                        folders["Trash"].Emails.Add(selectedEmail); // Add to the Trash folder
                                                                    // Refresh the view to update the UI
                        CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
                    }
                }
            }
        }

        private Folder GetFolderOfEmail(Email email)
        {
            foreach (var folder in folders.Values)
            {
                if (folder.Emails.Contains(email))
                {
                    return folder;
                }
            }
            return null;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedFolder = (TreeViewItem)e.NewValue;
            string folderName = selectedFolder.Tag.ToString();

            if (folders.ContainsKey(folderName))
            {
                ObservableCollection<Email> filteredEmails = folders[folderName].Emails;
                EmailList.ItemsSource = filteredEmails;
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                // Add logic to handle the imported file
                string filePath = openFileDialog.FileName;
                // TODO: Handle the import logic
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Create a new XML document
                    XmlDocument xmlDoc = new XmlDocument();

                    // Create the root element
                    XmlElement rootElement = xmlDoc.CreateElement("EmailData");
                    xmlDoc.AppendChild(rootElement);

                    // Export folders
                    ExportFolders(rootElement);

                    // Save the XML document to the specified file
                    xmlDoc.Save(filePath);

                    // Show a success message
                    MessageBox.Show("Export completed successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Show an error message if there was an exception during export
                    MessageBox.Show("Error occurred during export:\n" + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportFolders(XmlElement parentElement)
        {
            // Create the folders element
            XmlElement foldersElement = parentElement.OwnerDocument.CreateElement("Folders");
            parentElement.AppendChild(foldersElement);

            // Export each folder
            foreach (var folder in folders.Values)
            {
                XmlElement folderElement = parentElement.OwnerDocument.CreateElement("Folder");
                folderElement.SetAttribute("Name", folder.Name);

                // Export each email within the folder
                foreach (var email in folder.Emails)
                {
                    XmlElement emailElement = parentElement.OwnerDocument.CreateElement("Email");
                    emailElement.SetAttribute("Subject", email.Subject);
                    emailElement.SetAttribute("Sender", email.Sender);
                    emailElement.SetAttribute("Content", email.Content);
                    emailElement.SetAttribute("Copies", string.Join(", ", email.Copies));
                    emailElement.SetAttribute("Recipients", string.Join(", ", email.Copies));
                    emailElement.SetAttribute("Attachments", string.Join(", ", email.Copies));

                    folderElement.AppendChild(emailElement);
                }

                foldersElement.AppendChild(folderElement);
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

    public class Email : INotifyPropertyChanged
    {
        private string subject;
        public string Subject
        {
            get { return subject; }
            set
            {
                if (subject != value)
                {
                    subject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        private string sender;
        public string Sender
        {
            get { return sender; }
            set
            {
                if (sender != value)
                {
                    sender = value;
                    OnPropertyChanged(nameof(Sender));
                }
            }
        }

        private string content;
        public string Content
        {
            get { return content; }
            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        private List<string> recipients;
        public List<string> Recipients
        {
            get { return recipients; }
            set
            {
                if (recipients != value)
                {
                    recipients = value;
                    OnPropertyChanged(nameof(Recipients));
                }
            }
        }

        private List<string> copies;
        public List<string> Copies
        {
            get { return copies; }
            set
            {
                if (copies != value)
                {
                    copies = value;
                    OnPropertyChanged(nameof(Copies));
                }
            }
        }

        private List<string> attachments;
        public List<string> Attachments
        {
            get { return attachments; }
            set
            {
                if (attachments != value)
                {
                    attachments = value;
                    OnPropertyChanged(nameof(Attachments));
                    OnPropertyChanged(nameof(NumberOfAttachments));
                }
            }
        }

        public string NumberOfAttachments
        {
            get
            {
                return "Attachments: " + Attachments.Count.ToString();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Folder
    {
        public string Name { get; set; }
        public ObservableCollection<Email> Emails { get; set; } = new ObservableCollection<Email>();
    }
}
