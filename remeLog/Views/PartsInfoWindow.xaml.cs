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

        private void ValidationTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock { Parent: Grid grid }) return;
            foreach (UIElement gridChild in grid.Children)
            {
                if (gridChild is AdornedElementPlaceholder { AdornedElement: TextBlock textBlock } 
                && Validation.GetErrors(textBlock) is ICollection<ValidationError> { Count: > 0 } errors)
                {
                    MessageBox.Show(errors.First().ErrorContent.ToString(), "Некорректный ввод", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                // Получаем элемент, на который было произведено нажатие
                DependencyObject depObj = (DependencyObject)e.OriginalSource;
                // Находим ячейку, к которой относится событие
                DataGridCell cell = FindVisualParent<DataGridCell>(depObj);
                if (cell != null)
                {
                    // Получаем столбец ячейки
                    DataGridColumn column = cell.Column;
                    // Получаем значение ячейки
                    object value = cell.DataContext;
                    // Обработка события
                    // value содержит значение ячейки
                    // column содержит столбец, к которому относится ячейка
                    if (DataContext is PartsInfoWindowViewModel d && value is Part p)
                    {
                        switch (column.DisplayIndex)
                        {
                            case 1:
                                d.ShiftFilter = d.ShiftFilter.FilterText == p.Shift ? new Shift(Infrastructure.Types.ShiftType.All) : new Shift(p.Shift);
                                break;
                            case 2:
                                d.OperatorFilter = d.OperatorFilter == p.Operator ? "" : p.Operator;
                                break;
                            case 3:
                                d.PartNameFilter = d.PartNameFilter == p.PartName ? "" : p.PartName;
                                break;
                            case 4:
                                d.OrderFilter = d.OrderFilter == p.Order ? "" : p.Order;
                                break;
                            case 7:
                                d.SetupFilter = d.SetupFilter == p.Setup ? null : p.Setup;
                                break;
                        }
                    }
                    
                }
            }
        }

        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
