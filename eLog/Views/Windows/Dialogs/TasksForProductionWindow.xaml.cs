using eLog.Models;
using eLog.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

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

        private ProductionTaskData? _SelectedTask;

        public ProductionTaskData? SelectedTask
        {
            get { return _SelectedTask; }
            set
            {
                _SelectedTask = value;
                foreach (var task in Tasks)
                {
                    task.IsSelected = false;
                }
                if (SelectedTask is { } st) { st.IsSelected = true; }
            }
        }

        public bool NeedStart { get; set; }


        public IReadOnlyList<ProductionTaskData> Tasks { get; set; }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.MainWindow.DataContext is MainWindowViewModel cxt && cxt.CanAddPart)
            {
                if (MessageBox.Show($"Запустить изготовление {SelectedTask?.PartName} по М/Л {SelectedTask?.Order}?",
                    "Запуск детали",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    NeedStart = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show($"В данный момент нельзя запустить изготовление данной детали т.к. уже запущена другая деталь, либо не начата смена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
