using libeLog.Base;
using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Models
{
    public class SettingsItem : ViewModel
    {
        public SettingsItem(string value)
        {
            _Value = value;
        }

        private string _Value;
        /// <summary> Значение </summary>
        public string Value
        {
            get => _Value;
            set => Set(ref _Value, value);
        }

        private Status _Status = Status.Sync;
        /// <summary> Статус </summary>
        public Status Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }


        private string _Tip = "";
        /// <summary> Подсказка </summary>
        public string Tip
        {
            get => _Tip;
            set => Set(ref _Tip, value);
        }
    }
}
