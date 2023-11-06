using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eLog.Infrastructure;
using eLog.Models;

namespace eLog.Views.Windows.Settings;



public partial class OperatorsEditWindow
{
    public ObservableCollection<Operator> Operators { get; set; }

    public OperatorsEditWindow()
    {
        var tempOperators = 

        Operators = new ObservableCollection<Operator>(AppSettings.Instance.Operators.Select(op => new Operator(op)));
        InitializeComponent();
    }

    private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var grid = (DataGrid)sender;
        if (Key.Delete != e.Key) return;
        if (MessageBox.Show(
                $"Удалить оператора?", 
                "Подтверждение!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)
            == MessageBoxResult.Yes 
            && grid.SelectedItem is Operator @operator)
        {
            Operators.Remove(@operator);       
        }
    }
}
