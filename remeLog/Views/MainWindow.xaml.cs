using libeLog.Base;
using remeLog.Infrastructure;
using remeLog.ViewModels;
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel dx)
            {
                try
                {
                    await dx.InitializeAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupDataContext();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CleanupDataContext();
        }

        private void CleanupDataContext()
        {
            if (this.DataContext is MainWindowViewModel dx)
            {
                dx.StopBackgroundWorker();
            }
        }
    }
} 
