using libeLog.Models;
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
    /// Логика взаимодействия для EditSerialPartNormativeWindow.xaml
    /// </summary>
    public partial class EditSerialPartNormativeWindow : Window
    {
        public enum OperationDisplayMode
        {
            Setup, Production
        }

        public static readonly DependencyProperty SerialPartProperty =
        DependencyProperty.Register(nameof(SerialPart), typeof(SerialPart), typeof(EditSerialPartNormativeWindow),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(OperationDisplayMode), typeof(EditSerialPartNormativeWindow),
                new PropertyMetadata(OperationDisplayMode.Setup));

        public SerialPart SerialPart
        {
            get => (SerialPart)GetValue(SerialPartProperty);
            set => SetValue(SerialPartProperty, value);
        }

        public OperationDisplayMode DisplayMode
        {
            get => (OperationDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        public double NewNormative { get; set; }

        public EditSerialPartNormativeWindow()
        {
            InitializeComponent();
        }
    }
}
