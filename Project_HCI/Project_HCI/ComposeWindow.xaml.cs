using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace EmailApplication
{
    public partial class ComposeWindow : Window
    {
        public ComposeWindow()
        {
            InitializeComponent();
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
            string subject = Subject.Text;

            if (string.IsNullOrEmpty(recipients) || string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("Please enter recipients and subject.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Send the message and add it to the "Sent items" folder

            MessageBox.Show("Message sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Close the ComposeWindow
            Close();
        }
    }
}
