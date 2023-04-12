using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly DependencyProperty AppSettingsProperty =
            DependencyProperty.Register(
                nameof(AppSettings),
                typeof(AppSettingsModel),
                typeof(AppSettingsWindow),
                new PropertyMetadata(default(string)));


        public AppSettings AppSettings
        {
            get => (AppSettings)GetValue(AppSettingsProperty);
            set => SetValue(AppSettingsProperty, value);
        }

        public List<Machine> Machines { get; set; } = new();

        public Machine? CurrentMachine { get; set; }

        public AppSettingsWindow()
        {
            InitializeComponent();
            for (var i = 0; i < 11; i++)
            {
                Machines.Add(new Machine(i));
            }
        }

        private void SetXlPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Excel таблица (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            if (openFileDialog.ShowDialog() != true) return;
            AppSettings.Instance.XlPath = openFileDialog.FileName;
            XlPathTextBox.Text = AppSettings.Instance.XlPath;
        }
        private void SetOrdersSourceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Excel таблица (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            if (openFileDialog.ShowDialog() != true) return;
            AppSettings.OrdersSourcePath = openFileDialog.FileName;
            OrdersSourcePathTextBox.Text = AppSettings.OrdersSourcePath;
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
