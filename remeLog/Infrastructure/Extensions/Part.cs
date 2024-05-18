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
        public static double AverageReplacementTimeRatio(this IEnumerable<Models.Part> parts)
        {
            var validReplacementTimesRatios = parts.Where(p => p.PartReplacementTime != 0 && !double.IsNaN(p.PartReplacementTime) && !double.IsPositiveInfinity(p.PartReplacementTime)).Select(p => p.PartReplacementTime);
            return validReplacementTimesRatios.Any() ? validReplacementTimesRatios.Average() : 0.0;
        }
        public static double AverageSetupRatio(this IEnumerable<Models.Part> parts)
        {
            var validSetupRatios = parts.Where(p => p.SetupRatio > 0 && !double.IsNaN(p.SetupRatio) && !double.IsPositiveInfinity(p.SetupRatio)).Select(p => p.SetupRatio);
            return validSetupRatios.Any() ? validSetupRatios.Average() : 0.0;
        }

        public static double SetupRatio(this IEnumerable<Models.Part> parts)
        {
            double factSum = 0;
            foreach (var part in parts)
            {
                // убрать условие для учета без нормативов
                if (part.SetupTimePlanForCalc != 0) factSum += part.SetupTimeFact + part.PartialSetupTime;
            }
            var factPlanSum = SetupTimePlanForReport(parts);
            return factSum == 0 ? 0 : factPlanSum / factSum;
        }

        public static double AverageProductionRatio(this IEnumerable<Models.Part> parts)
        {
            var validProductionRatios = parts.Where(p => p.ProductionRatio > 0 && !double.IsNaN(p.ProductionRatio) && !double.IsPositiveInfinity(p.ProductionRatio)).Select(p => p.ProductionRatio);
            return validProductionRatios.Any() ? validProductionRatios.Average() : 0.0;
        }

        public static double ProductionRatio(this IEnumerable<Models.Part> parts)
        {
            double planSum = 0;
            double factSum = 0;
            foreach (var part in parts.Where(p => p.ProductionTimePlanForCalc > 0))
            {
                factSum += part.ProductionTimeFact;
                planSum += part.FinishedCountFact * part.ProductionTimePlanForCalc;
            }
            return factSum == 0 ? 0 : planSum / factSum;
        }

        /// <summary>
        /// Отмеченные простои за период
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shift">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimes(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
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

        public static double SpecifiedDowntimes(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
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
        public static double SpecifiedDowntimesRatio(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift) 
        {
            double sum = 0;
            var filteredParts = shift.Type is ShiftType.All ? parts : parts.Where(p => p.Shift == shift.Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * shift.Minutes;
            return sum / parts.FullWorkedTime().TotalMinutes;
        }

        /// <summary>
        /// Соотношение отмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shiftType">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimesRatio(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
        {
            double sum = 0;
            var filteredParts = shiftType is ShiftType.All ? parts : parts.Where(p => p.Shift == new Shift(shiftType).Name);
            foreach (var part in filteredParts)
            {
                sum += part.SetupDowntimes + part.MachiningDowntimes;
            }
            var totalWorkMinutes = (toDate.AddDays(1) - fromDate).TotalDays * (int)shiftType;
            return sum / parts.FullWorkedTime().TotalMinutes;
        }

        /// <summary>
        /// Соотношение отмеченных простоев к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="downtimeType">Фильтр по смене</param>
        /// <returns></returns>
        public static double SpecifiedDowntimeRatio(this IEnumerable<Models.Part> parts, Downtime downtimeType)
        {
            double sum = 0;
            foreach (var part in parts)
            {
                switch (downtimeType)
                {
                    case Downtime.Maintenance:
                        sum += part.MaintenanceTime;
                        break;
                    case Downtime.ToolSearching:
                        sum += part.ToolSearchingTime;
                        break;
                    case Downtime.Mentoring:
                        sum += part.MentoringTime;
                        break;
                    case Downtime.ContactingDepartments:
                        sum += part.ContactingDepartmentsTime;
                        break;
                    case Downtime.FixtureMaking:
                        sum += part.FixtureMakingTime;
                        break;
                    case Downtime.HardwareFailure:
                        sum += part.HardwareFailureTime;
                        break;
                }
            }
            return sum / parts.FullWorkedTime().TotalMinutes;
        }

        /// <summary>
        /// Соотношение частичных наладок к общему времени смены
        /// </summary>
        /// <param name="parts">Список изготовлений</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="shiftType">Фильтр по смене</param>
        /// <returns></returns>
        public static double PartialSetupRatio(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
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
        /// <param name="shiftType">Фильтр по смене</param>
        /// <returns></returns>
        public static double PartialSetup(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType) 
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
        public static double UnspecifiedDowntimes(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
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
        public static double UnspecifiedDowntimes(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
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
        public static double UnspecifiedDowntimesRatio(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, Shift shift)
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
        public static double UnspecifiedDowntimesRatio(this IEnumerable<Models.Part> parts, DateTime fromDate, DateTime toDate, ShiftType shiftType)
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

        /// <summary>
        /// Получение времени наладки для отчета один раз на партию, пока не работает нормально на граничных значениях, как вариант надо дергать из БД всю партию и по ней уже смотреть.
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static double SetupTimePlanForReport(this IEnumerable<Models.Part> parts)
        {
            var sum = 0.0;
            Models.Part prevPart = null!;
            foreach (var partsGroup in parts.GroupBy(p => p.Machine))
            {
                foreach (var part in partsGroup.ToList())
                {
                    var setupValue = prevPart != null ?
                                    (prevPart.Setup == part.Setup && prevPart.Order == part.Order && prevPart.PartName == part.PartName ? 0 : part.SetupTimePlanForCalc) :
                                    (part.PartialSetupTime == 0 && part.SetupTimeFact == 0 ? 0 : part.SetupTimePlanForCalc);

                    // раскоментировать для учета без нормативов
                    //if (setupValue == 0 && part.SetupTimePlanForCalc == 0)
                    //{
                    //    setupValue = part.SetupTimeFact + part.PartialSetupTime;
                    //}

                    sum += setupValue;
                    prevPart = part;
                }
            }
            return sum;
        }

        public static TimeSpan FullWorkedTime(this IEnumerable<Models.Part> parts)
        {
            var sum = new TimeSpan();
            foreach (var part in parts)
            {
                sum += part.EndMachiningTime - part.StartSetupTime;
            }
            return sum;
        }
    }
}
