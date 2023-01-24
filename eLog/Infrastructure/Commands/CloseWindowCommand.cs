using eLog.Infrastructure.Commands.Base;
using eLog.Models;
using eLog.Views.Windows;
using eLog.Views.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace eLog.Infrastructure.Commands
{
    class CloseWindowCommand : Command
    {
        public override bool CanExecute(object parameter) => parameter is Window;

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (Window)parameter;
            window.Close();
        }
    }

    class CloseDialogCommand : Command
    {
        public bool DialogResult { get; set; }
        public override bool CanExecute(object parameter) => parameter is Window;

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (Window)parameter;
            window.DialogResult = DialogResult;
            window.Close();
        }
    }

    class CloseEndSetupDialogCommand : Command
    {
        public EndSetupResult EndSetupResult { get; set; }
        public override bool CanExecute(object parameter) => parameter is Window;

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (EndSetupDialogWindow)parameter;
            window.EndSetupResult = EndSetupResult;
            window.Close();
        }

    }

    class CloseEndDetailDialogCommand : Command
    {
        public EndDetailResult EndDetailResult { get; set; }
        public override bool CanExecute(object parameter) => parameter is Window;

        public override void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (EndDetailDialogWindow)parameter;
            window.EndDetailResult = EndDetailResult;
            window.Close();
        }
    }
}