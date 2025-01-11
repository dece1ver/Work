using remeLog.Infrastructure;
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
    /// Логика взаимодействия для PartSelectionFilterWindow.xaml
    /// </summary>
    public partial class PartSelectionFilterWindow : Window
    {
        public static readonly DependencyProperty RunCountFilterProperty =
    DependencyProperty.Register(
        nameof(RunCountFilter),
        typeof(string),
        typeof(PartSelectionFilterWindow),
        new PropertyMetadata(string.Empty, OnRunCountFilterChanged));

        public static readonly DependencyProperty IsInputValidProperty =
    DependencyProperty.Register(
        nameof(IsInputValid),
        typeof(bool),
        typeof(PartSelectionFilterWindow),
        new PropertyMetadata(false));

        public static readonly DependencyProperty AddSheetPerMachineProperty =
            DependencyProperty.Register(
                nameof(AddSheetPerMachine),
                typeof(bool),
                typeof(PartSelectionFilterWindow),
                new PropertyMetadata(false));

        public string RunCountFilter
        {
            get => (string)GetValue(RunCountFilterProperty);
            set => SetValue(RunCountFilterProperty, value);
        }

        public bool IsInputValid
        {
            get => (bool)GetValue(IsInputValidProperty);
            private set => SetValue(IsInputValidProperty, value);
        }

        public bool AddSheetPerMachine
        {
            get => (bool)GetValue(AddSheetPerMachineProperty);
            private set => SetValue(AddSheetPerMachineProperty, value);
        }

        public PartSelectionFilterWindow(string initialValue, bool addSheetPerMachine)
        {
            InitializeComponent();
            DataContext = this;
            RunCountFilter = initialValue;
            AddSheetPerMachine = addSheetPerMachine;
        }

        private static void OnRunCountFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PartSelectionFilterWindow dialog)
            {
                dialog.ValidateInput();
            }
        }
        private void ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(RunCountFilter))
            {
                IsInputValid = true;
                return;
            }

            if (Util.TryParseComparison(RunCountFilter, out var op, out var value))
            {
                IsInputValid = value > 0;
                return;
            }

            IsInputValid = int.TryParse(RunCountFilter, out var simpleValue) && simpleValue > 0;
        }

        private void RunCountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void OnPasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!int.TryParse(text, out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
