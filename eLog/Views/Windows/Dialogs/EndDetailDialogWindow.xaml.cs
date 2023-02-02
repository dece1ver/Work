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
using System.Runtime.InteropServices;
using System.Windows.Threading;
using eLog.Infrastructure.Extensions;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EndSetupDialogWindow.xaml
    /// </summary>
    public partial class EndDetailDialogWindow : Window, INotifyPropertyChanged
    {
        private TimeSpan _MachineTime = TimeSpan.Zero;
        private int _PartsFinished;
        private string _MachineTimeText = string.Empty;
        private string _PartsFinishedText = string.Empty;
        public EndDetailResult EndDetailResult { get; set; }

        public PartInfoModel Part { get; set; }

        public string MachineTimeText
        {
            get => _MachineTimeText;
            set
            {
                Set(ref _MachineTimeText, value);
                OnPropertyChanged(nameof(_MachineTime));
                OnPropertyChanged(nameof(Valid));
                if (Valid) Part.MachineTime = _MachineTime;
            }
        }

        public string PartsFinishedText
        {
            get => _PartsFinishedText;
            set
            {
                Set(ref _PartsFinishedText, value);
                OnPropertyChanged(nameof(_PartsFinished));
                OnPropertyChanged(nameof(Valid));
                if (Valid) Part.PartsFinished = _PartsFinished;
            }
        }

        public bool Valid
        {
            get
            {
                var result = int.TryParse(PartsFinishedText, out _PartsFinished) && _PartsFinished > 0 &&
                             MachineTimeText.TimeParse(out _MachineTime) && _MachineTime.TotalSeconds > 0
                             || _PartsFinished == 0 && !string.IsNullOrWhiteSpace(_PartsFinishedText);
                if (!result) return result;
                Part.PartsFinished = _PartsFinished;
                Part.MachineTime = _MachineTime;
                return result;
            }
        }

        public EndDetailDialogWindow(PartInfoModel part)
        {
            Part = part;
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
