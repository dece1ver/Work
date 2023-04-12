using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
using DocumentFormat.OpenXml.Wordprocessing;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Infrastructure.Interfaces;
using eLog.Models;
using eLog.Services;
using static eLog.Views.Windows.Dialogs.EditDetailWindow;
using Text = eLog.Infrastructure.Extensions.Text;
using ValidationResult = System.Windows.Controls.ValidationResult;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDownTimesDialogWindow.xaml
    /// </summary>
    public partial class EditDownTimesDialogWindow : Window, INotifyPropertyChanged, IOverlay
    {
        private PartInfoModel _Part;
        private string _Status;
        private bool _DownTimesIsClosed;
        private Overlay _Overlay = new() {State = false};

        public PartInfoModel Part
        {
            get => _Part;
            set => Set(ref _Part, value);
        }

        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private void DeleteDownTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: DownTime downTime })
            {
                Part.DownTimes.Remove(downTime);
            }
        }

        private void AddDownTimeButton_Click(object sender, RoutedEventArgs e)
        {
            using (Overlay = new())
            {
                var downTimeType = WindowsUserDialogService.SetDownTimeType(this);
                if (downTimeType is { } type)
                {
                    Part.DownTimes.Add(new DownTime(Part, type));
                }
                OnPropertyChanged(nameof(Part.DownTimesIsClosed));
                OnPropertyChanged(nameof(DownTimesIsClosed));
            }
        }

        public bool DownTimesIsClosed
        {
            get => Part.DownTimesIsClosed;
            set => Set(ref _DownTimesIsClosed, value);
        }

        public EditDownTimesDialogWindow(PartInfoModel part)
        {
            _Part = part;
            _Status = string.Empty;
            InitializeComponent();
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

        private object GetPropertyValue(string propertyName)
        {
            return GetType().GetProperty(propertyName)?.GetValue(this, null)!;
        }

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }
    }
}
