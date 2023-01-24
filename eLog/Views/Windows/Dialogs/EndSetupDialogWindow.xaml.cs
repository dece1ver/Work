using eLog.Infrastructure;
using eLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
