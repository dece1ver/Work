using libeLog.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace eLog.Models
{
    public class Operator : INotifyPropertyChanged
    {
        [JsonConstructor]
        public Operator() { }
        public Operator(Operator @operator)
        {
            _FirstName = @operator.FirstName;
            _LastName = @operator.LastName;
            _Patronymic = @operator.Patronymic;
        }

        private string _FirstName = string.Empty;
        private string _LastName = string.Empty;
        private string _Patronymic = string.Empty;

        public string FirstName
        {
            get => _FirstName;
            set => _FirstName = value.Capitalize().Trim();
        }

        public string LastName
        {
            get => _LastName;
            set => _LastName = value.Capitalize().Trim();
        }

        public string Patronymic
        {
            get => _Patronymic;
            set => _Patronymic = value.Capitalize().Trim();
        }

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                var result = LastName;
                if (string.IsNullOrEmpty(FirstName)) return result;
                result += " " + FirstName[0] + ".";
                if (!string.IsNullOrEmpty(Patronymic))
                    result += " " + Patronymic[0] + ".";
                return result;
            }
        }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                var result = LastName.Trim();
                if (string.IsNullOrEmpty(FirstName)) return result;
                result += " " + FirstName.Trim();
                if (!string.IsNullOrEmpty(Patronymic))
                    result += " " + Patronymic.Trim();
                return result;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || GetType() != obj.GetType())
                return false;

            var other = (Operator)obj;
            return FirstName == other.FirstName
                   && LastName == other.LastName
                   && Patronymic == other.Patronymic;
        }

        public override int GetHashCode() => HashCode.Combine(FirstName, LastName, Patronymic);

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
                    dispatcherObject.Dispatcher.Invoke(action, this, args);
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
    }
}