using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class ShiftHandOverInfo
    {
        public ShiftHandOverInfo(DateTime date, string type, string machine, bool giver, bool workplaceCleaned, bool failures, bool extraneousNoises, bool liquidLeaks, bool toolBreakage, double coolantConcentration)
        {
            Date = date;
            Type = type;
            Machine = machine;
            Giver = giver;
            WorkplaceCleaned = workplaceCleaned;
            Failures = failures;
            ExtraneousNoises = extraneousNoises;
            LiquidLeaks = liquidLeaks;
            ToolBreakage = toolBreakage;
            CoolantConcentration = coolantConcentration;
        }

        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Machine { get; set; }
        public bool Giver { get; set; }
        public bool WorkplaceCleaned { get; set; }
        public bool Failures { get; set; }
        public bool ExtraneousNoises { get; set; }
        public bool LiquidLeaks { get; set; }
        public bool ToolBreakage { get; set; }
        public double CoolantConcentration { get; set; }
    }
}
