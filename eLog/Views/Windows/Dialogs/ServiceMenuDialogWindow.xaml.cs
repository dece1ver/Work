using System.Windows;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ServiceMenuDialogWindow.xaml
    /// </summary>
    public partial class ServiceMenuDialogWindow : Window
    {
        public ServiceMenuDialogWindow()
        {
            InitializeComponent();
        }
        public bool UnsyncAllParts { get; set; }
        public bool ClearParts { get; set; }
        public bool ClearLogs { get; set; }
        public bool ResetTasksInfo { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}