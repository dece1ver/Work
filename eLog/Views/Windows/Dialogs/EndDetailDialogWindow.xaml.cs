using eLog.Infrastructure;
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
using eLog.ViewModels.Base;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EndSetupDialogWindow.xaml
    /// </summary>
    public partial class EndDetailDialogWindow : Window, INotifyPropertyChanged
    {
        public EndDetailResult EndDetailResult { get; set; }

        public bool CanBeFinished => int.TryParse(_PartsCount, out partsCount) && partsCount > 0;

        private int partsCount;
        private string _partsCount = string.Empty;

        public int PartsCount => partsCount;

        public string _PartsCount
        {
            get => _partsCount;
            set
            {
                _partsCount = value;
                OnPropertyChanged(nameof(PartsCount));
                OnPropertyChanged(nameof(CanBeFinished));
            }
        }

        public EndDetailDialogWindow()
        {
            InitializeComponent();
            PartsCountTextBox.Focus();
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
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

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

        #endregion
    }
}
