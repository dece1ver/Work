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
    public partial class EndDetailDialogWindow : Window
    {
        public EndDetailResult EndDetailResult { get; set; }

        public bool CanBeFinished => Int32.TryParse(_PartsCount, out partsCount);

        private int partsCount;

        public int PartsCount => partsCount;

        public string _PartsCount { get; set; } = string.Empty;

        public EndDetailDialogWindow()
        {
            InitializeComponent();
        }
    }
}
