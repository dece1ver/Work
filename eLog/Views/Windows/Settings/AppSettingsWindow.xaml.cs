using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using eLog.Infrastructure;
using eLog.Models;
using Microsoft.Win32;

namespace eLog.Views.Windows.Settings
{
    /// <summary>
    /// Логика взаимодействия для AppSettingsWindow.xaml
    /// </summary>
    public partial class AppSettingsWindow : Window, INotifyPropertyChanged
    {
        private string _XlPath;
        private string _OrdersSourcePath;
        private Machine _Machine;
        private string[] _OrderQualifiers;
        public IReadOnlyList<Machine> Machines { get; }

        public string XlPath
        {
            get => _XlPath;
            set => Set(ref _XlPath, value);
        }

        public string OrdersSourcePath
        {
            get => _OrdersSourcePath;
            set => Set(ref _OrdersSourcePath, value);
        }


        public Machine Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        public string[] OrderQualifiers
        {
            get => _OrderQualifiers;
            set => Set(ref _OrderQualifiers, value);
        }

        public AppSettingsWindow()
        {
           
            Machines = Enumerable.Range(0, 11).Select(i => new Machine(i)).ToList();
            _XlPath = AppSettings.Instance.XlPath;
            _OrdersSourcePath = AppSettings.Instance.OrdersSourcePath;
            _OrderQualifiers = AppSettings.Instance.OrderQualifiers;
            _Machine = Machines.First(x => x.Id == AppSettings.Instance.Machine.Id);
            InitializeComponent();
        }

        private void SetXlPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Excel таблица (*.xlsx)|*.xlsx|Excel таблица с макросами(*.xlsm)|*.xlsm",
                DefaultExt = "xlsx"
            };
            if (dlg.ShowDialog() != true) return;
            XlPath = dlg.FileName;
        }
        private void SetOrdersSourceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Excel таблица (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            if (dlg.ShowDialog() != true) return;
            OrdersSourcePath = dlg.FileName;
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
