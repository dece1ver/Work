using eLog.Models;
using libeLog.Models;
using System.Windows;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SetDownTimeDialogWindow.xaml
    /// </summary>
    public partial class SetDownTimeDialogWindow : Window
    {
        public DownTime.Types? Type { get; set; } = null;
        public SetDownTimeDialogWindow()
        {
            InitializeComponent();
        }
    }
}