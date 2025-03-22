using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using remeLog.ViewModels;

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для GetDatesWindow.xaml
    /// </summary>
    public partial class GetDatesWindow : Window
    {
        public ObservableCollection<DateTime> Dates { get; set; } = new();

        public GetDatesWindow(DateTime dateTime)
        {
            InitializeComponent();
            DataContext = new GetDatesWindowViewModel(dateTime);
        }
    }
}
