using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using libeLog.Extensions;
using Microsoft.IdentityModel.Tokens;
using remeLog.Models;
using remeLog.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
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
        enum DataType
        {
            None, Numeric, TimeSpan
        }

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
                            case 37:
                                switch (p.EngineerComment)
                                {
                                    case "":
                                        p.EngineerComment = "Исправлено";
                                        break;
                                    case "Исправлено":
                                        p.EngineerComment = "Исправлено ранее";
                                        break;
                                    case "Исправлено ранее":
                                        p.EngineerComment = "Корректировка не требуется";
                                        break;
                                    case "Корректировка не требуется":
                                        p.EngineerComment = "Ожидание полного изготовления";
                                        break;
                                    case "Ожидание полного изготовления":
                                        p.EngineerComment = "Ожидание решения технолога";
                                        break;
                                    case "Ожидание решения технолога":
                                        p.EngineerComment = "Ожидание решения производства";
                                        break;
                                    case "Ожидание решения производства":
                                        p.EngineerComment = "Изменение технологии";
                                        break;
                                    case "Изменение технологии":
                                        p.EngineerComment = "";
                                        break;
                                }
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


        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && DataContext is PartsInfoWindowViewModel d)
            {
                var selectedCells = dataGrid.SelectedCells;
                if (selectedCells.Count <= 1)
                {
                    d.Status = string.Empty;
                    return;
                }
                string percent = "";
                double sum = 0;
                TimeSpan timeSpan = TimeSpan.Zero;
                int cnt = 0;
                int cntWithioutZeroes = 0;
                foreach (DataGridCellInfo cell in selectedCells.Where(c => c.IsValid))
                {
                    if (cell.Column.DisplayIndex is 8 or 9 or 10)
                    {
                        d.Status = string.Empty;
                        return;
                    }
                    var content = cell.Column.GetCellContent(cell.Item);
                    if (content is TextBlock textBlock)
                    {
                        var value = textBlock.Text;
                        if (value.EndsWith("%"))
                        {
                            percent = "%";
                            value = value.Replace("%", "");
                        }
                        if (double.TryParse(value, out double num))
                        {
                            sum += num;
                            if (num > 0) cntWithioutZeroes++;
                            cnt++;
                        }
                        else if (TimeSpan.TryParse(textBlock.Text, out TimeSpan span))
                        {
                            timeSpan += span;
                            if (timeSpan.Ticks > 0) cntWithioutZeroes++;
                            cnt++;
                        }
                    }
                }
                if (sum > 0 && cnt > 0 && timeSpan == TimeSpan.Zero)
                {
                    d.Status = $"Среднее: {sum / cnt:0.#}{percent} ({sum / cntWithioutZeroes:0.#}{percent})     Количество: {cnt:0.#}     Сумма: {sum}{percent}";
                }
                else if (timeSpan.Ticks > 0 && cnt > 0 && sum == 0)
                {
                    d.Status = $"Среднее: {TimeSpan.FromTicks(timeSpan.Ticks / cnt):hh\\:mm\\:ss} ({TimeSpan.FromTicks(timeSpan.Ticks / cntWithioutZeroes):hh\\:mm\\:ss})     Количество: {cnt}     Сумма: {timeSpan:hh\\:mm\\:ss}";
                }
                else
                {
                    d.Status = string.Empty;
                }
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is DataGrid dataGrid && DataContext is PartsInfoWindowViewModel d)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    switch (e.Key)
                    {
                        case Key.D:
                            var infoCell = dataGrid.SelectedCells.FirstOrDefault();
                            var colIndex = infoCell.Column.DisplayIndex;
                            var infoCellContent = infoCell.Column.GetCellContent(infoCell.Item);
                            var info = infoCellContent is TextBlock tb ? tb.Text : "null";
                            info = $"Выбрано ячеек: {dataGrid.SelectedCells.Count}\n\n" +
                                $"Информация о первой выделенной:\n" +
                                $"Индекс столбца: {colIndex}\n" +
                                $"Тип: {infoCellContent}\n" +
                                $"Содержимое: {info}\n\n" +
                                $"Деталь: {d.SelectedPart?.PartName}";
                            MessageBox.Show(info);
                            e.Handled = true;
                            break;
                        case Key.F:
                            var baseCell = dataGrid.SelectedCells.FirstOrDefault();
                            if (!baseCell.IsValid) return;
                            var content = baseCell.Column.GetCellContent(baseCell.Item);
                            if (content is TextBlock textBlock)
                            {
                                var value = textBlock.Text;
                                foreach (var cell in dataGrid.SelectedCells.Skip(1))
                                {
                                    var cellContent = cell.Column.GetCellContent(cell.Item);
                                    if (cellContent is TextBlock textBlockToUpdate)
                                    {
                                        textBlockToUpdate.Text = value;
                                    }
                                }
                            }
                            //else if (content is ContentPresenter contentPresenter && contentPresenter.Content is TextBlock cpTextBlock)
                            //{
                            //    var value = cpTextBlock.Text;
                            //    foreach (var cell in dataGrid.SelectedCells.Skip(1))
                            //    {
                            //        var cellContent = cell.Column.GetCellContent(cell.Item);
                            //        if (content is ContentPresenter contentPresenterToUpdaTe && contentPresenterToUpdaTe.Content is TextBlock cpTextBlockToUpdate)
                            //        {
                            //            cpTextBlockToUpdate.Text = value;
                            //        }
                            //    }
                            //}



                            
                            e.Handled = true;
                            break;

                        case Key.Delete:
                            if (d.SelectedPart is Part p)
                            {
                                d.DeletePartCommand.Execute(p);
                                e.Handled = true;
                            }
                            break;

                        default:
                            break;
                    }
                    
                }
            }
        }
    }
}
