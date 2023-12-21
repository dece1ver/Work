using DocumentFormat.OpenXml.VariantTypes;
using libeLog.Extensions;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace remeLog.Infrastructure.Extensions
{
    public static class Part
    {
        public static double AverageSetupRatio(this ICollection<Models.Part> parts)
        {
            var validSetupRatios = parts.Where(p => p.SetupRatio > 0 && !double.IsNaN(p.SetupRatio) && !double.IsPositiveInfinity(p.SetupRatio)).Select(p => p.SetupRatio);
            return validSetupRatios.Any() ? validSetupRatios.Average() : 0.0;
        }

        public static double SetupRatio(this ICollection<Models.Part> parts)
        {
            double planSum = 0;
            double factSum = 0;
            foreach (var part in parts)
            {
                factSum += part.SetupTimeFact + part.PartialSetupTime;
                planSum += part.SetupTimePlanForReport;
            }
            return factSum == 0 ? 0 : planSum / factSum;
        }

        public static double AverageProductionRatio(this ICollection<Models.Part> parts)
        {
            var validProductionRatios = parts.Where(p => p.ProductionRatio > 0 && !double.IsNaN(p.ProductionRatio) && !double.IsPositiveInfinity(p.ProductionRatio)).Select(p => p.ProductionRatio);
            return validProductionRatios.Any() ? validProductionRatios.Average() : 0.0;
        }

        public static double ProductionRatio(this ICollection<Models.Part> parts)
        {
            double planSum = 0;
            double factSum = 0;
            foreach (var part in parts)
            {
                factSum += part.ProductionTimeFact;
                planSum += part.FinishedCountFact * part.SingleProductionTimePlan;
            }
            return factSum == 0 ? 0 : planSum / factSum;
        }

        public static double SpecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate)
        {
            double sum = 0;
            foreach (var part in parts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * 1290;
            return sum / totalWorkMinutes;
        }

        public static double UnspecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate)
        {
            double sum = 0;
            foreach (var part in parts)
            {
                sum += part.SetupTimeFact + part.ProductionTimeFact + part.SetupDowntimes + part.MachiningDowntimes + part.PartialSetupTime;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * 1290;
            return (totalWorkMinutes - sum) / totalWorkMinutes;
        }
    }
}
