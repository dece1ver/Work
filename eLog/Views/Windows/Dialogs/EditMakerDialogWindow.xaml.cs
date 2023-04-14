using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Models;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditMakerDialogWindow.xaml
    /// </summary>
    public partial class EditMakerDialogWindow : Window, INotifyPropertyChanged
    {
        public Operator Operator { get; set; }
        public string Shift { get; set; }
        public static string[] Shifts => Text.Shifts;
        public EditMakerDialogWindow(Part part)
        {
            Operator = part.Operator;
            Shift = part.Shift;
            InitializeComponent();
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(Operator));
            OnPropertyChanged(nameof(Operator.DisplayName));
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
        {
            var handlers = PropertyChanged;
            if (handlers is null) return;

            var invokationList = handlers.GetInvocationList();
            var args = new PropertyChangedEventArgs(PropertyName);

            foreach (var action in invokationList)
            {
                if (action.Target is DispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action, this, args);
                }
                else
                {
                    action.DynamicInvoke(this, args);
                }
            }
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }




        #endregion
    }
}
