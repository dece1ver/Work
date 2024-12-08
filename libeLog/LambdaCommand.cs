using libeLog.Base;
using System;

namespace libeLog
{
    public class LambdaCommand : Command
    {
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null!)
        {
            _Execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            _CanExecute = CanExecute ?? (_ => true);
        }

        public static LambdaCommand Create(Action<object> execute, Func<object, bool> canExecute = null!)
        {
            return new LambdaCommand(execute, canExecute);
        }

        public override bool CanExecute(object parameter) => _CanExecute?.Invoke(parameter) ?? true;
        public override void Execute(object parameter) => _Execute(parameter);
    }
}