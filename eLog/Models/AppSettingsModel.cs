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
        public AppSettingsModel(Machine machine, string xlPath, string ordersSourcePath, string[] orderQualifiers, ObservableCollection<Operator> operators, string currentShift, ObservableCollection<PartInfoModel> parts, bool isShiftStarted = false, Operator? currentOperator = null)
        {
            Machine = machine;
            XlPath = xlPath;
            OrdersSourcePath = ordersSourcePath;
            OrderQualifiers = orderQualifiers;
            Operators = operators;
            CurrentShift = currentShift;
            Parts = parts;
            IsShiftStarted = isShiftStarted;
            CurrentOperator = currentOperator;
        }

        public Machine Machine { get; set; }
        public string XlPath { get; set; }
        public string OrdersSourcePath { get; set; }
        public string[] OrderQualifiers { get; set; }
        public ObservableCollection<Operator> Operators { get; set; }
        public string CurrentShift { get; set; }
        public ObservableCollection<PartInfoModel> Parts { get; set; }
        public bool IsShiftStarted { get; set; }
        public Operator? CurrentOperator { get; set; }
        
    }
}
