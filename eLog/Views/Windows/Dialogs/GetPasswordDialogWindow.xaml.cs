using System.Windows;
using System.Windows.Controls;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для GetPasswordDialogWindow.xaml
    /// </summary>
    public partial class GetPasswordDialogWindow : Window
    {
        public GetPasswordDialogWindow()
        {
            InitializeComponent();
        }

        public string Password { get; set; } = "";

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordPlaceholder.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordPlaceholder.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            if (DataContext != null)
            { ((dynamic)DataContext).Password = ((PasswordBox)sender).Password; }
        }
    }
}