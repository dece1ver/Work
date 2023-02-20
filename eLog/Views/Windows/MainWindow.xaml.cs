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
            switch (AppSettings.IsShiftStarted)
            {
                case false:
                    return;
                case true when AppSettings.Parts.Count == AppSettings.Parts.Count(x => x.IsFinished):
                {
                    var res = MessageBox.Show("При продолжении смена будет завершена автоматически.", "Смена не завершена", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    switch (res)
                    {
                        case MessageBoxResult.OK:
                            AppSettings.IsShiftStarted = false;
                            AppSettings.RewriteConfig();
                            break;
                        case MessageBoxResult.Cancel or _:
                            e.Cancel = true;
                            break;
                    }

                    break;
                }
                case true when AppSettings.Parts.Count != AppSettings.Parts.Count(x => x.IsFinished):
                {
                    _ = MessageBox.Show("Нельзя закрывать журнал пока запущены детали.", "Есть незавершенные детали", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                        break;
                }
            }
        }
    }
}
