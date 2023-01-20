using eLog.Models;
using eLog.Services.Interfaces;
using eLog.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace eLog.Services
{
    internal class WindowsUserDialogService : IUserDialogService
    {
        public bool Confirm(string message, string caption, bool exclamation = false)
        {
            return MessageBox.Show(
                    message, 
                    caption, 
                    MessageBoxButton.YesNo, 
                    exclamation 
                    ? MessageBoxImage.Exclamation 
                    : MessageBoxImage.Question) 
                == MessageBoxResult.Yes;
        }

        public bool Edit(object item)
        {
            switch (item)
            {
                case ObservableCollection<Operator> operators:
                    EditOperators(ref operators);
                    return true;
                default:
                    return false;
            }
        }

        public void ShowError(string message, string caption) => MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);

        public void ShowInfo(string message, string caption) => MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string caption) => MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);

        public bool EditOperators(ref ObservableCollection<Operator> operators)
        {
            var dlg = new OperatorsEditWindow()
            {
                Operators = operators,
                Owner = Application.Current.MainWindow,
            };
            if (dlg.ShowDialog() != true) return false;

            operators = dlg.Operators;

            return true;
        }

        public bool GetBarCode(ref string barCode)
        {
            var dlg = new ReadBarCodeWindow()
            {
                BarCode = string.Empty
            };
            if (dlg.ShowDialog() != true) return false;
            barCode = dlg.BarCode;
            return true;
        }
    }
}
