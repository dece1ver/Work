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
using eLog.Models;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SetPreviousPartDialogWindow.xaml
    /// </summary>
    public partial class SetPreviousPartDialogWindow : Window
    {
        private Part? _Part;

        public Part? Part
        {
            get => _Part;
            set
            {
                _Part = value;
                OkButton.IsEnabled = _Part != null;
            }
        }

        public List<Part> Parts { get; set; }
        public SetPreviousPartDialogWindow(List<Part> parts)
        {
            Parts = parts;
            InitializeComponent();
            Part = null;
        }
    }
}
