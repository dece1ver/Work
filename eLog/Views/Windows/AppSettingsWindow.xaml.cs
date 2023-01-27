using eLog.Models;
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

namespace eLog.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AppSettingsWindow.xaml
    /// </summary>
    public partial class AppSettingsWindow : Window
    {
        public static readonly DependencyProperty AppSettingsProperty =
            DependencyProperty.Register(
                nameof(AppSettings),
                typeof(AppSettingsModel),
                typeof(AppSettingsWindow),
                new PropertyMetadata(default(string)));


        public AppSettingsModel AppSettings
        {
            get => (AppSettingsModel)GetValue(AppSettingsProperty);
            set => SetValue(AppSettingsProperty, value);
        }

        public List<Machine> Machines { get; set; } = new();

        public Machine? CurrentMachine { get; set; }

        public AppSettingsWindow()
        {
            InitializeComponent();
            for (var i = 0; i < 11; i++)
            {
                Machines.Add(new Machine(i));
            }
        }
    }
}
