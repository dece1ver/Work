using remeLog.Infrastructure;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace remeLog.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (AppSettings.Instance.User == null)
            {
                var dlg = new SetRoleDialog();
                if (dlg.ShowDialog() != true) 
                { 
                    Close();
                    return;
                }
                AppSettings.Instance.User = dlg.SelectedRole;
                AppSettings.Save();
            }
            InitializeComponent();
        }
    }
}
