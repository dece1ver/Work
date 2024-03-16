using libeLog.Base;
using System;
using System.ComponentModel;

namespace libeLog.Models
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