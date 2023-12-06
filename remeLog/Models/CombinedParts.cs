using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class CombinedParts
    {
        public CombinedParts(string machine)
        {
            Machine = machine;
        }

        public string Machine { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ObservableCollection<Part> Parts { get; set; } = new();

        public double AverageSetupRatio => (Parts.Any() ? Parts.Average(part => part.SetupTimePlan != 0
            ? (double)part.SetupTimePlan / part.SetupTimeFact : 0)
            : 0);
        public double TotalRatio => Parts.Sum(part => part.SetupTimePlanForReport) != 0
            ? (double)Parts.Sum(part => part.SetupTimeFact) / Parts.Sum(part => part.SetupTimePlanForReport)
            : 0;
        public string SetupRatio => FromDate == ToDate ? $"{(AverageSetupRatio * 100):N0}%" : $"{(TotalRatio * 100):N0}%";
        public string SetupRatio2 => $"{(TotalRatio * 100):N0}%";
        public string SetupRatio3 => $"{(AverageSetupRatio * 100):N0}%";

        string GetOperators(string shiftType)
        {
            var operatorsString = string.Join(", ",
                Parts
                    .Where(s => s.Shift == shiftType)
                    .Select(s => s.Operator)
                    .Distinct());

            return operatorsString.Length > 0 ? operatorsString : "";
        }

        //double GetAverageSetup()
        //{
        //    if (Parts.Count == 0)
        //        return 0;
        //    double sum = 0;
        //    foreach (var part in Parts)
        //    {
        //        if (part.SetupTimePlanForReport != 0)
        //        {
        //            sum += (double)part.SetupFactTime / part.PlanTime;
        //        }
        //    }
        //}
    }
}
