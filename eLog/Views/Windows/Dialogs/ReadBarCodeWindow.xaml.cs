using System.Windows;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ReadBarCodeWindow.xaml
    /// </summary>
    public partial class ReadBarCodeWindow : Window
    {
        public ReadBarCodeWindow()
        {
            InitializeComponent();
            BarCodeTextBox.Focus();
        }

        public static readonly DependencyProperty BarCodeProperty =
            DependencyProperty.Register(
                nameof(BarCode),
                typeof(string),
                typeof(ReadBarCodeWindow),
                new PropertyMetadata(default(string)));


        public string BarCode
        {
            get => (string)GetValue(BarCodeProperty);
            set => SetValue(BarCodeProperty, value);
        }
    }
}