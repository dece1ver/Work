using eLog.Infrastructure;
using System.Windows;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EndSetupDialogWindow.xaml
    /// </summary>
    public partial class EndSetupDialogWindow : Window
    {
        public EndSetupResult EndSetupResult { get; set; }

        public EndSetupDialogWindow()
        {
            InitializeComponent();
        }
    }
}