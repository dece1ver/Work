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
    /// Логика взаимодействия для ChangeMachineWindow.xaml
    /// </summary>
    public partial class ChangeMachineWindow : Window
    {
        public static readonly DependencyProperty MachineProperty =
            DependencyProperty.Register(
                nameof(Machine),
                typeof(Machine),
                typeof(ChangeMachineWindow),
                new PropertyMetadata(default(string)));

        public Machine Machine
        {
            get => (Machine)GetValue(MachineProperty);
            set => SetValue(MachineProperty, value);
        }

        public List<Machine> Machines { get; set; } = new();

        public ChangeMachineWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 11; i++)
            {
                Machines.Add(new Machine(i));
            }
        }
    }
}
