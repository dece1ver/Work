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
    /// Логика взаимодействия для AssignedPartsWindow.xaml
    /// </summary>
    public partial class AssignedPartsWindow : Window
    {
        public AssignedPartsWindow(HashSet<string> serialParts, string machine)
        {
            SerialParts = serialParts;
            Machine = $"Детали закреплённые за станком {machine}";
            InitializeComponent();
        }

        public HashSet<string> SerialParts { get; init; }
        public string Machine { get; init; }
    }
}
