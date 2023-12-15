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

        public double AverageSetupRatio
        {
            get
            {
                var validSetupRatios = Parts.Where(p => p.SetupRatio > 0 && !double.IsNaN(p.SetupRatio) && !double.IsPositiveInfinity(p.SetupRatio)).Select(p => p.SetupRatio);
                return validSetupRatios.Any() ? validSetupRatios.Average() : 0.0;
            }
        }

        public double AverageProductionRatio
        {
            get
            {
                var validProductionRatios = Parts.Where(p => p.ProductionRatio > 0 && !double.IsNaN(p.ProductionRatio) && !double.IsPositiveInfinity(p.ProductionRatio)).Select(p => p.ProductionRatio);
                return validProductionRatios.Any() ? validProductionRatios.Average() : 0.0;
            }
        }

        public double SetupTimeRatio
        {
            get
            {
                double planSum = 0;
                double factSum = 0;
                foreach (var part in Parts)
                {
                    factSum += part.SetupTimeFact + part.PartialSetupTime;
                    planSum += part.SetupTimePlanForReport;
                }
                return factSum == 0 ? 0 : planSum / factSum;
                var validSetupTimes = Parts.Where(p => p.SetupTimePlan > 0 && p.SetupTimeFact > 0 && !double.IsPositiveInfinity(p.SetupTimeFact)).Select(p => p.SetupTimePlan / p.SetupTimeFact);
                return validSetupTimes.Any() ? validSetupTimes.Sum() : 0.0;
            }
        }

        public double ProductionTimeRatio
        {
            get
            {
                double planSum = 0;
                double factSum = 0;
                foreach (var part in Parts)
                {
                    factSum += part.ProductionTimeFact;
                    planSum += part.FinishedCountFact * part.SingleProductionTimePlan;
                }
                return factSum == 0 ? 0 : planSum / factSum;
                var validRatios = Parts.Where(p => p.FinishedCount > 0 && p.SingleProductionTimePlan > 0 && p.ProductionTimeFact > 0 && !double.IsPositiveInfinity(p.ProductionTimeFact)).Select(p => (p.FinishedCount * p.SingleProductionTimePlan) / p.ProductionTimeFact);
                return validRatios.Any() ? validRatios.Sum() : 0.0;
            }
        }

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
