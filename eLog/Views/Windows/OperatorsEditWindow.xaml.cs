using eLog.Models;
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

namespace eLog.Views.Windows
{
    

    public partial class OperatorsEditWindow
    {
        public static DependencyProperty OperatorsProperty = 
            DependencyProperty.Register(
                nameof(Operators),
                typeof(List<string>),
                typeof(OperatorsEditWindow),
                new PropertyMetadata(default(string)));
        public List<Operator> Operators { get => (List<Operator>)GetValue(OperatorsProperty) ; set => SetValue(OperatorsProperty, value); }


        public OperatorsEditWindow()
        {
            InitializeComponent();
        }
    }
}
