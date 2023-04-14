using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using eLog.ViewModels.Base;

namespace eLog.Infrastructure.Extensions
{
    public class Overlay : ViewModel, IDisposable, INotifyPropertyChanged
    {
        private bool _State;

        public Overlay(bool state = true)
        {
            _State = state;
        }

        public bool State
        {
            get => _State;
            set => Set(ref _State, value);
        }

        public void Dispose()
        {
            State = false;
        }
    }
}
