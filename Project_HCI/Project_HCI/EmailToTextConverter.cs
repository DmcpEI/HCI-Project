using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace EmailApplication
{
    public class EmailToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Email email)
            {
                // Call the ConcatenateEmailInformation function to get the string representation
                return ConcatenateEmailInformation(email);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ConcatenateEmailInformation(Email email)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Sender: {email.Sender}");
            sb.AppendLine($"Recipients: {string.Join(", ", email.Recipients)}");
            sb.AppendLine($"Copies: {(email.Copies != null ? string.Join(", ", email.Copies) : "")}");
            sb.AppendLine($"Attachments: {(email.Attachments != null ? string.Join(", ", email.Attachments) : "")}");
            sb.AppendLine($"Content: {email.Content}");
            return sb.ToString();
        }

    }

}
