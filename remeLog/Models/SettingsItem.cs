using libeLog.Base;
using remeLog.Infrastructure.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
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

        private CheckStatus _Status = CheckStatus.Sync;
        /// <summary> Статус </summary>
        public CheckStatus Status
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
