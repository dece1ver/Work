using remeLog.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для EditOperatorsWindow.xaml
    /// </summary>
    public partial class EditOperatorsWindow : Window
    {
        public EditOperatorsWindow()
        {
            InitializeComponent();
        }

        private void EmployeesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (sender is DataGrid grid && grid.SelectedItem is OperatorInfo operatorInfo)
                {
                    if (MessageBox.Show($"Вы уверены, что хотите удалить оператора: \"{operatorInfo.Name}\"?\nЭто действие необратимо.", "Подтверждение удаления",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) 
                    { 
                        e.Handled = true; 
                        return; 
                    }
                }
            }
        }
    }
}
