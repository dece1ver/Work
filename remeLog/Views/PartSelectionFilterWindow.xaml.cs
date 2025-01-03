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
        public static readonly DependencyProperty RunCountProperty =
            DependencyProperty.Register(
                nameof(RunCount),
                typeof(int),
                typeof(PartSelectionFilterWindow),
                new PropertyMetadata(0, OnRunCountChanged));

        public static readonly DependencyProperty IsInputValidProperty =
            DependencyProperty.Register(
                nameof(IsInputValid),
                typeof(bool),
                typeof(PartSelectionFilterWindow),
                new PropertyMetadata(false));

        public int RunCount
        {
            get => (int)GetValue(RunCountProperty);
            set => SetValue(RunCountProperty, value);
        }

        public bool IsInputValid
        {
            get => (bool)GetValue(IsInputValidProperty);
            private set => SetValue(IsInputValidProperty, value);
        }

        public PartSelectionFilterWindow(int initialValue)
        {
            InitializeComponent();
            DataContext = this;
            RunCount = initialValue;
        }

        private static void OnRunCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PartSelectionFilterWindow dialog)
            {
                dialog.ValidateInput();
            }
        }

        private void ValidateInput()
        {
            IsInputValid = RunCount > 0;
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
