using eLog.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace eLog.Views.Windows
{


    public partial class OperatorsEditWindow
    {
        public static readonly DependencyProperty OperatorsProperty =
            DependencyProperty.Register(
                nameof(Operators),
                typeof(ObservableCollection<Operator>),
                typeof(OperatorsEditWindow),
                new PropertyMetadata(default(string)));


        public ObservableCollection<Operator> Operators
        {
            get => (ObservableCollection<Operator>)GetValue(OperatorsProperty);
            set => SetValue(OperatorsProperty, value);
        }

        public OperatorsEditWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;
            if ( Key.Delete == e.Key ) {
                if (MessageBox.Show($"Удалить оператора?", "Подтверждение!", MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Operators.Remove(grid.SelectedItem as Operator);       
                }
            }
        }
    }
}
