using eLog.Infrastructure.Extensions;
using eLog.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Overlay = eLog.Infrastructure.Extensions.Overlay;

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
        private (bool UnSyncAll, bool RemoveLog) serviceResult;

        public static string About { get
            {
                var exe = Environment.ProcessPath;
                var date = exe is null ? string.Empty : $" от {File.GetLastWriteTime(exe).ToString(eLog.Infrastructure.Extensions.Text.DateTimeFormat)}";
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
                    var passWindow = new GetPasswordDialogWindow() { Owner = this };
                    passWindow.ShowDialog();
                    _secretMenuCounter = 0;
                    if (passWindow.Password == Encoding.UTF8.GetString(new byte[] { 97, 54, 54, 54, 114 }))
                    {

                    } else
                    {
                        serviceResult.UnSyncAll = false;
                        serviceResult.RemoveLog = false;
                    }
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
