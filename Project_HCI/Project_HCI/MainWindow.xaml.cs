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
                    OnPropertyChanged("FoldersContent"); // Notify that the FoldersContent property has changed
                }
            }
        }

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

            // Set the initial EmailList items source to the Inbox folder
            EmailList.ItemsSource = Folders.FirstOrDefault(f => f.Name == "Inbox")?.Emails;

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
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);   

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
            ComposeWindow composeWindow = new ComposeWindow(Folders);
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

                        // If the current folder is "Trash", permanently delete the email
                        if (currentFolder.Name != "Trash")
                        {
                            // Otherwise, move the email to the "Trash" folder
                            Folders.FirstOrDefault(f => f.Name == "Trash")?.Emails.Add(selectedEmail);
                        }

                        // Refresh the view to update the UI
                        CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
                    }
                }
            }
        }

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

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Folder selectedFolder)
            {
                ObservableCollection<Email> filteredEmails = selectedFolder.Emails;
                EmailList.ItemsSource = filteredEmails;
            }
        }


        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ImportData(filePath);
            }
        }

        private void ImportData(string filePath)
        {
            try
            {
                // Load the XML document from the specified file
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                Folders.Clear();

                // Get the root element
                XmlElement rootElement = xmlDoc.DocumentElement;
                if (rootElement.Name != "EmailData")
                {
                    MessageBox.Show("Invalid XML file. Root element 'EmailData' not found.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get the folders element
                XmlNodeList FoldersNodes = rootElement.GetElementsByTagName("Folders");
                if (FoldersNodes.Count == 0)
                {
                    MessageBox.Show("Invalid XML file. 'Folders' element not found.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                XmlNode FoldersNode = FoldersNodes[0];

                // Iterate over each folder element
                foreach (XmlNode folderNode in FoldersNode.ChildNodes)
                {
                    if (folderNode.Name == "Folder")
                    {
                        // Get the folder name
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

                        // Get the emails within the folder
                        foreach (XmlNode emailNode in folderNode.ChildNodes)
                        {
                            if (emailNode.Name == "Email")
                            {
                                // Get the email attributes
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

                                // Add the email to the folder
                                folder.Emails.Add(email);
                            }
                        }
                        Folders.Add(folder);
                    }
                }

                // Show a success message
                MessageBox.Show("Import completed successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);

                CollectionViewSource.GetDefaultView(EmailList.ItemsSource).Refresh();
            }
            catch (Exception ex)
            {
                // Show an error message if there was an exception during import
                MessageBox.Show("Error occurred during import:\n" + ex.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            foreach (var folder in Folders)
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
                    emailElement.SetAttribute("Recipients", string.Join(", ", email.Recipients));
                    emailElement.SetAttribute("Attachments", string.Join(", ", email.Attachments));

                    folderElement.AppendChild(emailElement);
                }

                foldersElement.AppendChild(folderElement);
            }
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Move the selected email to the "Trash" folder
                Email selectedEmail = (Email)EmailList.SelectedItem;
                // Create a new instance of the ComposeWindow
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);

                viewMessageWindow.DataContext = selectedEmail;

                viewMessageWindow.Recipients.Text = string.Join(", ", selectedEmail.Recipients);

                // Show the ComposeWindow
                viewMessageWindow.ShowDialog();

            }
        }

        private void ReplyAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ReplyAll clicked!");
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (EmailList.SelectedItem != null)
            {
                // Move the selected email to the "Trash" folder
                Email selectedEmail = (Email)EmailList.SelectedItem;
                // Create a new instance of the ComposeWindow
                ComposeWindow viewMessageWindow = new ComposeWindow(Folders);

                viewMessageWindow.DataContext = selectedEmail;

                // Set the text fields with the converted strings
                viewMessageWindow.Sender.Text = selectedEmail.Sender;
                viewMessageWindow.Subject.Text = selectedEmail.Subject;
                viewMessageWindow.Content.Text = selectedEmail.Content;

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

                // Show the ComposeWindow
                viewMessageWindow.ShowDialog();

            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search clicked!");
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
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

        // INotifyPropertyChanged implementation
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
