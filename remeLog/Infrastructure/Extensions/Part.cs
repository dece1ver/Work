using DocumentFormat.OpenXml.Spreadsheet;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;


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

        /// <summary>
        /// Соотношение отмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimes(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
        {
            double sum = 0;
            var filteredParts = shift.Type is ShiftType.All ? parts : parts.Where(p => p.Shift == shift.Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * shift.Minutes;
            return sum;
        }

        public static double SpecifiedDowntimes(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            double sum = 0;
            var filteredParts = shiftType is ShiftType.All ? parts : parts.Where(p => p.Shift == new Shift(shiftType).Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return sum;
        }

        /// <summary>
        /// Соотношение отмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift) 
        {
            double sum = 0;
            var filteredParts = shift.Type is ShiftType.All ? parts : parts.Where(p => p.Shift == shift.Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * shift.Minutes;
            return sum / totalWorkMinutes;
        }

        /// <summary>
        /// Соотношение отмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            double sum = 0;
            var filteredParts = shiftType is ShiftType.All ? parts : parts.Where(p => p.Shift == new Shift(shiftType).Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return sum / totalWorkMinutes;
        }

        /// <summary>
        /// Соотношение частичных наладок к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double PartialSetupRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            var sum = parts.Where(p => shiftType == ShiftType.All || p.Shift == new Shift(shiftType).Name)
                 .Sum(p => p.PartialSetupTime);
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return sum / totalWorkMinutes;
        }

        /// <summary>
        /// Время частичных наладок в минутах
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double PartialSetup(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType) 
            => parts
            .Where(p => shiftType == ShiftType.All || p.Shift == new Shift(shiftType).Name)
            .Sum(p => p.PartialSetupTime);

        /// <summary>
        /// Неотмеченные простои в минутах
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double UnspecifiedDowntimes(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
        {
            double sum = 0;
            var filteredParts = shift.Type is ShiftType.All ? parts : parts.Where(p => p.Shift == shift.Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupTimeFact + part.ProductionTimeFact + part.SetupDowntimes + part.MachiningDowntimes + part.PartialSetupTime;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * shift.Minutes;
            return totalWorkMinutes - sum;
        }

        /// <summary>
        /// Неотмеченные простои в минутах
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double UnspecifiedDowntimes(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            double sum = 0;
            var filteredParts = shiftType is ShiftType.All ? parts : parts.Where(p => p.Shift == new Shift(shiftType).Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupTimeFact + part.ProductionTimeFact + part.SetupDowntimes + part.MachiningDowntimes + part.PartialSetupTime;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return totalWorkMinutes - sum;
        }

        /// <summary>
        /// Соотношение неотмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double UnspecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
        {
            double sum = 0;
            var filteredParts = shift.Type is ShiftType.All ? parts : parts.Where(p => p.Shift == shift.Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupTimeFact + part.ProductionTimeFact + part.SetupDowntimes + part.MachiningDowntimes + part.PartialSetupTime;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * shift.Minutes;
            return (totalWorkMinutes - sum) / totalWorkMinutes;
        }

        /// <summary>
        /// Соотношение неотмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double UnspecifiedDowntimesRatio(this ICollection<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            double sum = 0;
            var filteredParts = shiftType is ShiftType.All ? parts : parts.Where(p => p.Shift == new Shift(shiftType).Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupTimeFact + part.ProductionTimeFact + part.SetupDowntimes + part.MachiningDowntimes + part.PartialSetupTime;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return (totalWorkMinutes - sum) / totalWorkMinutes;
        }
    }
}
