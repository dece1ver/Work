using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    internal class AppSettingsModel
    {
        public AppSettingsModel(Machine machine, string logBasePath, ObservableCollection<Operator> operators, Operator? currentOperator = null, bool isShiftStarted = false)
        {
            Machine = machine;
            LogBasePath = logBasePath;
            Operators = operators;
            CurrentOperator = currentOperator;
            IsShiftStarted = isShiftStarted;
        }

        public Machine Machine { get; set; }
        public string LogBasePath { get; set; }
        public ObservableCollection<Operator> Operators { get; set; }
        public Operator? CurrentOperator { get; set; }
        public bool IsShiftStarted {get; set; }
    }
}
