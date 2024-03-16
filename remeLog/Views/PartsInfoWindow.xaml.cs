using libeLog.Extensions;
using remeLog.Models;
using remeLog.ViewModels;
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

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для PartsInfoWindow.xaml
    /// </summary>
    public partial class PartsInfoWindow : Window
    {
        public PartsInfoWindow(CombinedParts parts)
        {
            InitializeComponent();
            DataContext = new PartsInfoWindowViewModel(parts);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ComboBox cb)
            {
                cb.Text = "Фильтр по станку";
            }
        }
    }
}
