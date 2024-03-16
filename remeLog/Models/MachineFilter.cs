using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class MachineFilter : ViewModel
    {
        public MachineFilter(string machine, bool filter)
        {
            _Machine = machine;
            _Filter = filter;
        }

        private string _Machine;
        /// <summary> Описание </summary>
        public string Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }


        private bool _Filter;
        /// <summary> Описание </summary>
        public bool Filter
        {
            get => _Filter;
            set => Set(ref _Filter, value);
        }

    }
}
