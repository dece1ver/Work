using eLog.Models;
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
