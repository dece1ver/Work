using eLog.Models;
using eLog.Views.Windows.Dialogs;
using libeLog.Base;
using libeLog.Infrastructure;
using libeLog.Models;
using System.Windows;

namespace eLog.Infrastructure.Commands
{
    class CloseWindowCommand : Command
    {
        public override bool CanExecute(object? parameter) => parameter is Window;

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (Window)parameter!;
            window.Close();
        }
    }

    internal class CloseDialogCommand : Command
    {
        public bool DialogResult { get; set; }
        public override bool CanExecute(object? parameter) => parameter is Window;

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (Window)parameter!;
            window.DialogResult = DialogResult;
            window.Close();
        }
    }

    internal class CloseEndSetupDialogCommand : Command
    {
        public EndSetupResult EndSetupResult { get; set; }
        public override bool CanExecute(object? parameter) => parameter is Window;

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (EndSetupDialogWindow)parameter!;
            window.EndSetupResult = EndSetupResult;
            window.Close();
        }
    }

    internal class SetDownTimeDialogCommand : Command
    {
        public DownTime.Types? DialogResult { get; set; } = null;
        public override bool CanExecute(object? parameter) => parameter is Window;

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (SetDownTimeDialogWindow)parameter!;
            window.Type = DialogResult;
            window.Close();
        }
    }

    internal class CloseEndDetailDialogCommand : Command
    {
        public EndDetailResult EndDetailResult { get; set; }
        public override bool CanExecute(object? parameter) => parameter is Window;

        public override void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            var window = (EndDetailDialogWindow)parameter!;
            window.EndDetailResult = EndDetailResult;
            window.Close();
        }
    }
}