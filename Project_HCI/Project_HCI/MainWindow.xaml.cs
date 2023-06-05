using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace EmailApplication
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Folder> folders;
        public ObservableCollection<Folder> Folders
        {
            get { return folders; }
            set
            {
                if (folders != value)
                {
                    folders = value;
                    OnPropertyChanged("Folders");
                }
            }
        }

        // Represents the content of the selected email
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

            // Initialize the collection of folders
            Folders = new ObservableCollection<Folder>
            {
                new Folder
                {
                    Name = "Inbox",
                    Emails = new ObservableCollection<Email>
                    {
                        new Email { Subject = "Subject 1", Sender = "diogo.castro@student.um.si", Content = "Email content 1", Recipients = new List<string> { "Recipient 1", "Recipient 2" }, Copies = new List<string> { "Copy 1" }, Attachments = new List<string>()},
                        new Email { Subject = "Subject 2", Sender = "diogo.castro@student.um.si", Content = "Email content 2", Recipients = new List<string> { "Recipient 3" }, Copies = new List<string>(), Attachments = new List<string>()},
                        new Email { Subject = "Subject 3", Sender = "diogo.castro@student.um.si", Content = "Email content 3", Recipients = new List<string> { "Recipient 4" }, Copies = new List<string>(), Attachments = new List<string>()}
                    }
                },
                new Folder { Name = "Sent", Emails = new ObservableCollection<Email>() },
                new Folder { Name = "Trash", Emails = new ObservableCollection<Email>() }
            };

            // Set the items source of the email list to the emails in the "Inbox" folder
            EmailList.ItemsSource = Folders.FirstOrDefault(f => f.Name == "Inbox")?.Emails;

            // Set the data context of the window to itself
            DataContext = this;
        }

        // Event that is triggered when a property value changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Event handler for the selection change in the email list
        private void EmailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Get the selected email and update the selected email content
                Email selectedEmail = (Email)EmailList.SelectedItem;
                SelectedEmailContent = selectedEmail.Content;
            }
        }

        private void EmailList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected email
            Email selectedEmail = (Email)EmailList.SelectedItem;

            if (selectedEmail != null)
            {
                // Create a new compose window and set its data context to the selected email
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);

                viewMessageWindow.DataContext = selectedEmail;

                // Set the fields of the compose window to display the details of the selected email
                viewMessageWindow.Sender.Text = selectedEmail.Sender;
                viewMessageWindow.Subject.Text = selectedEmail.Subject;
                viewMessageWindow.Content.Text = selectedEmail.Content;
                viewMessageWindow.Recipients.Text = string.Join(", ", selectedEmail.Recipients);

                // If there are any copies, display them in the copies field of the compose window
                if (selectedEmail.Copies != null && selectedEmail.Copies.Count > 0)
                {
                    viewMessageWindow.Copies.Text = string.Join(", ", selectedEmail.Copies);
                }

                // If there are any attachments, add them to the attachments list of the compose window
                if (selectedEmail.Attachments != null && selectedEmail.Attachments.Count > 0)
                {
                    foreach (string attachment in selectedEmail.Attachments)
                    {
                        viewMessageWindow.Attachments.Items.Add(attachment);
                    }
                }

                // Disable certain controls in the compose window to make it read-only
                viewMessageWindow.SendButton.IsEnabled = false;
                viewMessageWindow.AttachmentsButton.IsEnabled = false;
                viewMessageWindow.Recipients.IsReadOnly = true;
                viewMessageWindow.Copies.IsReadOnly = true;
                viewMessageWindow.Subject.IsReadOnly = true;
                viewMessageWindow.Content.IsReadOnly = true;

                // Show the compose window as a modal dialog
                viewMessageWindow.ShowDialog();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Event handler for the New Message button click
        private void NewMessage_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the ComposeWindow
            ComposeWindow composeWindow = new ComposeWindow(Folders);
            // Show the ComposeWindow as a modal dialog
            composeWindow.ShowDialog();
        }

        // Event handler for the Remove button click
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Display a confirmation dialog to confirm deletion
                MessageBoxResult result = MessageBox.Show("Do you really want to delete this message?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Get the selected email
                    Email selectedEmail = (Email)EmailList.SelectedItem;

                    // Find the folder that contains the selected email
                    Folder currentFolder = GetFolderOfEmail(selectedEmail);

                    if (currentFolder != null)
                    {
                        // Remove the selected email from the current folder
                        currentFolder.Emails.Remove(selectedEmail);

                        // If the current folder is not the "Trash" folder, move the email to the "Trash" folder
                        if (currentFolder.Name != "Trash")
                        {
                            Folders.FirstOrDefault(f => f.Name == "Trash")?.Emails.Add(selectedEmail);
                        }
                    }

                    // Notify that the "Folders" property has changed (used for data binding)
                    OnPropertyChanged("Folders");
                }
            }
        }

        // Helper method to get the folder that contains the specified email
        private Folder GetFolderOfEmail(Email email)
        {
            foreach (var folder in Folders)
            {
                if (folder.Emails.Contains(email))
                {
                    return folder;
                }
            }

            return null;
        }


        // Event handler for the TreeView's SelectedItemChanged event
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Check if the newly selected item is a Folder object
            if (e.NewValue is Folder selectedFolder)
            {
                // Set the filteredEmails collection to the Emails property of the selectedFolder
                ObservableCollection<Email> filteredEmails = selectedFolder.Emails;

                // Set the ItemsSource of the EmailList control to the filteredEmails collection
                EmailList.ItemsSource = filteredEmails;
            }
        }

        // Event handler for the Import button click
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";

            // Display the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string filePath = openFileDialog.FileName;

                // Call the ImportData method with the file path
                ImportData(filePath);

                // Refresh the GUI elements (This is not working)
                CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
            }

        }

        private void ImportData(string filePath)
        {
            try
            {
                // Load the XML document from the specified file path
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                // Clear the existing folders
                Folders.Clear();

                // Get the root element of the XML document
                XmlElement rootElement = xmlDoc.DocumentElement;

                // Check if the root element is "EmailData"
                if (rootElement.Name != "EmailData")
                {
                    MessageBox.Show("Invalid XML file. Root element 'EmailData' not found.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get the "Folders" element
                XmlNodeList FoldersNodes = rootElement.GetElementsByTagName("Folders");
                if (FoldersNodes.Count == 0)
                {
                    MessageBox.Show("Invalid XML file. 'Folders' element not found.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                XmlNode FoldersNode = FoldersNodes[0];

                // Iterate through each "Folder" element
                foreach (XmlNode folderNode in FoldersNode.ChildNodes)
                {
                    // Check if the node is a "Folder" element
                    if (folderNode.Name == "Folder")
                    {
                        // Get the folder name attribute
                        string folderName = folderNode.Attributes["Name"]?.Value;
                        if (string.IsNullOrEmpty(folderName))
                        {
                            MessageBox.Show("Invalid XML file. Folder name not found.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Create a new Folder object
                        Folder folder = new Folder
                        {
                            Name = folderName,
                            Emails = new ObservableCollection<Email>()
                        };

                        // Iterate through each "Email" element within the current folder
                        foreach (XmlNode emailNode in folderNode.ChildNodes)
                        {
                            // Check if the node is an "Email" element
                            if (emailNode.Name == "Email")
                            {
                                // Get the attributes of the email
                                string subject = emailNode.Attributes["Subject"]?.Value;
                                string sender = emailNode.Attributes["Sender"]?.Value;
                                string content = emailNode.Attributes["Content"]?.Value;
                                string copies = emailNode.Attributes["Copies"]?.Value;
                                string recipients = emailNode.Attributes["Recipients"]?.Value;
                                string attachments = emailNode.Attributes["Attachments"]?.Value;

                                // Create a new Email object
                                Email email = new Email
                                {
                                    Subject = subject,
                                    Sender = sender,
                                    Content = content,
                                    Copies = string.IsNullOrEmpty(copies) ? new List<string>() : copies?.Split(',').Select(copie => copie.Trim()).ToList(),
                                    Recipients = string.IsNullOrEmpty(recipients) ? new List<string>() : recipients?.Split(',').Select(recipient => recipient.Trim()).ToList(),
                                    Attachments = string.IsNullOrEmpty(attachments) ? new List<string>() : attachments.Split(',').Select(attachment => attachment.Trim()).ToList()
                                };

                                // Add the email to the folder's Emails collection
                                folder.Emails.Add(email);
                            }
                        }

                        // Add the folder to the Folders collection
                        Folders.Add(folder);
                    }
                }

                // Display a success message
                MessageBox.Show("Import completed successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the view of the EmailList (used for data binding)
                CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs during the import process
                MessageBox.Show("Error occurred during import:\n" + ex.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog to specify the export file path and filter
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml";

            // If the user selects a file and clicks Save
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Create a new XmlDocument to hold the exported data
                    XmlDocument xmlDoc = new XmlDocument();

                    // Create the root element named "EmailData" and append it to the XmlDocument
                    XmlElement rootElement = xmlDoc.CreateElement("EmailData");
                    xmlDoc.AppendChild(rootElement);

                    // Export the folders and their emails
                    ExportFolders(rootElement);

                    // Save the XmlDocument to the specified file path
                    xmlDoc.Save(filePath);

                    // Display a success message
                    MessageBox.Show("Export completed successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Display an error message if an exception occurs during the export process
                    MessageBox.Show("Error occurred during export:\n" + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportFolders(XmlElement parentElement)
        {
            // Create the "Folders" element as a child of the parent element
            XmlElement foldersElement = parentElement.OwnerDocument.CreateElement("Folders");
            parentElement.AppendChild(foldersElement);

            // Iterate through each folder in the Folders collection
            foreach (var folder in Folders)
            {
                // Create a new "Folder" element with the folder name as an attribute
                XmlElement folderElement = parentElement.OwnerDocument.CreateElement("Folder");
                folderElement.SetAttribute("Name", folder.Name);

                // Iterate through each email in the folder's Emails collection
                foreach (var email in folder.Emails)
                {
                    // Create a new "Email" element with the email details as attributes
                    XmlElement emailElement = parentElement.OwnerDocument.CreateElement("Email");
                    emailElement.SetAttribute("Subject", email.Subject);
                    emailElement.SetAttribute("Sender", email.Sender);
                    emailElement.SetAttribute("Content", email.Content);
                    emailElement.SetAttribute("Copies", string.Join(", ", email.Copies));
                    emailElement.SetAttribute("Recipients", string.Join(", ", email.Recipients));
                    emailElement.SetAttribute("Attachments", string.Join(", ", email.Attachments));

                    // Append the "Email" element to the current "Folder" element
                    folderElement.AppendChild(emailElement);
                }

                // Append the "Folder" element to the "Folders" element
                foldersElement.AppendChild(folderElement);
            }
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Get the selected email
                Email selectedEmail = (Email)EmailList.SelectedItem;

                // Create a new compose window
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);

                // Set the data context of the compose window to the selected email
                viewMessageWindow.DataContext = selectedEmail;

                // Set the recipients text box to the recipients of the selected email
                viewMessageWindow.Recipients.Text = string.Join(", ", selectedEmail.Recipients);

                // Show the compose window
                viewMessageWindow.ShowDialog();
            }
        }

        private void ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            // Display a message box indicating that the ReplyAll button was clicked
            MessageBox.Show("ReplyAll clicked!");
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Get the selected email
                Email selectedEmail = (Email)EmailList.SelectedItem;

                // Create a new compose window
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);

                // Set the data context of the compose window to the selected email
                viewMessageWindow.DataContext = selectedEmail;

                // Populate the compose window with the details of the selected email
                viewMessageWindow.Sender.Text = selectedEmail.Sender;
                viewMessageWindow.Subject.Text = selectedEmail.Subject;
                viewMessageWindow.Content.Text = selectedEmail.Content;

                // Populate the copies field if there are any copies
                if (selectedEmail.Copies != null && selectedEmail.Copies.Count > 0)
                {
                    viewMessageWindow.Copies.Text = string.Join(", ", selectedEmail.Copies);
                }

                // Populate the attachments list if there are any attachments
                if (selectedEmail.Attachments != null && selectedEmail.Attachments.Count > 0)
                {
                    foreach (string attachment in selectedEmail.Attachments)
                    {
                        viewMessageWindow.Attachments.Items.Add(attachment);
                    }
                }

                // Display the compose window as a dialog
                viewMessageWindow.ShowDialog();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            // Display a message box indicating that the Search button was clicked
            MessageBox.Show("Search clicked!");
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Display a message box indicating that the Search TextBox was clicked
            MessageBox.Show("Search TextBox clicked!");
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


    public class Folder : INotifyPropertyChanged
    {
        private string name;
        private ObservableCollection<Email> emails;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ObservableCollection<Email> Emails
        {
            get { return emails; }
            set
            {
                if (emails != value)
                {
                    emails = value;
                    OnPropertyChanged(nameof(Emails));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
