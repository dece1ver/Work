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
    /// Логика взаимодействия для PartnameDialogWindow.xaml
    /// </summary>
    public partial class PartnameDialogWindow : Window
    {
        public static readonly DependencyProperty PartNameProperty =
        DependencyProperty.Register(
            nameof(PartName),
            typeof(string),
            typeof(PartnameDialogWindow),
            new PropertyMetadata(string.Empty, OnPartNameChanged));

        public static readonly DependencyProperty IsInputValidProperty =
            DependencyProperty.Register(
                nameof(IsInputValid),
                typeof(bool),
                typeof(PartnameDialogWindow),
                new PropertyMetadata(false));

        public string PartName
        {
            get => (string)GetValue(PartNameProperty);
            set => SetValue(PartNameProperty, value);
        }

        public bool IsInputValid
        {
            get => (bool)GetValue(IsInputValidProperty);
            private set => SetValue(IsInputValidProperty, value);
        }

        public PartnameDialogWindow(string initialValue)
        {
            InitializeComponent();
            DataContext = this;
            PartName = initialValue;
        }

        private static void OnPartNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PartnameDialogWindow dialog)
            {
                dialog.ValidateInput();
            }
        }

        private void ValidateInput()
        {
            IsInputValid = !string.IsNullOrWhiteSpace(PartName);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
