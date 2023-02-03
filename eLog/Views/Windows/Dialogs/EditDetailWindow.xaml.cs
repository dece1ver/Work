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
using eLog.Infrastructure.Extensions;
using eLog.Models;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDetailWindow.xaml
    /// </summary>
    public partial class EditDetailWindow : Window
    {
        public PartInfoModel Part { get; set; }

        public EditDetailWindow(PartInfoModel part)
        {
            Part = part;
            InitializeComponent();
        }

        /// <summary> Реализация поиска номенклатуры по номеру М/Л (имитация) </summary>
        private void FindOrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Part.Order))
            {
                MessageBox.Show("Нет", "Нет", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var tempPart = Part.Order.GetPartFromOrder();
                if (tempPart is null)
                {
                    MessageBox.Show("Заказ не найден.", "Заказ не найден.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Part.Name = tempPart.Name;
                Part.Number = tempPart.Number;
                Part.PartsCount = tempPart.PartsCount;
            }
        }

        /// <summary> Приравнивает окончание наладки к началу </summary>
        private void EndSetupButton_Click(object sender, RoutedEventArgs e)
        {
            Part.StartMachiningTime = Part.StartSetupTime;
        }

        /// <summary> Вставляет текущее время как конец наладки</summary>
        private void EndSetupTimeButton_Click(object sender, RoutedEventArgs e)
        {
            Part.StartMachiningTime = DateTime.Now;
        }

        /// <summary> Вставляет текущее время как конец изготовления</summary>
        private void EndProductionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            Part.EndMachiningTime = DateTime.Now;
        }
    }
}
