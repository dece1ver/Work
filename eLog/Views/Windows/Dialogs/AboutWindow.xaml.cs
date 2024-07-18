using eLog.Infrastructure.Extensions;
using libeLog;
using libeLog.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Overlay = libeLog.Models.Overlay;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window, IOverlay, INotifyPropertyChanged
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private int _secretMenuCounter = 0;
        public (bool UnSyncAll, bool ClearParts, bool ClearLogs, bool ResetTasksInfo) ServiceResult;

        public static string About
        {
            get
            {
                var exe = Environment.ProcessPath;
                var date = exe is null ? string.Empty : $" от {File.GetLastWriteTime(exe).ToString(Constants.DateTimeFormat)}";
                var ver = Assembly.GetExecutingAssembly().GetName().Version!;
                return $"v{ver.Major}.{ver.Minor}.{ver.Build}{date}";
            }
        }

        private Overlay _Overlay = new() { State = false };
        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _secretMenuCounter++;
            if (_secretMenuCounter >= 5)
            {
                using (Overlay = new())
                {
                    Util.WriteLog("Попытка входа в сервисное меню.");
                    ServiceResult.UnSyncAll = false;
                    ServiceResult.ClearLogs = false;

                    var passWindow = new GetPasswordDialogWindow() { Owner = this };

                    _secretMenuCounter = 0;
                    if ((bool)passWindow.ShowDialog()! && passWindow.Password == Encoding.UTF8.GetString(new byte[] { 97, 54, 54, 54, 114 }))
                    {
                        Util.WriteLog("Успешный вход.");
                        var serviceWindow = new ServiceMenuDialogWindow() { Owner = this };
                        if ((bool)serviceWindow.ShowDialog()!)
                        {
                            ServiceResult.UnSyncAll = serviceWindow.UnsyncAllParts;
                            ServiceResult.ClearParts = serviceWindow.ClearParts;
                            ServiceResult.ClearLogs = serviceWindow.ClearLogs;
                            ServiceResult.ResetTasksInfo = serviceWindow.ResetTasksInfo;
                        }
                    }
                    Util.WriteLog($"serviceResult.UnSyncAll = {ServiceResult.UnSyncAll}");
                    Util.WriteLog($"serviceResult.ClearParts = {ServiceResult.ClearParts}");
                    Util.WriteLog($"serviceResult.ClearLogs = {ServiceResult.ClearLogs}");
                    Util.WriteLog($"serviceResult.ResetTasksInfo = {ServiceResult.ResetTasksInfo}");
                    Debug.Print($"serviceResult.UnSyncAll = {ServiceResult.UnSyncAll}");
                    Debug.Print($"serviceResult.ClearParts = {ServiceResult.ClearParts}");
                    Debug.Print($"serviceResult.ClearLogs = {ServiceResult.ClearLogs}");
                    Debug.Print($"serviceResult.ResetTasksInfo = {ServiceResult.ResetTasksInfo}");
                }
            }
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