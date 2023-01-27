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
using eLog.Infrastructure.Extensions.Windows;
using Keyboard = eLog.Infrastructure.Extensions.Windows.Keyboard;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EndSetupDialogWindow.xaml
    /// </summary>
    public partial class EndDetailDialogWindow : Window, INotifyPropertyChanged
    {
        public EndDetailResult EndDetailResult { get; set; }

        public bool CanBeFinished => int.TryParse(_PartsCount, out partsCount) && partsCount >= 0 && _MachineTime.TimeParse(out machineTime) && machineTime.TotalSeconds > 0;

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

        private TimeSpan machineTime;
        private string _machineTime = string.Empty;

        public TimeSpan MachineTime => machineTime;
        public string _MachineTime
        {
            get => _machineTime;
            set
            {
                _machineTime = value;
                OnPropertyChanged(nameof(MachineTime));
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

        #region Keyboard
        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D7);
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D8);
        }

        private void Button9_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D9);
        }

        private void ButtonBackspace_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.Back);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D4);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D5);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D6);
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.Delete);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D1);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D2);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D3);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyDown(Keys.LControlKey);
            Keyboard.KeyPress(Keys.A);
            Keyboard.KeyUp(Keys.LControlKey);
            Keyboard.KeyPress(Keys.Delete);
        }

        private void Button0_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.D0);
        }

        private void ButtonDot_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keyboard.GetKeyboardLayout() == 1033 ? Keys.OemPeriod : Keys.Oem2);
        }

        private void ButtonColon_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.GetKeyboardLayout() == 1033)
            {
                Keyboard.KeyDown(Keys.LShiftKey);
                Keyboard.KeyPress(Keys.Oem1);
                Keyboard.KeyUp(Keys.LShiftKey);
            }
            else
            {
                Keyboard.KeyDown(Keys.LShiftKey);
                Keyboard.KeyPress(Keys.D6);
                Keyboard.KeyUp(Keys.LShiftKey);
            }
        } 
        #endregion
    }
}
