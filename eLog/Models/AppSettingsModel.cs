using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    internal class AppSettingsModel
    {
        public AppSettingsModel(Machine machine, string logBasePath, List<Operator> operators, Operator? currentOperator = null)
        {
            Machine = machine;
            LogBasePath = logBasePath;
            Operators = operators;
            CurrentOperator = currentOperator;
        }

        public Machine Machine { get; set; }
        public string LogBasePath { get; set; }
        public List<Operator> Operators { get; set; }
        public Operator? CurrentOperator { get; set; }
    }
}
