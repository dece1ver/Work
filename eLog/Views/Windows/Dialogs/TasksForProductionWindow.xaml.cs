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
    /// Логика взаимодействия для TasksForProductionWindow.xaml
    /// </summary>
    public partial class TasksForProductionWindow : Window
    {
        public TasksForProductionWindow(IReadOnlyList<ProductionTaskData> tasks)
        {
            DataContext = this;
            Tasks = tasks;
            InitializeComponent();
        }

        public IReadOnlyList<ProductionTaskData> Tasks { get; set; }
    }
}
