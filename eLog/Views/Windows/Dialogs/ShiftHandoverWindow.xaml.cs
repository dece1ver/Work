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

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ShiftHandoverWindow.xaml
    /// </summary>
    public partial class ShiftHandoverWindow : Window, INotifyPropertyChanged
    {

        private string _TypeTitle;
        /// <summary> Заголовок </summary>
        public string TypeTitle
        {
            get => _TypeTitle;
            set => Set(ref _TypeTitle, value);
        }

        public bool WorkplaceCleaned { get; set; }
        public bool Failures { get; set; }
        public bool ExtraneousNoises { get; set; }
        public bool LiquidLeaks { get; set; }
        public bool ToolBreakage { get; set; }


        private double _СoolantСoncentration;
        /// <summary> Концентрация СОЖ </summary>
        public double СoolantСoncentration
        {
            get => _СoolantСoncentration;
            set => Set(ref _СoolantСoncentration, value);
        }


        public ShiftHandoverWindow(string title)
        {
            InitializeComponent();
            _TypeTitle = title;
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
