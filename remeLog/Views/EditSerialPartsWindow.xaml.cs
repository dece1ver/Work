using remeLog.Models;
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

namespace remeLog.Views
{
    /// <summary>
    /// Логика взаимодействия для EditSerialPartsWindow.xaml
    /// </summary>
    public partial class EditSerialPartsWindow : Window
    {
        public EditSerialPartsWindow()
        {
            InitializeComponent();
        }

        private void PartsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (sender is DataGrid grid && grid.SelectedItem is SerialPart serialPart)
                {
                    if (MessageBox.Show($"Вы уверены, что хотите удалить деталь: \"{serialPart.PartName}\"?\nЭто действие необратимо.", "Подтверждение удаления",
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
