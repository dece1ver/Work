using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    /// <summary>
    /// Не статический т.к. для записи/чтения json нужен экземпляр
    /// </summary>
    public class AppSettingsModel
    {
        public AppSettingsModel(Machine machine, string xlPath, ObservableCollection<Operator> operators, Operator? currentOperator = null)
        {
            Machine = machine;
            XlPath = xlPath;
            Operators = operators;
            CurrentOperator = currentOperator;
        }

        public Machine Machine { get; set; }
        public string XlPath { get; set; }
        public ObservableCollection<Operator> Operators { get; set; }
        public Operator? CurrentOperator { get; set; }
    }
}
