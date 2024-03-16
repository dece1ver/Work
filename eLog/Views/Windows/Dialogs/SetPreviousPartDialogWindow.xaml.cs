using eLog.Models;
using libeLog.Models;
using System.Collections.Generic;
using System.Windows;

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