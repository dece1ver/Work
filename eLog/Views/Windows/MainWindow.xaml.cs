using eLog.Infrastructure;
using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using eLog.Models;

namespace eLog.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += OnWindowClosing!;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            AppSettings.Save();
            switch (AppSettings.Instance.IsShiftStarted)
            {
                case false:
                    return;
                case true when AppSettings.Instance.Parts.Count == AppSettings.Instance.Parts.Count(x => x.IsFinished is not Part.State.InProgress):
                {
                    var res = MessageBox.Show("Смена не завершена.", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    switch (res)
                    {
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.Cancel or _:
                            e.Cancel = true;
                            break;
                    }
                    break;
                }
                case true when AppSettings.Instance.Parts.Count != AppSettings.Instance.Parts.Count(x => x.IsFinished is not Part.State.InProgress):
                {
                    var res = MessageBox.Show("Есть незавершенные детали.", "Внимание!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    switch (res)
                    {
                        case MessageBoxResult.OK:
                            break;
                        case MessageBoxResult.Cancel or _:
                            e.Cancel = true;
                            break;
                    }
                    break;
                }
            }
        }
    }
}
