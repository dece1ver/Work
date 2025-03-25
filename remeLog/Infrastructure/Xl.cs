using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using libeLog;
using libeLog.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using remeLog.Infrastructure.Extensions;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Part = remeLog.Models.Part;
using CM = remeLog.Infrastructure.ColumnManager;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing;

namespace remeLog.Infrastructure
{
    public static class Xl
    {
        static XLColor _lightRed = XLColor.FromHtml("#DA9694");
        static XLColor _lightGreen = XLColor.FromHtml("#96DA94");

        const double Coefficient1 = 1.2;
        const double Coefficient2 = 1.4;

        /// <summary>
        /// Типы экспорта отчетов операторов.
        /// </summary>
        public enum ExportOperatorReportType
        {
            /// <summary>
            /// Отчет по выполнению норм, где операторы выполнили план ниже нормы.
            /// </summary>
            Under,

            /// <summary>
            /// Отчет по выполнению норм, где операторы выполнили план значительно ниже нормы.
            /// (например, на уровне критической точки).
            /// </summary>
            Below
        }

        /// <summary>
        /// Варианты ориентации заголовков.
        /// </summary>
        public enum HeaderRotateOption
        {
            /// <summary>
            /// Горизонтальная ориентация заголовков.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Вертикальная ориентация заголовков.
            /// </summary>
            Vertical
        }


        /// <summary>
        /// Перечисление, которое определяет полужирность для левого и правого заголовков.
        /// </summary>
        public enum BoldOption
        {
            /// <summary>
            /// Левый заголовок выделен полужирным, правый — нет.
            /// </summary>
            Left,

            /// <summary>
            /// Правый заголовок выделен полужирным, левый — нет.
            /// </summary>
            Right,

            /// <summary>
            /// Оба заголовка выделены полужирным.
            /// </summary>
            Both
        }


        /// <summary>
        /// Отчёт за период
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExportReportForPeroid(ICollection<Part> parts, DateTime fromDate, DateTime toDate, Shift shift, string path, int? underOverBorder, string runCountFilter, bool addSheetPerMachine, IProgress<string> progress)
        {
            progress?.Report("Начало экспорта...");
            underOverBorder ??= 10;


            var runCountCondition = Util.TryParseComparison(runCountFilter, out var comparisonOperator, out var comparisonValue)
                ? Util.CreateComparisonFunc(comparisonOperator, comparisonValue)
                : (count => count >= comparisonValue);

            var tempParts = new List<Part>();

            foreach (var p in parts)
            {
                tempParts.Add(p);
            }

            var machines = new List<string>();
            var res = machines.ReadMachines();
            Database.GetShiftsByPeriod(machines, fromDate, toDate, shift, out List<ShiftInfo> shifts);
            var totalDays = Util.GetWorkDaysBeetween(fromDate, toDate);
            double totalWorkedMinutes;

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет за период");
            ws.Style.Font.FontSize = 12;
            var cm = new CM.Builder()
                .Add(CM.Machine)
                .Add(CM.WorkedShifts)
                .Add(CM.NoOperatorShifts)
                .Add(CM.HardwareRepairShifts)
                .Add(CM.NoPowerShifts)
                .Add(CM.ProcessRelatedLossShifts)
                .Add(CM.UnspecifiedOtherShifts)
                .Add(CM.SetupRatio)
                .Add(CM.SetupRatioIncludeDowntimes)
                .Add(CM.ProductionRatio)
                .Add(CM.ProductionRatioIncludeDowntimes)
                .Add(CM.SetupRatioUnder)
                .Add(CM.ProductionRatioUnder)
                .Add(CM.SetupRatioOver)
                .Add(CM.ProductionRatioOver)
                .Add(CM.SetupUnderOverRatio)
                .Add(CM.ProductionUnderOverRatio)
                .Add(CM.SetupToTotalRatio)
                .Add(CM.ProductionToTotalRatio)
                .Add(CM.ProductionEfficiencyToTotalRatio)
                .Add(CM.AverageSetupTime)
                .Add(CM.TotalSetupTime)
                .Add(CM.TotalProductionTime)
                .Add(CM.TotalDowntimesTime)
                .Add(CM.TotalTime)
                .Add(CM.AverageFinishedCount)
                .Add(CM.AveragePartsCount)
                .Add(CM.SmallProductionsRatio)
                .Add(CM.SmallSeriesRatio)
                .Add(CM.AverageReplacementTime)
                .Add(CM.SpecifiedDowntimes)
                .Add(CM.CreateNcProgramTime)
                .Add(CM.MaintenanceTime)
                .Add(CM.ToolSearchingTime)
                .Add(CM.ToolChangingTime)
                .Add(CM.MentoringTime)
                .Add(CM.ContactingDepartmentsTime)
                .Add(CM.FixtureMakingTime)
                .Add(CM.HardwareFailureTime)
                .Add(CM.UnspecifiedDowntimes)
                .Add(CM.CountPerMachine)
                .Build();
            var headerRow = 2;
            var ci = cm.GetIndexes();
            ConfigureWorksheetHeader(ws, cm, HeaderRotateOption.Vertical, 65, 8);

            var headerRange = ws.Range(2, 1, 2, cm.Count);
            var row = 3;
            var firstDataRow = row;
            progress?.Report("Подготовка данных...");
            var totalUnique = parts
                .Select(p => new
                {
                    Part = p,
                    NormalizedName = NormalizePartName(p.PartName)
                })
                .GroupBy(x => x.NormalizedName)
                .Where(group => runCountFilter == null ||
                       runCountCondition(group.Select(x => x.Part.Order).Distinct().Count()))
                .Select(group => group.First().Part)
                .ToList();

            var filteredParts = parts
                .Where(p => !p.ExcludeFromReports)
                .GroupBy(p => p.Machine)
                .SelectMany(machineGroup =>
                    machineGroup
                        .GroupBy(p => p.PartName)
                        .Where(partGroup => runCountFilter == null ||
                            runCountCondition(partGroup.GroupBy(p => p.Order).Count()))
                        .SelectMany(partGroup => partGroup))
                .OrderBy(p => p.Machine);
            progress?.Report("Формирование общего листа...");
            var totalFinished = filteredParts.DistinctBy(p => p.Order).Sum(p => p.TotalCount);
            var uniqueParts = filteredParts.DistinctBy(p => p.PartName).ToList();

            foreach (var partGroup in filteredParts.GroupBy(p => p.Machine).OrderBy(pg => pg.Key))
            {
                parts = partGroup.OrderBy(p => p.StartSetupTime).ToList();
                totalWorkedMinutes = parts.FullWorkedTime().TotalMinutes;
                ws.Cell(row, ci[CM.Machine]).Value = partGroup.Key;
                ws.Cell(row, ci[CM.WorkedShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s is not ({ Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 }));
                ws.Cell(row, ci[CM.NoOperatorShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие оператора" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, ci[CM.HardwareRepairShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Ремонт оборудования" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, ci[CM.NoPowerShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие электричества" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, ci[CM.ProcessRelatedLossShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Организационные потери" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, ci[CM.UnspecifiedOtherShifts]).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Другое" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is ({ Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 }));
                ws.Cell(row, ci[CM.SetupRatio]).Value = parts.AverageSetupRatio();
                ws.Cell(row, ci[CM.SetupRatioIncludeDowntimes]).Value = parts.AverageSetupRatioIncludeDowntimes();
                ws.Cell(row, ci[CM.ProductionRatio]).Value = parts.ProductionRatio();
                ws.Cell(row, ci[CM.ProductionRatioIncludeDowntimes]).Value = parts.ProductionRatioIncludeDowntimes();
                var setupUnderRatio = parts.Where(p => p.FinishedCountFact < underOverBorder).AverageSetupRatio();
                ws.Cell(row, ci[CM.SetupRatioUnder]).Value = setupUnderRatio;
                var productionUnderRatio = parts.Where(p => p.FinishedCountFact < underOverBorder).ProductionRatio();
                ws.Cell(row, ci[CM.ProductionRatioUnder]).Value = productionUnderRatio;
                var setupOverRatio = parts.Where(p => p.FinishedCountFact >= underOverBorder).AverageSetupRatio();
                ws.Cell(row, ci[CM.SetupRatioOver]).Value = setupOverRatio;
                var productionOverRatio = parts.Where(p => p.FinishedCountFact >= underOverBorder).ProductionRatio();
                ws.Cell(row, ci[CM.ProductionRatioOver]).Value = productionOverRatio;
                ws.Cell(row, ci[CM.SetupUnderOverRatio]).Value = setupUnderRatio == 0 ? 0 : setupOverRatio / setupUnderRatio;
                ws.Cell(row, ci[CM.ProductionUnderOverRatio]).Value = productionUnderRatio == 0 ? 0 : productionOverRatio / productionUnderRatio;
                var setupTimeFactSum = parts.Sum(p => p.SetupTimeFact);
                var prodTimeFactSum = parts.Sum(p => p.ProductionTimeFact);
                ws.Cell(row, ci[CM.SetupToTotalRatio]).Value = 1 - prodTimeFactSum / totalWorkedMinutes - parts.SpecifiedDowntimesRatio(ShiftType.All);
                ws.Cell(row, ci[CM.ProductionToTotalRatio]).Value = prodTimeFactSum / totalWorkedMinutes;
                var prodTimePlanSum = parts.Sum(p => p.PlanForBatch);
                ws.Cell(row, ci[CM.ProductionEfficiencyToTotalRatio]).Value = prodTimePlanSum / totalWorkedMinutes;

                ws.Cell(row, ci[CM.AverageSetupTime]).SetValue(parts.AverageSetupTime().TotalHours);
                ws.Cell(row, ci[CM.TotalSetupTime]).SetValue(parts.TotalSetupTime().TotalHours);
                ws.Cell(row, ci[CM.TotalProductionTime]).SetValue(parts.TotalProductionTime().TotalHours);
                ws.Cell(row, ci[CM.TotalDowntimesTime]).SetValue(parts.TotalDowntimesTime().TotalHours);
                ws.Cell(row, ci[CM.TotalTime]).SetValue(totalWorkedMinutes / 60);

                var uniquePartsPerMachine = parts.DistinctBy(p => p.PartName).ToList();

                var averageFinishedCount = parts.Where(p => p.FinishedCount > 0).Average(p => p.FinishedCountFact);
                var averagePartsCount = uniquePartsPerMachine.Average(p => p.TotalCount);
                var smallProductionsRatio = (double)parts.Count(p => p.FinishedCount <= underOverBorder && p.FinishedCount > 0) / parts.Count;
                var smallSeriesRatio = (double)uniquePartsPerMachine.Count(p => p.TotalCount <= underOverBorder) / uniquePartsPerMachine.Count;

                ws.Cell(row, ci[CM.AverageFinishedCount])
                    .SetValue(averageFinishedCount)
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;

                ws.Cell(row, ci[CM.AveragePartsCount])
                    .SetValue(averagePartsCount)
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;

                ws.Cell(row, ci[CM.SmallProductionsRatio])
                    .SetValue(smallProductionsRatio)
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, ci[CM.SmallSeriesRatio])
                    .SetValue(smallSeriesRatio)
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Range(row, ci[CM.SetupRatio], row, ci[CM.ProductionEfficiencyToTotalRatio]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, ci[CM.AverageReplacementTime])
                    .SetValue(parts.Where(p => p.FinishedCountFact >= underOverBorder).AverageReplacementTimeRatio())
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                ws.Cell(row, ci[CM.SpecifiedDowntimes]).Value = parts.SpecifiedDowntimesRatio(ShiftType.All);
                ws.Cell(row, ci[CM.CreateNcProgramTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.CreateNcProgram);
                ws.Cell(row, ci[CM.MaintenanceTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.Maintenance);
                ws.Cell(row, ci[CM.ToolSearchingTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.ToolSearching);
                ws.Cell(row, ci[CM.ToolChangingTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.ToolChanging);
                ws.Cell(row, ci[CM.MentoringTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.Mentoring);
                ws.Cell(row, ci[CM.ContactingDepartmentsTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.ContactingDepartments);
                ws.Cell(row, ci[CM.FixtureMakingTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.FixtureMaking);
                ws.Cell(row, ci[CM.HardwareFailureTime]).Value = parts.SpecifiedDowntimeRatio(Downtime.HardwareFailure);
                ws.Cell(row, ci[CM.UnspecifiedDowntimes]).Value = parts.UnspecifiedDowntimesRatio(fromDate, toDate, ShiftType.All);
                ws.Cell(row, ci[CM.CountPerMachine]).Value = parts.Count;
                ws.Range(row, ci[CM.SpecifiedDowntimes], row, ci[CM.SpecifiedDowntimes]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                row++;
            }
            var lastDataRow = row - 1;
            //ws.Cell(row, workedShiftsColId).Value = totalDays;
            var dataRange = ws.RangeUsed();

            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            ws.Range(headerRow, ci[CM.Machine], lastDataRow, ci[CM.Machine]).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, ci[CM.SetupRatio], lastDataRow, ci[CM.ProductionRatioIncludeDowntimes]).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, ci[CM.SetupRatio], lastDataRow, ci[CM.ProductionRatioIncludeDowntimes]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.8);
            ws.Range(headerRow, ci[CM.SetupRatioUnder], lastDataRow, ci[CM.ProductionRatioUnder]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent2, 0.8);
            ws.Range(headerRow, ci[CM.SetupRatioOver], lastDataRow, ci[CM.ProductionRatioOver]).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, ci[CM.SetupRatioOver], lastDataRow, ci[CM.ProductionRatioOver]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent3, 0.8);
            ws.Range(headerRow, ci[CM.SetupUnderOverRatio], lastDataRow, ci[CM.ProductionUnderOverRatio]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent4, 0.8);
            ws.Range(headerRow, ci[CM.SetupToTotalRatio], lastDataRow, ci[CM.ProductionEfficiencyToTotalRatio]).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, ci[CM.SetupToTotalRatio], lastDataRow, ci[CM.ProductionEfficiencyToTotalRatio]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent5, 0.8);
            ws.Range(headerRow, ci[CM.SpecifiedDowntimes], lastDataRow, ci[CM.UnspecifiedDowntimes]).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, ci[CM.SpecifiedDowntimes], lastDataRow, ci[CM.UnspecifiedDowntimes]).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent6, 0.8);
            ws.Range(headerRow, ci[CM.SpecifiedDowntimes], lastDataRow, ci[CM.UnspecifiedDowntimes]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
            ws.Range(firstDataRow, ci[CM.WorkedShifts], lastDataRow, ci[CM.WorkedShifts]).Style.Font.FontColor = XLColor.Red;
            ws.Range(firstDataRow, ci[CM.WorkedShifts], lastDataRow, ci[CM.WorkedShifts]).AddConditionalFormat().WhenEquals($"=$B${lastDataRow + 2}").Font.FontColor = XLColor.Green;
            dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRange.SetAutoFilter(true);
            ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Columns().AdjustToContents();
            ws.RowsUsed().Height = 20;
            ws.Row(2).Height = 130;
            var title = $"Отчёт за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            switch (shift.Type)
            {
                case ShiftType.Day:
                    title += " за дневные смены";
                    break;
                case ShiftType.Night:
                    title += " за ночные смены";
                    break;
            }
            if (comparisonValue > 1) title += $" ( {Util.GetOperatorSymbol(comparisonOperator)}{comparisonValue.FormattedLaunches(true)} )";
            ws.Cell(1, 1).Value = title;
            ws.Range(1, 1, 1, cm.Count).Merge();
            ws.Range(1, 1, 1, 1).Style.Font.FontSize = 16;
            ws.Columns(2, cm.Count).Width = 8;

            ws.Cell(row, ci[CM.Machine]).Value = "Итог:";
            ws.Cell(row, ci[CM.Machine]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, ci[CM.Machine], row, ci[CM.UnspecifiedOtherShifts]).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, ci[CM.NoOperatorShifts], row, ci[CM.UnspecifiedOtherShifts]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            for (int col = ci[CM.WorkedShifts]; col <= ci[CM.UnspecifiedOtherShifts]; col++)
            {
                string colLetter = ws.Column(col).ColumnLetter();
                ws.Cell(row, col).FormulaA1 = $"AVERAGE({colLetter}{firstDataRow}:{colLetter}{lastDataRow})/$B${lastDataRow + 2}";
            } 
            for (int col = ci[CM.SetupRatio]; col <= ci[CM.AverageSetupTime]; col++)
            {
                string colLetter = ws.Column(col).ColumnLetter();
                ws.Cell(row, col).FormulaA1 = $"AVERAGE({colLetter}{firstDataRow}:{colLetter}{lastDataRow})";
            }
            for (int col = ci[CM.TotalSetupTime]; col <= ci[CM.TotalTime]; col++)
            {
                string colLetter = ws.Column(col).ColumnLetter();
                ws.Cell(row, col).FormulaA1 = $"SUM({colLetter}{firstDataRow}:{colLetter}{lastDataRow})";
            }
            for (int col = ci[CM.SpecifiedDowntimes]; col <= ci[CM.UnspecifiedDowntimes]; col++)
            {
                string colLetter = ws.Column(col).ColumnLetter();
                ws.Cell(row, col).FormulaA1 = $"AVERAGE({colLetter}{firstDataRow}:{colLetter}{lastDataRow})";
            }
            ws.Range(row, ci[CM.WorkedShifts], row, ci[CM.UnspecifiedOtherShifts]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentPrecision2;
            ws.Range(row, ci[CM.SetupRatio], row, ci[CM.UnspecifiedDowntimes]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
            ws.Range(firstDataRow, ci[CM.AverageSetupTime], row, ci[CM.TotalTime]).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

            ws.Cell(row, ci[CM.WorkedShifts]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, ci[CM.WorkedShifts]).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            row++;
            ws.Cell(row, ci[CM.Machine]).Value = "Рабочих смен:";
            ws.Cell(row, ci[CM.Machine]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, ci[CM.Machine]).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, ci[CM.WorkedShifts]).Value = shift.Type == ShiftType.All ? totalDays * 2 : totalDays;
            ws.Cell(row, ci[CM.WorkedShifts]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, ci[CM.WorkedShifts]).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Columns(ci[CM.SetupUnderOverRatio], ci[CM.ProductionUnderOverRatio]).Hide();

            var partsByMachine = filteredParts.GroupBy(p => p.Machine).ToDictionary(g => g.Key, g => g.ToList());

            var mcm = new CM.Builder()
                        .Add(CM.Date)
                        .Add(CM.Shift)
                        .Add(CM.Operator)
                        .Add(CM.Part)
                        .Add(CM.Order)
                        .Add(CM.TotalByOrder)
                        .Add(CM.Finished)
                        .Add(CM.Setup)
                        .Add(CM.StartSetupTime)
                        .Add(CM.StartMachiningTime)
                        .Add(CM.EndMachiningTime)
                        .Add(CM.SetupTimePlan)
                        .Add(CM.SetupTimeFact)
                        .Add(CM.SingleProductionTimePlan)
                        .Add(CM.MachiningTime)
                        .Add(CM.SingleProductionTime)
                        .Add(CM.PartReplacementTime)
                        .Add(CM.ProductionTimeFact)
                        .Add(CM.PlanForBatch)
                        .Add(CM.OperatorComment)
                        .Add(CM.SetupDowntimes)
                        .Add(CM.MachiningDowntimes)
                        .Add(CM.PartialSetupTime)
                        .Add(CM.CreateNcProgramTime)
                        .Add(CM.MaintenanceTime)
                        .Add(CM.ToolSearchingTime)
                        .Add(CM.MentoringTime)
                        .Add(CM.ContactingDepartmentsTime)
                        .Add(CM.FixtureMakingTime)
                        .Add(CM.HardwareFailureTime)
                        .Add(CM.SpecifiedDowntimesRatio)
                        .Add(CM.SpecifiedDowntimesComment)
                        .Add(CM.SetupRatioTitle)
                        .Add(CM.MasterSetupComment)
                        .Add(CM.ProductionRatioTitle)
                        .Add(CM.MasterProductionComment)
                        .Add(CM.MasterComment)
                        .Add(CM.FixedSetupTimePlan)
                        .Add(CM.FixedProductionTimePlan)
                        .Add(CM.EngineerComment)
                        .Build();
            if (addSheetPerMachine)
            {
                foreach (var machine in partsByMachine.Keys)
                {
                    progress?.Report($"Формирование листа по станку {machine}...");
                    ConfigureMachineSheetForPeriod(wb, partsByMachine[machine], machine, mcm);
                }

                var tcm = new CM
                    .Builder()
                    .Add(CM.Part)
                    .Add(CM.Order)
                    .Build();

                var wst = wb.AddWorksheet("Общий список");
                ConfigureWorksheetHeader(wst, tcm);
                wst.Style.Font.FontSize = 10;
                wst.Style.Alignment.WrapText = true;
                wst.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wst.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ci = tcm.GetIndexes();
                int rowt = 3;
                progress?.Report($"Формирование общего списка номенклатуры...");
                foreach (var part in totalUnique)
                {
                    wst.Cell(rowt, ci[CM.Part]).SetValue(part.PartName);
                    wst.Cell(rowt, ci[CM.Order]).SetValue(part.Order);
                    rowt++;
                }
                wst.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                wst.Columns().AdjustToContents();


                wst.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                wst.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                wst.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                wst.RangeUsed().SetAutoFilter(true);
                wst.Columns().AdjustToContents();
                wst.Row(1).Delete();
                wst.SheetView.FreezeRows(1);

            }
            progress?.Report("Формирование завершено, сохранение файла...");
            wb.SaveAs(path);
            progress?.Report("Формирование завершено, выберите ответ в диалоговом окне");
            //AddDiagramToReportForPeriod(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }

        private static string NormalizePartName(string name) =>
            name.ToLower().Replace("\"", "");

        private static void ConfigureMachineSheetForPeriod(XLWorkbook wb, IEnumerable<Part> parts, string machine, CM cm)
        {
            var ws = wb.AddWorksheet(machine);
            ConfigureWorksheetHeader(ws, cm);
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            var ci = cm.GetIndexes();
            int row = 3;

            foreach (var part in parts.Where(p => p.Machine == machine))
            {
                ws.Cell(row, ci[CM.Date])
                    .SetValue(part.ShiftDate)
                    .Style.DateFormat.Format = "dd.MM.yy";
                ws.Cell(row, ci[CM.Shift]).SetValue(part.Shift);
                ws.Cell(row, ci[CM.Operator]).SetValue(part.Operator);
                ws.Cell(row, ci[CM.Part]).SetValue(part.PartName);
                ws.Cell(row, ci[CM.Order]).SetValue(part.Order);
                ws.Cell(row, ci[CM.TotalByOrder]).SetValue(part.TotalCount);
                ws.Cell(row, ci[CM.Finished]).SetValue(part.FinishedCount);
                ws.Cell(row, ci[CM.Setup]).SetValue(part.Setup);
                ws.Cell(row, ci[CM.StartSetupTime])
                    .SetValue(part.StartSetupTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.StartMachiningTime])
                    .SetValue(part.StartMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.EndMachiningTime])
                    .SetValue(part.EndMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.SetupTimePlan]).SetValue(part.SetupTimePlan);
                ws.Cell(row, ci[CM.SetupTimeFact]).SetValue(part.SetupTimeFact);
                ws.Cell(row, ci[CM.SingleProductionTimePlan]).SetValue(part.SingleProductionTimePlan);
                ws.Cell(row, ci[CM.MachiningTime]).SetValue(part.MachiningTime);
                if (part.SingleProductionTime is double spt && spt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SingleProductionTime])
                        .SetValue(spt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                if (part.PartReplacementTime is double prt && prt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.PartReplacementTime])
                        .SetValue(prt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, ci[CM.ProductionTimeFact]).SetValue(part.ProductionTimeFact);
                ws.Cell(row, ci[CM.PlanForBatch]).SetValue(part.PlanForBatch);
                ws.Cell(row, ci[CM.OperatorComment])
                    .SetValue(part.OperatorComment)
                    .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, ci[CM.SetupDowntimes]).SetValue(part.SetupDowntimes);
                ws.Cell(row, ci[CM.MachiningDowntimes]).SetValue(part.MachiningDowntimes);
                ws.Cell(row, ci[CM.PartialSetupTime]).SetValue(part.PartialSetupTime);
                ws.Cell(row, ci[CM.CreateNcProgramTime]).SetValue(part.CreateNcProgramTime);
                ws.Cell(row, ci[CM.MaintenanceTime]).SetValue(part.MaintenanceTime);
                ws.Cell(row, ci[CM.ToolSearchingTime]).SetValue(part.ToolSearchingTime);
                ws.Cell(row, ci[CM.MentoringTime]).SetValue(part.MentoringTime);
                ws.Cell(row, ci[CM.ContactingDepartmentsTime]).SetValue(part.ContactingDepartmentsTime);
                ws.Cell(row, ci[CM.FixtureMakingTime]).SetValue(part.FixtureMakingTime);
                ws.Cell(row, ci[CM.HardwareFailureTime]).SetValue(part.HardwareFailureTime);
                if (part.SpecifiedDowntimesRatio is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SpecifiedDowntimesRatio])
                        .SetValue(part.SpecifiedDowntimesRatio)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, ci[CM.SpecifiedDowntimesComment]).SetValue(part.SpecifiedDowntimesComment);
                ws.Cell(row, ci[CM.SetupRatioTitle]).SetValue(part.SetupRatioTitle);
                ws.Cell(row, ci[CM.MasterSetupComment]).SetValue(part.MasterSetupComment);
                ws.Cell(row, ci[CM.ProductionRatioTitle]).SetValue(part.ProductionRatioTitle);
                ws.Cell(row, ci[CM.MasterProductionComment]).SetValue(part.MasterMachiningComment);
                ws.Cell(row, ci[CM.MasterComment]).SetValue(part.MasterComment);
                ws.Cell(row, ci[CM.FixedSetupTimePlan]).SetValue(part.FixedSetupTimePlan);
                ws.Cell(row, ci[CM.FixedProductionTimePlan]).SetValue(part.FixedProductionTimePlan);
                ws.Cell(row, ci[CM.EngineerComment]).SetValue(part.EngineerComment);
                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();


            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Column(ci[CM.Operator]).Width = 15;

            ws.Column(ci[CM.Part]).Width = 25;

            ws.Range(3, ci[CM.OperatorComment], row, ci[CM.OperatorComment])
               .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(ci[CM.OperatorComment]).Width = 35;

            ws.Column(ci[CM.MasterSetupComment]).Width = 20;
            ws.Column(ci[CM.MasterProductionComment]).Width = 20;
            ws.Column(ci[CM.MasterComment]).Width = 20;
            ws.Row(1).Delete();
            ws.SheetView.FreezeRows(1);
        }

        /// <summary>
        /// Отчёт за период с использованием EPPlus
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="path"></param>
        /// <param name="underOverBorder"></param>
        /// <returns></returns>
        public static void AddDiagramToReportForPeriod(string path)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo fileInfo = new FileInfo(path);
            using (var package = new ExcelPackage(fileInfo))
            {
                var ws = package.Workbook.Worksheets[0];
                var range = ws.Cells["B18:G18"];
                var headers = ws.Cells["B2:G2"];

                var chart = ws.Drawings.AddChart("Эффективность работы", eChartType.PieExploded) as ExcelPieChart;
                var series = chart?.Series.Add(range, headers);
                chart!.Legend.Position = eLegendPosition.Left;
                chart!.DataLabel.Format = "0.00%";
                chart!.DataLabel.ShowValue = true;
                chart!.DataLabel.ShowCategory = true;
                chart!.DataLabel.ShowLegendKey = true;
                chart!.DataLabel.Position = eLabelPosition.BestFit;
                chart!.DataLabel.ShowLeaderLines = true;
                chart!.DataLabel.SourceLinked = false;

                chart.SetPosition(20, 0, 0, 0);
                chart.SetSize(1000, 800);

                // Заголовок диаграммы
                chart.Title.Text = "Работа оборудования";

                // Сохраняем изменения
                package.Save();
            }

        }

        /// <summary>
        /// Экспорт отчета по операторам
        /// </summary>
        /// <param name="parts">Отметки по которым будут производиться расчеты</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="toDate">Конечная дата</param>
        /// <param name="path">Путь к формируемому файлу</param>
        /// <param name="underOverBorder">Граничное значение отсечки результатов</param>
        /// <param name="reportType">Тип расчета: Under = от underOverBorder, Below = до underOverBorder</param>
        /// <returns>При удачном выполнении возвращает путь к записанному файлу</returns>
        public static string ExportOperatorReport(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path, int minPartsCount, int maxPartsCount, ExportOperatorReportType reportType = ExportOperatorReportType.Under)
        {
            var operators = Database.GetOperators();
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет по операторам");
            var workDays = Util.GetWorkDaysBeetween(fromDate, toDate);
            var cm = new CM.Builder()
                .Add(CM.Operator)
                .Add(CM.Qualification)
                .Add(CM.Machine)
                .Add(CM.SetupRatio, "Наладка средняя")
                .Add(CM.ProductionRatio, "Изготовление общее")
                .Add(CM.AverageReplacementTime)
                .Add(CM.CreateNcProgramTime)
                .Add(CM.MaintenanceTime)
                .Add(CM.ToolSearchingTime)
                .Add(CM.ToolChangingTime)
                .Add(CM.MentoringTime)
                .Add(CM.ContactingDepartmentsTime)
                .Add(CM.FixtureMakingTime)
                .Add(CM.HardwareFailureTime)
                .Add(CM.SpecifiedDowntimes)
                .Add(CM.SpecifiedDowntimesEx, $"Простои{Environment.NewLine}(без отказа оборудования и обучения)")
                .Add(CM.GeneralRatio)
                .Add(CM.SetupsCount)
                .Add(CM.ProductionsCount)
                .Add(CM.WorkedShifts)
                .Add(CM.Coefficient)
                .Build();

            var ci = cm.GetIndexes();


            double? Coefficient(int qualification, double specDowntimesEx, double generalRatio, double averageSetupRatio, int workedShifts)
            {
                return averageSetupRatio < 0.5 || specDowntimesEx > 0.1 || workedShifts < workDays / 4 ? null :
                    (qualification, generalRatio) switch
                    {
                        (1 or 2, > 1) => Coefficient2,     // 1.4
                        (1 or 2, > 0.9) => Coefficient1,   // 1.2
                        (3 or 4, > 1.05) => Coefficient2,  // 1.4
                        (3 or 4, > 0.95) => Coefficient1,  // 1.2
                        (5 or 6, > 1.1) => Coefficient2,   // 1.4
                        (5 or 6, > 1) => Coefficient1,     // 1.2
                        _ => null
                    };
            }
            ConfigureWorksheetHeader(ws, cm);
            var row = 3;
            
            foreach (var partGroup in parts
                .Where(p => !p.ExcludeFromReports)
                .GroupBy(p => new { p.Operator, p.Machine })
                .OrderBy(g => g.Key.Machine)
                .ThenBy(g => g.Key.Operator))
            {
                if (partGroup.Key.Operator.ToLower() == "ученик") continue;
                var filteredParts = partGroup.ToList();

                ws.Cell(row, ci[CM.Operator]).SetValue(partGroup.Key.Operator);
                var qualification = partGroup.Key.Operator.GetOperatorQualification(operators);
                var validQualification = int.TryParse(qualification, out int qualificationNumber);
                ws.Cell(row, ci[CM.Qualification]).SetValue(validQualification ? qualificationNumber : qualification);
                ws.Cell(row, ci[CM.Machine]).SetValue(filteredParts.First().Machine);

                var averageSetupRatio = filteredParts.AverageSetupRatio();
                ws.Cell(row, ci[CM.SetupRatio])
                  .SetValue(averageSetupRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var productionRatio = filteredParts.Where(p => p.FinishedCountFact >= minPartsCount && p.FinishedCountFact < maxPartsCount && !p.ExcludeFromReports).ProductionRatio();
                ws.Cell(row, ci[CM.ProductionRatio])
                  .SetValue(productionRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, ci[CM.AverageReplacementTime])
                  .SetValue(filteredParts.AverageReplacementTimeRatio())
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                ws.Cell(row, ci[CM.CreateNcProgramTime])
                  .SetValue(filteredParts.Sum(p => p.CreateNcProgramTime));

                ws.Cell(row, ci[CM.MaintenanceTime])
                  .SetValue(filteredParts.Sum(p => p.MaintenanceTime));

                ws.Cell(row, ci[CM.ToolSearchingTime])
                  .SetValue(filteredParts.Sum(p => p.ToolSearchingTime));

                ws.Cell(row, ci[CM.ToolChangingTime])
                  .SetValue(filteredParts.Sum(p => p.ToolChangingTime));

                ws.Cell(row, ci[CM.MentoringTime])
                  .SetValue(filteredParts.Sum(p => p.MentoringTime));

                ws.Cell(row, ci[CM.ContactingDepartmentsTime])
                  .SetValue(filteredParts.Sum(p => p.ContactingDepartmentsTime));

                ws.Cell(row, ci[CM.FixtureMakingTime])
                  .SetValue(filteredParts.Sum(p => p.FixtureMakingTime));

                ws.Cell(row, ci[CM.HardwareFailureTime])
                  .SetValue(filteredParts.Sum(p => p.HardwareFailureTime));

                ws.Cell(row, ci[CM.SpecifiedDowntimes])
                  .SetValue(filteredParts.SpecifiedDowntimesRatio(ShiftType.All))
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var specDowntimesEx = filteredParts.SpecifiedDowntimesRatioExcluding(new Downtime[] { Downtime.HardwareFailure, Downtime.Mentoring });
                ws.Cell(row, ci[CM.SpecifiedDowntimesEx])
                  .SetValue(specDowntimesEx)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var generalRatio = (averageSetupRatio != 0 && productionRatio != 0)
                    ? (averageSetupRatio + productionRatio) / 2
                    : (averageSetupRatio != 0 ? averageSetupRatio : productionRatio);
                if ((qualificationNumber == 1 || qualification?.ToLowerInvariant() == "ученик") && productionRatio != 0) generalRatio = productionRatio;

                ws.Cell(row, ci[CM.GeneralRatio])
                  .SetValue(generalRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, ci[CM.SetupsCount])
                  .SetValue(filteredParts.Count(p => p.SetupRatio is not (0 or double.NaN or double.NegativeInfinity or double.PositiveInfinity)));

                ws.Cell(row, ci[CM.ProductionsCount])
                  .SetValue(filteredParts.Count(p => p.ProductionRatio != 0));

                var workedShifts = parts.Where(p => p.Operator == partGroup.Key.Operator && p.Machine == partGroup.Key.Machine)
                                 .Select(p => p.ShiftDate)
                                 .Distinct()
                                 .Count();
                ws.Cell(row, ci[CM.WorkedShifts])
                  .SetValue(workedShifts);

                if (validQualification)
                    ws.Cell(row, ci[CM.Coefficient]).SetValue(Coefficient(qualificationNumber, specDowntimesEx, generalRatio, averageSetupRatio, workedShifts));

                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Column(ci[CM.Qualification]).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(ci[CM.Qualification]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Columns(ci[CM.CreateNcProgramTime], ci[CM.SpecifiedDowntimes]).Group(true);
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)} (изготовление от {minPartsCount}{(maxPartsCount == int.MaxValue ? "" : $" до {maxPartsCount}")} шт.)";
            ws.Range(1, ci[CM.Operator], 1, cm.Count).Merge();
            ws.Range(1, ci[CM.Operator], 1, 1).Style.Font.FontSize = 14;
            ws.Columns(ci[CM.SetupRatio], cm.Count).Width = 7;
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }

        /// <summary>
        /// Экспортирует отчет по сменам операторов в Excel.
        /// </summary>
        /// <param name="parts">Коллекция деталей, содержащих данные для отчета.</param>
        /// <param name="fromDate">Начальная дата периода для отчета.</param>
        /// <param name="toDate">Конечная дата периода для отчета.</param>
        /// <param name="path">Путь для сохранения отчета в виде файла Excel.</param>
        /// <returns>Строка сообщения, указывающая путь и успешность выполнения.</returns>
        /// <remarks>
        /// В отчете выводится количество смен для каждого оператора за указанный период. 
        /// Отчет формируется в виде Excel-файла с заголовками и форматированием.
        /// Если оператором указан "Ученик", то данные по нему не включаются в отчет.
        /// После успешного сохранения файла пользователю предлагается открыть его.
        /// </remarks>
        public static string ExportOperatorsShiftsReport(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет по операторам");
            var columns = new Dictionary<string, (int index, string header)>
            {
                { "operator", (1, "Оператор") },
                { "shifts", (2, "Смены") },
            };
            var lastCol = columns.Count - 1;
            ConfigureWorksheetHeader(ws, columns, HeaderRotateOption.Horizontal);

            var row = 3;

            foreach (var partGroup in parts
                .Where(p => p.Operator != "Ученик")
                .GroupBy(p => p.Operator)
                .OrderBy(pg => pg.Key))
            {
                var distinctShiftsCount = partGroup.Select(pg => pg.ShiftDate).Distinct().Count();

                ws.Cell(row, columns["operator"].index)
                  .SetValue(partGroup.Key);

                ws.Cell(row, columns["shifts"].index)
                  .SetValue(distinctShiftsCount)
                  .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;
            }
            var usedRange = ws.RangeUsed();
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            usedRange.SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }

            return $"Файл сохранен в \"{path}\"";
        }

        public static string ExportHistory(ICollection<Part> parts, string path, int ordersCount)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet($"История последних {ordersCount} заказов");

            ws.Style.Font.FontSize = 8;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var cm = new CM.Builder()
                .Add(CM.Machine)
                .Add(CM.Date)
                .Add(CM.Operator)
                .Add(CM.Part)
                .Add(CM.Finished)
                .Add(CM.Setup)
                .Add(CM.MachiningTime)
                .Add(CM.OperatorComment)
                .Add(CM.Problems)
                .Add(CM.MasterComment)
                .Build();

            ConfigureWorksheetHeader(ws, cm, HeaderRotateOption.Vertical, 65, 8);

            ws.Range(2, 1, 2, cm.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            int row = 3;
            var lastOrders = parts
                .OrderBy(p => p.StartSetupTime)
                .Select(p => p.Order)
                .Distinct()
                .TakeLast(ordersCount)
                .ToList();

            var filteredParts = parts
                .Where(p => lastOrders.Contains(p.Order) && p.FinishedCount > 0)
                .OrderBy(p => p.Order)
                .ThenBy(p => p.StartSetupTime)
                .ToList();

            var ci = cm.GetIndexes();

            foreach (var order in lastOrders)
            {
                ws.Cell(row, 1).SetValue($"{order}");
                ws.Range(row, 1, row, cm.Count).Merge().Style.Font
                    .SetBold(true)
                    .Border.SetLeftBorder(XLBorderStyleValues.Thin)
                    .Border.SetRightBorder(XLBorderStyleValues.Thin)
                    .Border.SetTopBorder(XLBorderStyleValues.Medium)
                    .Border.SetBottomBorder(XLBorderStyleValues.Medium);
                row++;

                foreach (var part in filteredParts.Where(p => p.Order == order))
                {
                    ws.Cell(row, ci[CM.Machine]).SetValue(part.Machine);
                    ws.Cell(row, ci[CM.Date]).SetValue(part.ShiftDate);
                    ws.Cell(row, ci[CM.Operator]).SetValue(part.Operator);
                    ws.Cell(row, ci[CM.Part]).SetValue(part.PartName);
                    ws.Cell(row, ci[CM.Finished]).SetValue(part.FinishedCount);
                    ws.Cell(row, ci[CM.Setup]).SetValue(part.Setup);

                    if (part.MachiningTime != TimeSpan.Zero)
                        ws.Cell(row, ci[CM.MachiningTime]).SetValue(part.MachiningTime);

                    var comment = part.OperatorComment;
                    if (comment.Contains("Отмеченные простои:\n"))
                    {
                        comment = comment.Split("Отмеченные простои:\n")[0].Trim();
                    }

                    ws.Cell(row, ci[CM.OperatorComment]).SetValue(comment)
                        .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    var cells = ws.Range(row, 1, row, cm.Count).Style
                        .Border.SetLeftBorder(XLBorderStyleValues.Medium)
                        .Border.SetRightBorder(XLBorderStyleValues.Medium)
                        .Border.SetInsideBorder(XLBorderStyleValues.Thin);
                    row++;
                }

                //ws.Range(row - filteredParts.Count(p => p.Order == order) - 1, 1, row - 1, cm.Count)
                //    .Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            }

            ws.Columns().AdjustToContents();

            ws.Column(ci[CM.Machine]).Width = 13;
            ws.Column(ci[CM.Date]).Width = 8;
            ws.Column(ci[CM.Operator]).Width = 13;
            ws.Column(ci[CM.Part]).Width = 15;
            ws.Columns(ci[CM.Finished], ci[CM.Setup]).Width = 3;
            ws.Column(ci[CM.MachiningTime]).Width = 7;
            ws.Columns(ci[CM.OperatorComment], ci[CM.Problems]).Width = 30;
            ws.Column(ci[CM.MasterComment]).Width = 20;

            ws.PageSetup.PrintAreas.Add(1, 1, row - 1, cm.Count);
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.FitToPages(1, 0);
            ws.PageSetup.Margins.SetLeft(0.3);
            ws.PageSetup.Margins.SetRight(0.2);
            ws.PageSetup.Margins.SetTop(0.4);
            ws.PageSetup.Margins.SetBottom(0.2);
            ws.Range(2, 1, 2, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row - 1, 1, row - 1, cm.Count).Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            SetTitle(ws, ci.Count, "История изготовления", parts.Last().PartName, 14, BoldOption.Right);
            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }

            return path;
        }

        public static async Task<string> ExportToolSearchCasesAsync(ICollection<Part> parts, string path, IProgress<string> progress)
        {
            progress.Report("Создание книги");
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet($"Поиски истины");
            progress.Report("Получение данных из БД");
            var cases = await Database.GetToolSearchCasesAsync(parts.Select(p => p.Guid).ToList(), progress);

            
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            progress.Report("Формирование столбцов");

            var cm = new CM.Builder()
                .Add(CM.Operator)
                .Add(CM.Type)
                .Add(CM.Description)
                .Add(CM.StartMachiningTime, "Начало")
                .Add(CM.EndMachiningTime, "Завершение")
                .Add(CM.ToolSearchingTime, "Затраченное время")
                .Add(CM.Part)
                .Add(CM.Machine)
                .Build();

            ConfigureWorksheetHeader(ws, cm, HeaderRotateOption.Horizontal, 65, 10);

            ws.Range(2, 1, 2, cm.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            int row = 3;
            
            var ci = cm.GetIndexes();


            foreach (var @case in cases)
            {
                ws.Cell(row, ci[CM.Operator]).SetValue(@case.Operator);
                ws.Cell(row, ci[CM.Part]).SetValue(@case.Part);
                ws.Cell(row, ci[CM.Machine]).SetValue(@case.Machine);
                ws.Cell(row, ci[CM.Type]).SetValue(@case.Type);
                ws.Cell(row, ci[CM.Description]).SetValue(@case.Description);
                ws.Cell(row, ci[CM.StartMachiningTime]).SetValue(@case.StartTime)
                    .Style.DateFormat.Format = "dd.MM.yy HH:mm";
                ws.Cell(row, ci[CM.EndMachiningTime]).SetValue(@case.EndTime)
                    .Style.DateFormat.Format = "dd.MM.yy HH:mm";
                ws.Cell(row, ci[CM.ToolSearchingTime]).SetValue(@case.Time);
            
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Column(ci[CM.Type]).Width = 25;
            ws.Column(ci[CM.Description]).Width = 60;
            ws.Column(ci[CM.Part]).Width = 60;
            ws.Column(ci[CM.StartMachiningTime]).Width = 12;
            ws.Column(ci[CM.EndMachiningTime]).Width = 12;
            ws.Column(ci[CM.ToolSearchingTime]).Width = 12;
            ws.Range(2, ci[CM.StartMachiningTime], row, ci[CM.ToolSearchingTime]).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            SetTitle(ws, ci.Count, "Поиски инструмента");
            progress.Report("Сохранение файла");
            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }

            return path;
        }

        public static string ExportNormsAndWorkloadAnalysis(ICollection<Part> parts, List<DateTime> dates, string path, IProgress<string> progress)
        {
            var wb = new XLWorkbook();
            var wsNorms = wb.AddWorksheet("Нормативы");
            wsNorms.Style.Alignment.WrapText = true;
            progress.Report("Формирование листа \"Нормативы\"");

            var builder = new CM.Builder().Add(CM.Part);

            var setupColumns = new List<string>();
            var workloadColumns = new List<string>();
            dates.Sort();
            foreach (var date in dates)
            {
                string setupColumn = $"Наладка{Environment.NewLine}на {date.ToString(Constants.ShortDateFormat)}";
                string workloadColumn = $"Изготовление{Environment.NewLine}на {date.ToString(Constants.ShortDateFormat)}";

                builder.Add(setupColumn, setupColumn)
                       .Add(workloadColumn, workloadColumn);

                setupColumns.Add(setupColumn);
                workloadColumns.Add(workloadColumn);
            }

            var cm = builder.Build();

            ConfigureWorksheetHeader(wsNorms, cm, HeaderRotateOption.Horizontal, 65, 10);
            int row = 3;
            var ci = cm.GetIndexes();

            var partsDict = parts
                .Select(p => new {
                    Part = p,
                    NormalizedName = NormalizePartName(p.PartName)
                })
                .GroupBy(x => x.NormalizedName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Part).ToList()
                );

            var totalUnique = partsDict
                .Where(kvp => kvp.Value
                    .Where(p => p.TotalCount >= 50)
                    .Select(p => p.Order)
                    .Distinct()
                    .Take(3)
                    .Count() == 3)
                .Select(kvp => kvp.Value.First())
                .ToList();

            foreach (var part in totalUnique)
            {
                var normalizedName = NormalizePartName(part.PartName);
                var partsForName = partsDict[normalizedName];
                var machineGroups = partsForName.GroupBy(p => p.Machine).ToDictionary(g => g.Key, g => g.ToList());

                wsNorms.Cell(row, ci[CM.Part]).Value = part.PartName;

                for (int i = 0; i < dates.Count; i++)
                {
                    var date = dates[i];
                    var setupColumn = setupColumns[i];
                    var workloadColumn = workloadColumns[i];

                    double setupSum = 0;
                    double workloadSum = 0;

                    foreach (var machineGroup in machineGroups)
                    {
                        var machine = machineGroup.Key;
                        var machineParts = machineGroup.Value;

                        foreach (var order in machineParts.Select(mg => mg.Order))
                        {
                            foreach (var p in machineParts.Where(p => p.Order == order))
                            {
                                if (p.SetupTimePlan != 0) setupSum = p.SetupTimePlanForCalc;
                                if (p.SingleProductionTimePlan != 0) workloadSum = p.SingleProductionTimePlan;
                            }
                        }

                        var dateParts = machineParts.Where(p => p.EndMachiningTime <= date).ToList();

                        double setup = 0;
                        if (dateParts.Any())
                        {
                            var lastNonZero = dateParts.LastOrDefault(p => p.SetupTimePlan != 0);
                            setup = lastNonZero != null ? lastNonZero.SetupTimePlan : dateParts.Last().SetupTimePlan;
                        }

                        double workload = 0;
                        if (dateParts.Any())
                        {
                            var lastNonZero = dateParts.LastOrDefault(p => p.SingleProductionTimePlan != 0);
                            workload = lastNonZero != null ? lastNonZero.SingleProductionTimePlan : dateParts.Last().SingleProductionTimePlan;
                        }

                        setupSum += setup;
                        workloadSum += workload;
                    }

                    wsNorms.Cell(row, ci[setupColumn]).Value = setupSum;
                    wsNorms.Cell(row, ci[workloadColumn]).Value = workloadSum;
                }

                row++;
            }

            for (int i = 1; i < setupColumns.Count; i++)
            {
                var currentSetupColumn = setupColumns[i];
                var previousSetupColumn = setupColumns[i - 1];

                var setupRange = wsNorms.Range(3, ci[currentSetupColumn], row - 1, ci[currentSetupColumn]);
                var setupFormattingGreen = setupRange.AddConditionalFormat();
                setupFormattingGreen.WhenIsTrue($"{setupRange.FirstCell().Address}<{wsNorms.Cell(3, ci[previousSetupColumn]).Address}").Fill.BackgroundColor = _lightGreen;

                var setupFormattingRed = setupRange.AddConditionalFormat();
                setupFormattingRed.WhenIsTrue($"{setupRange.FirstCell().Address}>{wsNorms.Cell(3, ci[previousSetupColumn]).Address}").Fill.BackgroundColor = _lightRed;
            }

            for (int i = 1; i < workloadColumns.Count; i++)
            {
                var currentWorkloadColumn = workloadColumns[i];
                var previousWorkloadColumn = workloadColumns[i - 1];

                var workloadRange = wsNorms.Range(3, ci[currentWorkloadColumn], row - 1, ci[currentWorkloadColumn]);

                var workloadFormattingGreen = workloadRange.AddConditionalFormat();
                workloadFormattingGreen.WhenIsTrue($"{workloadRange.FirstCell().Address}<{wsNorms.Cell(3, ci[previousWorkloadColumn]).Address}").Fill.BackgroundColor = _lightGreen;

                var workloadFormattingRed = workloadRange.AddConditionalFormat();
                workloadFormattingRed.WhenIsTrue($"{workloadRange.FirstCell().Address}>{wsNorms.Cell(3, ci[previousWorkloadColumn]).Address}").Fill.BackgroundColor = _lightRed;
            }

            wsNorms.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            wsNorms.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            wsNorms.Columns().AdjustToContents();

            foreach (var setupColumn in setupColumns)
            {
                wsNorms.Column(ci[setupColumn]).Width = 14;
            }

            foreach (var workloadColumn in workloadColumns)
            {
                wsNorms.Column(ci[workloadColumn]).Width = 14;
            }

            wsNorms.RangeUsed().SetAutoFilter(true);
            SetTitle(wsNorms, ci.Count, "Изменение нормативов");

            //var wsFact = wb.AddWorksheet("Факт");
            //wsFact.Style.Alignment.WrapText = true;
            //progress.Report("Формирование листа \"Факт\"");

            //builder = new CM.Builder().Add(CM.Part);

            //setupColumns = new List<string>();
            //workloadColumns = new List<string>();

            //foreach (var date in dates)
            //{
            //    string setupColumn = $"Наладка{Environment.NewLine}на {date.ToString(Constants.ShortDateFormat)}";
            //    string workloadColumn = $"Изготовление{Environment.NewLine}на {date.ToString(Constants.ShortDateFormat)}";

            //    builder.Add(setupColumn, setupColumn)
            //           .Add(workloadColumn, workloadColumn);

            //    setupColumns.Add(setupColumn);
            //    workloadColumns.Add(workloadColumn);
            //}

            //cm = builder.Build();

            //ConfigureWorksheetHeader(wsFact, cm, HeaderRotateOption.Horizontal, 65, 10);
            //row = 3;
            //ci = cm.GetIndexes();

            //foreach (var partGrop in parts.GroupBy(p => p.PartName))
            //{
            //    var col = ci[CM.Part] + 1;
            //    wsFact.Cell(row, ci[CM.Part]).Value = partGrop.Key;

            //    foreach (var oreder in partGrop.Select(pg => pg.Order))
            //    {

            //    }

            //    for (int i = 0; i < dates.Count; i++)
            //    {
            //        var date = dates[i];
            //        var setupColumn = setupColumns[i];
            //        var workloadColumn = workloadColumns[i];

            //        double setupSum = 0;
            //        double workloadSum = 0;

            //        foreach (var machineGroup in machineGroups)
            //        {
            //            var machine = machineGroup.Key;
            //            var machineParts = machineGroup.Value;

            //            var dateParts = machineParts.Where(p => p.EndMachiningTime <= date).ToList();

            //            double setup = 0;
            //            if (dateParts.Any())
            //            {
            //                var lastNonZero = dateParts.LastOrDefault(p => p.SetupTimePlan != 0);
            //                setup = lastNonZero != null ? lastNonZero.SetupTimePlan : dateParts.Last().SetupTimePlan;
            //            }

            //            double workload = 0;
            //            if (dateParts.Any())
            //            {
            //                var lastNonZero = dateParts.LastOrDefault(p => p.SingleProductionTimePlan != 0);
            //                workload = lastNonZero != null ? lastNonZero.SingleProductionTimePlan : dateParts.Last().SingleProductionTimePlan;
            //            }

            //            setupSum += setup;
            //            workloadSum += workload;
            //        }

            //        wsFact.Cell(row, ci[setupColumn]).Value = setupSum;
            //        wsFact.Cell(row, ci[workloadColumn]).Value = workloadSum;
            //    }

            //    row++;
            //}

            //for (int i = 1; i < setupColumns.Count; i++)
            //{
            //    var currentSetupColumn = setupColumns[i];
            //    var previousSetupColumn = setupColumns[i - 1];

            //    var setupRange = wsFact.Range(3, ci[currentSetupColumn], row - 1, ci[currentSetupColumn]);
            //    var setupFormattingGreen = setupRange.AddConditionalFormat();
            //    setupFormattingGreen.WhenIsTrue($"{setupRange.FirstCell().Address}<{wsFact.Cell(3, ci[previousSetupColumn]).Address}").Fill.BackgroundColor = _lightGreen;

            //    var setupFormattingRed = setupRange.AddConditionalFormat();
            //    setupFormattingRed.WhenIsTrue($"{setupRange.FirstCell().Address}>{wsFact.Cell(3, ci[previousSetupColumn]).Address}").Fill.BackgroundColor = _lightRed;
            //}

            //for (int i = 1; i < workloadColumns.Count; i++)
            //{
            //    var currentWorkloadColumn = workloadColumns[i];
            //    var previousWorkloadColumn = workloadColumns[i - 1];

            //    var workloadRange = wsFact.Range(3, ci[currentWorkloadColumn], row - 1, ci[currentWorkloadColumn]);

            //    var workloadFormattingGreen = workloadRange.AddConditionalFormat();
            //    workloadFormattingGreen.WhenIsTrue($"{workloadRange.FirstCell().Address}<{wsFact.Cell(3, ci[previousWorkloadColumn]).Address}").Fill.BackgroundColor = _lightGreen;

            //    var workloadFormattingRed = workloadRange.AddConditionalFormat();
            //    workloadFormattingRed.WhenIsTrue($"{workloadRange.FirstCell().Address}>{wsFact.Cell(3, ci[previousWorkloadColumn]).Address}").Fill.BackgroundColor = _lightRed;
            //}

            //wsFact.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            //wsFact.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            //wsFact.Columns().AdjustToContents();

            //foreach (var setupColumn in setupColumns)
            //{
            //    wsFact.Column(ci[setupColumn]).Width = 14;
            //}

            //foreach (var workloadColumn in workloadColumns)
            //{
            //    wsFact.Column(ci[workloadColumn]).Width = 14;
            //}

            //wsFact.RangeUsed().SetAutoFilter(true);
            //SetTitle(wsFact, ci.Count, "Изменение фактической трудоемкости");

            progress.Report("Сохранение файла");
            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }

            return path;
        }

        public static string ExportAssignmentCheckResult(IEnumerable<Part> factParts, Dictionary<string, string> assignmentParts, string path, IProgress<string> progress)
        {
            var wb = new XLWorkbook();
            var wsAll = wb.AddWorksheet($"Все");
            wsAll.Style.Alignment.WrapText = true;

            var cm = new CM.Builder()
                .Add(CM.Date)
                .Add(CM.Operator)
                .Add(CM.Part)
                .Add(CM.Setup)
                .Add(CM.Machine)
                .Add(CM.MachineAssigned)
                .Add(CM.IsEqual)
                .Build();

            ConfigureWorksheetHeader(wsAll, cm, HeaderRotateOption.Vertical, 65, 8);

            wsAll.Range(2, 1, 2, cm.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            int row = 3;
            var ci = cm.GetIndexes();

            foreach (var part in factParts)
            {
                progress.Report($"Все: Проверка {part.PartName}");
                var isAssigned = assignmentParts.ContainsKey(part.PartName.ToLowerInvariant().Trim());
                wsAll.Cell(row, ci[CM.Date]).Value = part.ShiftDate;
                wsAll.Cell(row, ci[CM.Operator]).Value = part.Operator;
                wsAll.Cell(row, ci[CM.Part]).Value = part.PartName;
                wsAll.Cell(row, ci[CM.Setup]).Value = part.Setup;
                wsAll.Cell(row, ci[CM.Machine]).Value = part.Machine;
                var assignedMachine = isAssigned
                    ? assignmentParts
                        .Where(ap => ap.Key.ToLowerInvariant().Trim() == part.PartName.ToLowerInvariant().Trim())
                        .First()
                        .Value
                    : "-";
                wsAll.Cell(row, ci[CM.MachineAssigned]).Value = assignedMachine;
                wsAll.Cell(row, ci[CM.IsEqual]).Value = !isAssigned ? "-" : part.Machine.Contains(assignedMachine);
                row++;
            }

            progress.Report("Все: Настройка листа");
            wsAll.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            wsAll.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            wsAll.RangeUsed().SetAutoFilter(true);
            SetTitle(wsAll, ci.Count, "Проверка на изготовление в соответствии со СЗН");
            wsAll.Columns().AdjustToContents();

            var wsUnique = wb.AddWorksheet("Уникальные");
            wsUnique.Style.Alignment.WrapText = true;

            cm = new CM.Builder()
                .Add(CM.Date)
                .Add(CM.Part)
                .Add(CM.Order)
                .Add(CM.Machine)
                .Add(CM.MachineAssigned)
                .Add(CM.IsEqual)
                .Build();
            ConfigureWorksheetHeader(wsUnique, cm, HeaderRotateOption.Vertical, 65, 8);
            wsUnique.Range(2, 1, 2, cm.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            row = 3;
            ci = cm.GetIndexes();

            foreach (var part in factParts.GroupBy(p => (p.PartName, p.Order, p.Machine)).Select(g => g.First()))
            {
                progress.Report($"Уникальные: Проверка {part.PartName}");
                var isAssigned = assignmentParts.ContainsKey(part.PartName.ToLowerInvariant().Trim());
                wsUnique.Cell(row, ci[CM.Date]).Value = part.ShiftDate;
                wsUnique.Cell(row, ci[CM.Part]).Value = part.PartName;
                wsUnique.Cell(row, ci[CM.Order]).Value = part.Order;
                wsUnique.Cell(row, ci[CM.Machine]).Value = part.Machine;
                var assignedMachine = isAssigned
                    ? assignmentParts
                        .Where(ap => ap.Key.ToLowerInvariant().Trim() == part.PartName.ToLowerInvariant().Trim())
                        .First()
                        .Value
                    : "-";
                wsUnique.Cell(row, ci[CM.MachineAssigned]).Value = assignedMachine;
                wsUnique.Cell(row, ci[CM.IsEqual]).Value = !isAssigned ? "-" : part.Machine.Contains(assignedMachine);
                row++;
            }
            progress.Report("Уникальные: Настройка листа");
            wsUnique.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            wsUnique.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            wsUnique.RangeUsed().SetAutoFilter(true);
            SetTitle(wsUnique, ci.Count, "Проверка на изготовление в соответствии со СЗН");
            wsUnique.Columns().AdjustToContents();

            var wsFromList = wb.AddWorksheet("По списку");
            wsFromList.Style.Alignment.WrapText = true;

            cm = new CM.Builder()
                .Add(CM.Part)
                .Add(CM.MachineAssigned)
                .Add(CM.Order, "Заказы")
                .Add(CM.Machine, "Станки")
                .Add(CM.IsEqual, "Делали")
                .Add(CM.CountPerMachine, "Количество запусков")
                .Build();
            ConfigureWorksheetHeader(wsFromList, cm, HeaderRotateOption.Vertical, 65, 8);
            wsFromList.Range(2, 1, 2, cm.Count).Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);

            row = 3;
            ci = cm.GetIndexes();

            foreach (var partName in assignmentParts.Keys.Skip(1))
            {
                progress.Report($"По списку: Проверка {partName}");
                var isProcessed = factParts.Select(p => p.PartName.ToLowerInvariant().Trim()).Contains(partName);
                wsFromList.Cell(row, ci[CM.Part]).Value = partName;
                var orders = factParts.Where(p => p.PartName.ToLowerInvariant().Trim() == partName).Select(pn => pn.Order).Distinct();
                wsFromList.Cell(row, ci[CM.Order]).Value = string.Join(" ", orders);
                var machines = factParts.Where(p => p.PartName.ToLowerInvariant().Trim() == partName).Select(pn => pn.Machine).Distinct();
                wsFromList.Cell(row, ci[CM.Machine]).Value = string.Join(" ", machines);
                wsFromList.Cell(row, ci[CM.MachineAssigned]).Value = assignmentParts[partName];
                var status = "";
                if (isProcessed && machines.Count() == 1 && string.Join(" ", machines).Contains(assignmentParts[partName]))
                {
                    status = "Делали как надо";
                }
                else if (isProcessed && machines.Count() > 1 && string.Join(" ", machines).Contains(assignmentParts[partName]))
                {
                    status = "Делали и где надо и где не надо";
                }
                else if (isProcessed)
                {
                    status = "Делали где попало, но не где надо";
                }
                else
                {
                    status = "Не делали";
                }
                wsFromList.Cell(row, ci[CM.IsEqual]).Value = status;
                var count = factParts.Where(p => p.PartName.ToLowerInvariant().Trim() == partName).Select(p => (p.Order, p.Machine)).Distinct().Count();
                wsFromList.Cell(row, ci[CM.CountPerMachine]).Value = count;
                row++;
            }
            progress.Report("Уникальные: Настройка листа");
            wsFromList.Range(2, 1, row - 1, ci.Count).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            wsFromList.Range(2, 1, row - 1, cm.Count).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            wsFromList.RangeUsed().SetAutoFilter(true);
            SetTitle(wsFromList, ci.Count, "Проверка на изготовление в соответствии со СЗН");
            wsFromList.Columns().AdjustToContents();



            progress.Report("Сохранение файла");
            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = path });
            }

            return path;
        }

        /// <summary>
        /// Экспортирует историю последних заказов в Excel-файл.
        ///
        /// Метод создает Excel-файл с историей заказов, включая информацию о станках, операторах,
        /// времени работы, установках, комментариях и других данных. Данные фильтруются по количеству последних заказов.
        /// Форматирует таблицу, добавляет стили и сохраняет файл по указанному пути.
        ///
        /// </summary>
        /// <param name="parts">
        /// Коллекция деталей (Part), содержащая данные для экспорта.
        /// </param>
        /// <param name="path">
        /// Путь, по которому будет сохранен созданный Excel-файл.
        /// </param>
        /// <param name="ordersCount">
        /// Количество последних заказов, которые необходимо включить в отчет.
        /// </param>
        /// <returns>
        /// Полный путь к сохраненному файлу.
        /// </returns>
        /// <remarks>
        /// Формат выходного файла:
        /// - Включает следующие столбцы: "Станок", "Дата", "Оператор", "М/Л", "Выполнено", "Установка",
        ///   "Машинное время", "Комментарий оператора", "Типовые проблемы", "Комментарий мастера".
        /// - Данные упорядочены сначала по станку, затем по времени установки.
        /// - Добавляются стили ячеек, границы, автофильтры, а также настройки страницы (ориентация, размеры полей и т. д.).
        ///
        /// Если файл успешно сохранен, пользователю предлагается открыть его.
        /// </remarks>
        /// <example>
        /// Пример вызова метода:
        /// <code>
        /// var parts = GetParts();
        /// string filePath = "C:\\Reports\\History.xlsx";
        /// ExportHistory(parts, filePath, 10);
        /// </code>
        /// </example>
        public static string ExportPartsInfo(ICollection<Part> parts, string path, DateTime fromDate, DateTime toDate)
        {
            var wb = new XLWorkbook();
            var wsTotal = wb.AddWorksheet("Общий");
            ConfigureWorksheetStyles(wsTotal);

            var machines = parts.Where(p => !p.ExcludeFromReports).Select(p => p.Machine).Distinct().ToArray();

            foreach (var machine in machines)
            {
                wsTotal.CopyTo(machine);
            }

            var columns = new Dictionary<string, (int index, string header)>
            {
                {"machine", (1, "Станок") },
                {"part", (2, "Деталь") },
                {"ordersCnt", (3, $"Количество М/Л{Environment.NewLine}(по станку)") },
                {"partsCnt", (4, $"Количество деталей{Environment.NewLine}(по станку)") },
                {"planSum", (5, $"Сумма нормативов{Environment.NewLine}(по станку)") },
                {"factSum", (6, $"Время фактическое{Environment.NewLine}(по станку)") },
                {"ordersCntTotal", (7, $"Количество М/Л{Environment.NewLine}(общее)") },
                {"partsCntTotal", (8, $"Количество деталей{Environment.NewLine}(общее)") },
                {"planSumTotal", (9, $"Сумма нормативов{Environment.NewLine}(общее)") },
                {"factSumTotal", (10, $"Время фактическое{Environment.NewLine}(общее)") },
            };

            ConfigureWorksheetHeader(wsTotal, columns);

            var tempParts = parts.Where(p => !p.ExcludeFromReports).Select(part => new Part(part) { PartName = part.PartName.ToUpper().Trim('"') }).ToList();

            var machinesGroup = tempParts.GroupBy(p => p.Machine);

            FillTotalMachinesWorksheetData(wsTotal, machinesGroup, columns, tempParts);

            foreach (var ws in wb.Worksheets.Skip(1))
            {
                ConfigureWorksheetHeader(ws, columns);
                FillMachineWorksheetData(ws, tempParts.Where(p => p.Machine == ws.Name), columns);
            }

            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
        }

        public static string ExportShiftsInfo(ICollection<Part> parts, string path, DateTime fromDate, DateTime toDate)
        {
            var wb = new XLWorkbook();
            var wsTotal = wb.AddWorksheet("Общий");
            ConfigureWorksheetStyles(wsTotal);

            var machines = parts.Where(p => !p.ExcludeFromReports).Select(p => p.Machine).Distinct().ToArray();

            //foreach (var machine in machines)
            //{
            //    wsTotal.CopyTo(machine);
            //}

            var columns = new Dictionary<string, (int index, string header)>
            {
                {"date", (1, "Дата") },
                {"type", (2, "Смена") },
                {"machine", (3, "Станок") },
                {"master", (4, "Мастер") },
                {"ordersCnt", (5, $"Количество М/Л") },
                {"partsCnt", (6, $"Количество деталей") },
                {"planSum", (7, $"Сумма нормативов") },
                {"factSum", (8, $"Время фактическое") },
                {"specDowntimes", (9, $"Отмеченные простои") },
                {"unspecDowntimes", (10, $"Неотмеченные простои") },
                {"comment", (11, $"Комментарий") },
                {"isChecked", (12, $"Проверено СГТ") },
            };

            ConfigureWorksheetHeader(wsTotal, columns);

            switch (Database.GetShiftsByPeriod(machines, fromDate, toDate, new Shift(ShiftType.All), out var shifts))
            {
                case libeLog.Models.DbResult.AuthError:
                    MessageBox.Show("AuthError");
                    return "";
                case libeLog.Models.DbResult.Error:
                    MessageBox.Show("Error");
                    return "";
                case libeLog.Models.DbResult.NoConnection:
                    MessageBox.Show("NoConnection");
                    return "";
            }
            shifts = shifts.OrderBy(s => s.Machine).ToList();
            int row = 3;
            for (DateTime dt = fromDate; dt <= toDate; dt += TimeSpan.FromDays(1))
            {
                foreach (var machine in machines)
                {
                    var dayShiftInfo = shifts.Find(s => s.ShiftDate == dt && s.Machine == machine && s.Shift == "День");
                    var dayParts = parts.Where(p => p.ShiftDate == dt && p.Machine == machine && p.Shift == "День");
                    var nightShiftInfo = shifts.Find(s => s.ShiftDate == dt && s.Machine == machine && s.Shift == "Ночь");
                    var nightParts = parts.Where(p => p.ShiftDate == dt && p.Machine == machine && p.Shift == "Ночь");
                    wsTotal.Cell(row, columns["date"].index).Value = dt;
                    wsTotal.Range(row, columns["date"].index, row + 1, columns["date"].index).Merge();

                    wsTotal.Cell(row, columns["type"].index).Value = "День";
                    wsTotal.Cell(row + 1, columns["type"].index).Value = "Ночь";

                    wsTotal.Cell(row, columns["machine"].index).Value = machine;
                    wsTotal.Range(row, columns["machine"].index, row + 1, columns["machine"].index).Merge();

                    wsTotal.Cell(row, columns["master"].index).Value = dayShiftInfo != null ? dayShiftInfo.Master : "Н/Д";
                    wsTotal.Range(row, columns["master"].index, row + 1, columns["master"].index).Merge();

                    wsTotal.Cell(row, columns["ordersCnt"].index).Value = dayParts
                        .Select(p => p.Order)
                        .Distinct()
                        .Count();
                    wsTotal.Cell(row + 1, columns["ordersCnt"].index).Value = nightParts
                        .Select(p => p.Order)
                        .Distinct()
                        .Count();

                    row += 2;
                }
            }

            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
        }

        public static string ExportVerevkinReport(IEnumerable<VerevkinPart> parts, string path, IProgress<string> progress)
        {
            var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();

            var columns = new Dictionary<string, int>
            {
                {"no", 1 },
                {"part", 2 },
                {"fact1", 4 },
                {"fact2", 5 },
                {"fact3", 6 },
                {"fact4", 7 },
                {"fact5", 8 },
                {"factSum", 9 },
                {"factTurn1prev", 10 },
                {"factTurn2prev", 11 },
                {"factTurn3prev", 12 },
                {"factSumPrev", 13 },
                {"factMill1prev", 14 },
                {"factMill2prev", 15 },
                {"factMill3prev", 16 },
                {"factMill4prev", 17 },
                {"factSumMillPrev", 18 },
                {"factSum2", 19 },
                {"factSumPrev2", 20 },
                {"effectTime", 21 },
                {"effectPercent", 22 },
                {"order", 23 },
                {"count", 24 },
            };
            progress.Report("Чтение содержимого");

            int rowIndex = 0;

            foreach (var row in ws.Rows().Skip(2))
            {
                rowIndex++;
                if (!row.Cell(2).IsEmpty() || !row.Cell(2).Value.IsBlank) continue;
                break;
            }
            rowIndex += 2;
            progress.Report("Добавление деталей");
            foreach (var part in parts)
            {
                ws.Row(rowIndex).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(rowIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(rowIndex, columns["no"]).SetValue(rowIndex - 2);
                ws.Cell(rowIndex, columns["part"]).SetValue(part.PartName).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Range(rowIndex, columns["part"], rowIndex, columns["part"] + 1).Merge();
                if (part.MachineTime1 != TimeSpan.Zero) ws.Cell(rowIndex, columns[$"fact1"]).SetValue(part.MachineTime1);
                if (part.MachineTime2 != TimeSpan.Zero) ws.Cell(rowIndex, columns[$"fact2"]).SetValue(part.MachineTime2);
                if (part.MachineTime3 != TimeSpan.Zero) ws.Cell(rowIndex, columns[$"fact3"]).SetValue(part.MachineTime3);
                if (part.MachineTime4 != TimeSpan.Zero) ws.Cell(rowIndex, columns[$"fact4"]).SetValue(part.MachineTime4);
                if (part.MachineTime5 != TimeSpan.Zero) ws.Cell(rowIndex, columns[$"fact5"]).SetValue(part.MachineTime5);
                ws.Cell(rowIndex, columns["factSum"]).SetFormulaA1($"SUM(D{rowIndex}:H{rowIndex})");
                ws.Cell(rowIndex, columns["factSumPrev"]).SetFormulaA1($"SUM(J{rowIndex}:L{rowIndex})");
                ws.Cell(rowIndex, columns["factSumMillPrev"]).SetFormulaA1($"SUM(N{rowIndex}:Q{rowIndex})");
                ws.Cell(rowIndex, columns["factSum2"]).SetFormulaA1($"I{rowIndex}");
                ws.Cell(rowIndex, columns["factSumPrev2"]).SetFormulaA1($"M{rowIndex}+R{rowIndex}");
                ws.Cell(rowIndex, columns["effectTime"]).SetFormulaA1(
                    $"IF(ISNUMBER(T{rowIndex})*AND(T{rowIndex}>0), IF(T{rowIndex} - S{rowIndex} < 0, \"-\" " +
                    $"& TEXT(ABS(T{rowIndex} - S{rowIndex}), \"Ч:ММ:СС\"), TEXT(T{rowIndex} - S{rowIndex}, \"Ч:ММ:СС\")), T{rowIndex})");

                ws.Cell(rowIndex, columns["effectPercent"]).SetFormulaA1($"IFERROR(T{rowIndex}/S{rowIndex},T{rowIndex})")
                    .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Range(rowIndex, columns["fact1"], rowIndex, columns["effectTime"]).Style.NumberFormat.SetFormat("h:mm:ss");
                ws.Cell(rowIndex, columns["order"]).SetValue(part.Order);
                ws.Cell(rowIndex, columns["count"]).SetValue(part.FinishedCount);
                rowIndex++;
            }

            progress.Report("Сохранение");
            wb.SaveAs(path);

            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
        }

        public static string ExportLongSetups(ICollection<Part> parts, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Длительные наладки");
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var limits = parts
                .Select(p => p.Machine)
                .Distinct()
                .ToDictionary(
                machine => machine,
                machine =>
                {
                    var (_, SetupLimit, _) = machine.GetMachineSetupLimit();
                    var (_, SetupCoefficient, _) = machine.GetMachineSetupCoefficient();
                    return (SetupCoefficient, SetupLimit);
                });

            var totalSetups = parts
                .Where(p => p.SetupTimePlanForCalc > 0 || p.PartialSetupTime > 0)
                .GroupBy(p => (p.PartName, p.Order, p.Setup))
                .SelectMany(g => g.Distinct())
                .Count();

            var cm = new CM();
            cm.Add(CM.Machine);
            cm.Add(CM.Date);
            cm.Add(CM.Shift);
            cm.Add(CM.Operator);
            cm.Add(CM.Part);
            cm.Add(CM.Order);
            cm.Add(CM.Finished);
            cm.Add(CM.Setup);
            cm.Add(CM.StartSetupTime);
            cm.Add(CM.StartMachiningTime);
            cm.Add(CM.EndMachiningTime);
            cm.Add(CM.SetupLimit);
            cm.Add(CM.SetupTimePlan);
            cm.Add(CM.SetupTimeFact);
            cm.Add(CM.PartialSetupTime);
            cm.Add(CM.SingleProductionTimePlan);
            cm.Add(CM.MachiningTime);
            cm.Add(CM.SingleProductionTime);
            cm.Add(CM.PartReplacementTime);
            cm.Add(CM.OperatorComment);
            cm.Add(CM.SetupDowntimes);
            cm.Add(CM.MachiningDowntimes);
            cm.Add(CM.MaintenanceTime);
            cm.Add(CM.ToolSearchingTime);
            cm.Add(CM.ToolChangingTime);
            cm.Add(CM.MentoringTime);
            cm.Add(CM.ContactingDepartmentsTime);
            cm.Add(CM.FixtureMakingTime);
            cm.Add(CM.HardwareFailureTime);
            cm.Add(CM.SpecifiedDowntimesRatio);
            cm.Add(CM.SpecifiedDowntimesComment);
            cm.Add(CM.SetupRatioTitle);
            cm.Add(CM.MasterSetupComment);
            cm.Add(CM.MasterComment);
            cm.Add(CM.FixedSetupTimePlan);
            cm.Add(CM.FixedProductionTimePlan);
            cm.Add(CM.EngineerComment);

            ConfigureWorksheetHeader(ws, cm);

            var ci = cm.GetIndexes();
            var row = 3;
            var cnt = 0;
            foreach (var part in parts)
            {
                var (limitValue, limitInfo) = part.SetupLimit(limits[part.Machine].SetupCoefficient, limits[part.Machine].SetupLimit);
                if (limitValue >= part.SetupTimeFact + part.PartialSetupTime) continue;

                ws.Cell(row, ci[CM.Machine]).SetValue(part.Machine);
                ws.Cell(row, ci[CM.Date])
                    .SetValue(part.ShiftDate)
                    .Style.DateFormat.Format = "dd.MM.yy";
                ws.Cell(row, ci[CM.Shift]).SetValue(part.Shift);
                ws.Cell(row, ci[CM.Operator]).SetValue(part.Operator);
                ws.Cell(row, ci[CM.Part]).SetValue(part.PartName);
                ws.Cell(row, ci[CM.Order]).SetValue(part.Order);
                ws.Cell(row, ci[CM.Finished]).SetValue(part.FinishedCount);
                ws.Cell(row, ci[CM.Setup]).SetValue(part.Setup);
                ws.Cell(row, ci[CM.StartSetupTime])
                    .SetValue(part.StartSetupTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.StartMachiningTime])
                    .SetValue(part.StartMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.EndMachiningTime])
                    .SetValue(part.EndMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";
                ws.Cell(row, ci[CM.SetupTimePlan]).SetValue(part.SetupTimePlan);
                ws.Cell(row, ci[CM.SetupTimeFact]).SetValue(part.SetupTimeFact);
                if (part.SetupTimeFact > limitValue) ws.Cell(row, ci[CM.SetupTimeFact]).Style.Fill.BackgroundColor = XLColor.LightYellow;
                ws.Cell(row, ci[CM.SetupLimit]).SetValue(limitValue).CreateComment().SetAuthor("Отчёт").AddText(limitInfo).AddNewLine();
                ws.Cell(row, ci[CM.SingleProductionTimePlan]).SetValue(part.SingleProductionTimePlan);
                ws.Cell(row, ci[CM.MachiningTime]).SetValue(part.MachiningTime);
                if (part.SingleProductionTime is double spt && spt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SingleProductionTime])
                        .SetValue(spt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                if (part.PartReplacementTime is double prt && prt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.PartReplacementTime])
                        .SetValue(prt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, ci[CM.OperatorComment])
                    .SetValue(part.OperatorComment)
                    .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, ci[CM.SetupDowntimes]).SetValue(part.SetupDowntimes);
                ws.Cell(row, ci[CM.MachiningDowntimes]).SetValue(part.MachiningDowntimes);
                ws.Cell(row, ci[CM.PartialSetupTime]).SetValue(part.PartialSetupTime);
                if (part.PartialSetupTime > limitValue) ws.Cell(row, ci[CM.PartialSetupTime]).Style.Fill.BackgroundColor = XLColor.LightYellow;
                ws.Cell(row, ci[CM.MaintenanceTime]).SetValue(part.MaintenanceTime);
                ws.Cell(row, ci[CM.ToolSearchingTime]).SetValue(part.ToolSearchingTime);
                ws.Cell(row, ci[CM.ToolChangingTime]).SetValue(part.ToolChangingTime);
                ws.Cell(row, ci[CM.MentoringTime]).SetValue(part.MentoringTime);
                ws.Cell(row, ci[CM.ContactingDepartmentsTime]).SetValue(part.ContactingDepartmentsTime);
                ws.Cell(row, ci[CM.FixtureMakingTime]).SetValue(part.FixtureMakingTime);
                ws.Cell(row, ci[CM.HardwareFailureTime]).SetValue(part.HardwareFailureTime);
                if (part.SpecifiedDowntimesRatio is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SpecifiedDowntimesRatio])
                        .SetValue(part.SpecifiedDowntimesRatio)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, ci[CM.SpecifiedDowntimesComment]).SetValue(part.SpecifiedDowntimesComment);
                ws.Cell(row, ci[CM.SetupRatioTitle]).SetValue(part.SetupRatioTitle);
                ws.Cell(row, ci[CM.MasterSetupComment]).SetValue(part.MasterSetupComment);
                ws.Cell(row, ci[CM.MasterComment]).SetValue(part.MasterComment);
                ws.Cell(row, ci[CM.FixedSetupTimePlan]).SetValue(part.FixedSetupTimePlan);
                ws.Cell(row, ci[CM.FixedProductionTimePlan]).SetValue(part.FixedProductionTimePlan);
                ws.Cell(row, ci[CM.EngineerComment]).SetValue(part.EngineerComment);
                cnt++;
                row++;
            }

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Range(3, ci[CM.Machine], row, ci[CM.Machine])
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(ci[CM.Operator]).Width = 15;
            ws.Column(ci[CM.Part]).Width = 25;
            ws.Range(3, ci[CM.OperatorComment], row, ci[CM.OperatorComment])
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(ci[CM.OperatorComment]).Width = 35;
            ws.Column(ci[CM.MasterSetupComment]).Width = 20;
            ws.Column(ci[CM.MasterComment]).Width = 20;


            var hiddenColumns = new List<string>
            {
                CM.SingleProductionTimePlan,
                CM.MachiningTime,
                CM.SingleProductionTime,
                CM.PartReplacementTime,
                CM.MachiningDowntimes,
                CM.MaintenanceTime,
                CM.ToolSearchingTime,
                CM.ToolChangingTime,
                CM.MentoringTime,
                CM.ContactingDepartmentsTime,
                CM.FixtureMakingTime,
                CM.HardwareFailureTime,
                CM.SpecifiedDowntimesRatio,
                CM.SpecifiedDowntimesComment,
                CM.FixedProductionTimePlan
            };
            foreach (var col in hiddenColumns)
            {
                ws.Column(ci[col]).Hide();
            }
            SetTitle(ws, cm.Count, $"Отчёт по длительным наладкам: {cnt} из {totalSetups} ({((double)cnt / (double)totalSetups) * 100:N2}%) за период от {parts.Min(p => p.ShiftDate).ToString(Constants.ShortDateFormat)} до {parts.Max(p => p.ShiftDate).ToString(Constants.ShortDateFormat)}");
            ws.SheetView.FreezeRows(2);

            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
        }

        public static string ExportDataset(ICollection<Part> parts, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Экспорт");
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var cm = new CM.Builder()
                .Add(CM.Machine)
                .Add(CM.Date)
                .Add(CM.Shift)
                .Add(CM.Operator)
                .Add(CM.Part)
                .Add(CM.Order)
                .Add(CM.TotalByOrder)
                .Add(CM.Finished)
                .Add(CM.Setup)
                .Add(CM.StartSetupTime)
                .Add(CM.StartMachiningTime)
                .Add(CM.EndMachiningTime)
                .Add(CM.SetupTimePlan)
                .Add(CM.SetupTimeFact)
                .Add(CM.SingleProductionTimePlan)
                .Add(CM.MachiningTime)
                .Add(CM.SingleProductionTime)
                .Add(CM.PartReplacementTime)
                .Add(CM.ProductionTimeFact)
                .Add(CM.PlanForBatch)
                .Add(CM.OperatorComment)
                .Add(CM.SetupDowntimes)
                .Add(CM.MachiningDowntimes)
                .Add(CM.PartialSetupTime)
                .Add(CM.MaintenanceTime)
                .Add(CM.ToolSearchingTime)
                .Add(CM.ToolChangingTime)
                .Add(CM.MentoringTime)
                .Add(CM.ContactingDepartmentsTime)
                .Add(CM.FixtureMakingTime)
                .Add(CM.HardwareFailureTime)
                .Add(CM.SpecifiedDowntimesRatio)
                .Add(CM.SpecifiedDowntimesComment)
                .Add(CM.SetupRatioTitle)
                .Add(CM.MasterSetupComment)
                .Add(CM.ProductionRatioTitle)
                .Add(CM.MasterProductionComment)
                .Add(CM.MasterComment)
                .Add(CM.FixedSetupTimePlan)
                .Add(CM.FixedProductionTimePlan)
                .Add(CM.EngineerComment)
                .Build();

            ConfigureWorksheetHeader(ws, cm);

            var ci = cm.GetIndexes();

            var row = 3;
            foreach (var part in parts)
            {
                ws.Cell(row, ci[CM.Machine]).SetValue(part.Machine);

                ws.Cell(row, ci[CM.Date])
                    .SetValue(part.ShiftDate)
                    .Style.DateFormat.Format = "dd.MM.yy";

                ws.Cell(row, ci[CM.Shift]).SetValue(part.Shift);

                ws.Cell(row, ci[CM.Operator]).SetValue(part.Operator);

                ws.Cell(row, ci[CM.Part]).SetValue(part.PartName);

                ws.Cell(row, ci[CM.Order]).SetValue(part.Order);

                ws.Cell(row, ci[CM.TotalByOrder]).SetValue(part.TotalCount);

                ws.Cell(row, ci[CM.Finished]).SetValue(part.FinishedCount);

                ws.Cell(row, ci[CM.Setup]).SetValue(part.Setup);

                ws.Cell(row, ci[CM.StartSetupTime])
                    .SetValue(part.StartSetupTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, ci[CM.StartMachiningTime])
                    .SetValue(part.StartMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, ci[CM.EndMachiningTime])
                    .SetValue(part.EndMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, ci[CM.SetupTimePlan]).SetValue(part.SetupTimePlan);

                ws.Cell(row, ci[CM.SetupTimeFact]).SetValue(part.SetupTimeFact);

                ws.Cell(row, ci[CM.SingleProductionTimePlan]).SetValue(part.SingleProductionTimePlan);

                ws.Cell(row, ci[CM.MachiningTime]).SetValue(part.MachiningTime);

                if (part.SingleProductionTime is double spt && spt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SingleProductionTime])
                        .SetValue(spt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                if (part.PartReplacementTime is double prt && prt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.PartReplacementTime])
                        .SetValue(prt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                ws.Cell(row, ci[CM.ProductionTimeFact]).SetValue(part.ProductionTimeFact);

                ws.Cell(row, ci[CM.PlanForBatch]).SetValue(part.PlanForBatch);

                ws.Cell(row, ci[CM.OperatorComment])
                    .SetValue(part.OperatorComment)
                    .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(row, ci[CM.SetupDowntimes]).SetValue(part.SetupDowntimes);

                ws.Cell(row, ci[CM.MachiningDowntimes]).SetValue(part.MachiningDowntimes);

                ws.Cell(row, ci[CM.PartialSetupTime]).SetValue(part.PartialSetupTime);

                ws.Cell(row, ci[CM.MaintenanceTime]).SetValue(part.MaintenanceTime);

                ws.Cell(row, ci[CM.ToolSearchingTime]).SetValue(part.ToolSearchingTime);

                ws.Cell(row, ci[CM.ToolChangingTime]).SetValue(part.ToolChangingTime);

                ws.Cell(row, ci[CM.MentoringTime]).SetValue(part.MentoringTime);

                ws.Cell(row, ci[CM.ContactingDepartmentsTime]).SetValue(part.ContactingDepartmentsTime);

                ws.Cell(row, ci[CM.FixtureMakingTime]).SetValue(part.FixtureMakingTime);

                ws.Cell(row, ci[CM.HardwareFailureTime]).SetValue(part.HardwareFailureTime);

                if (part.SpecifiedDowntimesRatio is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, ci[CM.SpecifiedDowntimesRatio])
                        .SetValue(part.SpecifiedDowntimesRatio)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, ci[CM.SpecifiedDowntimesComment]).SetValue(part.SpecifiedDowntimesComment);

                ws.Cell(row, ci[CM.SetupRatioTitle]).SetValue(part.SetupRatioTitle);

                ws.Cell(row, ci[CM.MasterSetupComment]).SetValue(part.MasterSetupComment);

                ws.Cell(row, ci[CM.ProductionRatioTitle]).SetValue(part.ProductionRatioTitle);

                ws.Cell(row, ci[CM.MasterProductionComment]).SetValue(part.MasterMachiningComment);

                ws.Cell(row, ci[CM.MasterComment]).SetValue(part.MasterComment);

                ws.Cell(row, ci[CM.FixedSetupTimePlan]).SetValue(part.FixedSetupTimePlan);

                ws.Cell(row, ci[CM.FixedProductionTimePlan]).SetValue(part.FixedProductionTimePlan);

                ws.Cell(row, ci[CM.EngineerComment]).SetValue(part.EngineerComment);

                row++;
            }


            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Range(3, ci[CM.Machine], row, ci[CM.Machine])
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(ci[CM.Operator]).Width = 15;

            ws.Column(ci[CM.Part]).Width = 25;

            ws.Range(3, ci[CM.OperatorComment], row, ci[CM.OperatorComment])
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(ci[CM.OperatorComment]).Width = 35;

            ws.Column(ci[CM.MasterSetupComment]).Width = 20;
            ws.Column(ci[CM.MasterProductionComment]).Width = 20;
            ws.Column(ci[CM.MasterComment]).Width = 20;
            ws.Row(1).Delete();
            ws.SheetView.FreezeRows(1);

            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
        }

        //public static string GetOperatorQualification(this string operatorName)
        //{
        //    if (!File.Exists(AppSettings.Instance.QualificationSourcePath)) return "Н/Д";
        //    try
        //    {
        //        using (var fs = new FileStream(AppSettings.Instance.QualificationSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        {
        //            var wb = new XLWorkbook(fs);
        //            foreach (var xlRow in wb.Worksheet(1).Rows().Skip(2))
        //            {
        //                if (xlRow.Cell(2).Value.IsText && xlRow.Cell(2).Value.GetText() == operatorName)
        //                {
        //                    if (xlRow.Cell(4).Value.IsText) return xlRow.Cell(4).Value.GetText();
        //                    else if (xlRow.Cell(4).Value.IsNumber) return xlRow.Cell(4).Value.GetNumber().ToString();
        //                }
        //            }
        //            return "Н/Д";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.WriteLog(ex);
        //        return "Н/Д";
        //    }
        //}

        public static string GetOperatorQualification(this string operatorFullName, IEnumerable<OperatorInfo> operators)
        {
            foreach (var @operator in operators)
            {
                if (@operator.FullName == operatorFullName)
                {
                    return @operator.Qualification.ToString();
                }
            }
            return "Н/Д";
        }


        private static void ConfigureWorksheetStyles(IXLWorksheet ws)
        {
            ws.Style.Font.FontSize = 12;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        /// <summary>
        /// Настраивает заголовки на рабочем листе Excel на основе словаря колонок.
        /// </summary>
        /// <param name="ws">Рабочий лист Excel.</param>
        /// <param name="columns">Словарь колонок, где ключ — идентификатор, значение — пара (индекс, заголовок).</param>
        /// <param name="headerRotateOption">Опция вращения заголовков (по умолчанию вертикально).</param>
        /// <param name="height">Высота строки заголовков (по умолчанию 90).</param>
        /// <param name="fontSize">Размер шрифта заголовков (по умолчанию 10).</param>
        private static void ConfigureWorksheetHeader(IXLWorksheet ws, Dictionary<string, (int index, string header)> columns, HeaderRotateOption headerRotateOption = HeaderRotateOption.Vertical, int height = 90, int fontSize = 10)
        {
            ConfigureWorksheetHeaderInternal(ws, columns.Select(c => (c.Value.index, c.Value.header)).ToList(), headerRotateOption, height, fontSize);
        }

        /// <summary>
        /// Настраивает заголовки на рабочем листе Excel EPPlus на основе словаря колонок.
        /// </summary>
        /// <param name="ws">Рабочий лист Excel.</param>
        /// <param name="columns">Словарь колонок, где ключ — идентификатор, значение — пара (индекс, заголовок).</param>
        /// <param name="headerRotateOption">Опция вращения заголовков (по умолчанию вертикально).</param>
        /// <param name="height">Высота строки заголовков (по умолчанию 90).</param>
        /// <param name="fontSize">Размер шрифта заголовков (по умолчанию 10).</param>
        private static void ConfigureWorksheetHeader(ExcelWorksheet ws, Dictionary<string, (int index, string header)> columns, HeaderRotateOption headerRotateOption = HeaderRotateOption.Vertical, int height = 90, int fontSize = 10)
        {
            ConfigureWorksheetHeaderInternal(ws, columns.Select(c => (c.Value.index, c.Value.header)).ToList(), headerRotateOption, height, fontSize);
        }

        /// <summary>
        /// Настраивает заголовки на рабочем листе Excel на основе объекта CM.
        /// </summary>
        /// <param name="ws">Рабочий лист Excel.</param>
        /// <param name="cm">Объект CM, содержащий информацию о колонках.</param>
        /// <param name="headerRotateOption">Опция вращения заголовков (по умолчанию вертикально).</param>
        /// <param name="height">Высота строки заголовков (по умолчанию 90).</param>
        /// <param name="fontSize">Размер шрифта заголовков (по умолчанию 10).</param>
        private static void ConfigureWorksheetHeader(IXLWorksheet ws, CM cm, HeaderRotateOption headerRotateOption = HeaderRotateOption.Vertical, int height = 90, int fontSize = 10)
        {
            ConfigureWorksheetHeaderInternal(ws, cm.GetIndexedHeaders().ToList(), headerRotateOption, height, fontSize);
        }

        /// <summary>
        /// Настраивает заголовки на рабочем листе Excel на основе объекта CM.
        /// </summary>
        /// <param name="ws">Рабочий лист Excel.</param>
        /// <param name="cm">Объект CM, содержащий информацию о колонках.</param>
        /// <param name="headerRotateOption">Опция вращения заголовков (по умолчанию вертикально).</param>
        /// <param name="height">Высота строки заголовков (по умолчанию 90).</param>
        /// <param name="fontSize">Размер шрифта заголовков (по умолчанию 10).</param>
        private static void ConfigureWorksheetHeader(ExcelWorksheet ws, CM cm, HeaderRotateOption headerRotateOption = HeaderRotateOption.Vertical, int height = 90, int fontSize = 10)
        {
            ConfigureWorksheetHeaderInternal(ws, cm.GetIndexedHeaders().ToList(), headerRotateOption, height, fontSize);
        }

        /// <summary>
        /// Настраивает заголовки на листе Excel.
        /// </summary>
        /// <param name="ws">Рабочий лист Excel.</param>
        /// <param name="columns">Список колонок, где каждая пара состоит из индекса и заголовка.</param>
        /// <param name="headerRotateOption">Опция вращения заголовков (по умолчанию вертикально).</param>
        /// <param name="height">Высота строки заголовков (по умолчанию 90).</param>
        /// <param name="fontSize">Размер шрифта заголовков (по умолчанию 10).</param>
        private static void ConfigureWorksheetHeaderInternal(IXLWorksheet ws, List<(int index, string header)> columns, HeaderRotateOption headerRotateOption, int height, int fontSize)
        {
            foreach (var (index, header) in columns)
            {
                ws.Cell(2, index).Value = header;
            }

            var headerRange = ws.Range(2, 1, 2, columns.Count);
            if (headerRotateOption == HeaderRotateOption.Vertical)
                headerRange.Style.Alignment.TextRotation = 90;

            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Font.FontSize = fontSize;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.WrapText = true;
            ws.Row(2).Height = height;
        }

        /// <summary>
        /// Конфигурирует заголовок таблицы Excel с использованием EPPlus.
        /// </summary>
        /// <param name="ws">Лист Excel.</param>
        /// <param name="columns">Список заголовков и их индексов.</param>
        /// <param name="headerRotateOption">Опция поворота заголовка.</param>
        /// <param name="height">Высота строки заголовка.</param>
        /// <param name="fontSize">Размер шрифта заголовка.</param>
        private static void ConfigureWorksheetHeaderInternal(ExcelWorksheet ws, List<(int index, string header)> columns, HeaderRotateOption headerRotateOption, int height, int fontSize)
        {
            // Установка заголовков
            foreach (var (index, header) in columns)
            {
                ws.Cells[2, index].Value = header; // Установка значения для ячейки
            }

            // Определение диапазона заголовков
            var headerRange = ws.Cells[2, 1, 2, columns.Count];

            // Поворот текста заголовка
            if (headerRotateOption == HeaderRotateOption.Vertical)
            {
                headerRange.Style.TextRotation = 90; // Вертикальный текст
            }

            // Настройка шрифта и выравнивания
            headerRange.Style.Font.Name = "Segoe UI Semibold";
            headerRange.Style.Font.Size = fontSize;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Установка высоты строки заголовка
            ws.Row(2).Height = height;
        }

        /// <summary>
        /// Устанавливает заголовок в первую строку рабочего листа, растягивая его на указанные столбцы.
        /// </summary>
        /// <param name="ws">Рабочий лист, на котором устанавливается заголовок.</param>
        /// <param name="columnsCount">Количество столбцов, на которые будет растянут заголовок (для объединения ячеек).</param>
        /// <param name="title">Текст заголовка, который будет выведен в первой ячейке.</param>
        /// <param name="fontSize">Размер шрифта для заголовка. По умолчанию 14.</param>
        /// <param name="bold">Указывает, будет ли заголовок полужирным. По умолчанию true.</param>
        /// <param name="merge">Указывает, нужно ли объединять ячейки заголовка на весь диапазон столбцов. По умолчанию true.</param>
        /// <param name="alignment">Выравнивание текста в ячейке. По умолчанию выравнивание по левому краю.</param>
        private static void SetTitle(IXLWorksheet ws, int columnsCount, string title, int fontSize = 14, bool bold = true, bool merge = true, XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
        {
            var cell = ws.Cell(1, 1).SetValue(title)
                .Style.Font.SetFontSize(fontSize)
                .Font.SetBold(bold)
                .Alignment.SetHorizontal(alignment)
                .Alignment.SetWrapText(false);

            if (merge)
            {
                ws.Range(1, 1, 1, columnsCount).Merge();
            }
        }

        /// <summary>
        /// Устанавливает два заголовка: один слева, другой справа. Каждый заголовок может быть выделен полужирным.
        /// </summary>
        /// <param name="ws">Рабочий лист, на котором устанавливаются заголовки.</param>
        /// <param name="columnsCount">Количество столбцов, на которые будет растянут второй заголовок (справа).</param>
        /// <param name="left">Текст заголовка, который будет выведен слева в первой ячейке.</param>
        /// <param name="right">Текст заголовка, который будет выведен справа в последней ячейке первой строки.</param>
        /// <param name="fontSize">Размер шрифта для обоих заголовков. По умолчанию 14.</param>
        /// <param name="bold">Указывает, будет ли левый и правый заголовок полужирным. 
        /// Определяется с помощью перечисления <see cref="BoldOption">BoldOption</see>. По умолчанию оба заголовка будут полужирными.</param>
        private static void SetTitle(IXLWorksheet ws, int columnsCount, string left, string right, int fontSize = 14, BoldOption bold = BoldOption.Both)
        {
            var leftCell = ws.Cell(1, 1).SetValue(left)
                .Style.Font.SetFontSize(fontSize)
                .Font.SetBold(bold == BoldOption.Left || bold == BoldOption.Both)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                .Alignment.SetWrapText(false);

            var rightCell = ws.Cell(1, columnsCount).SetValue(right)
                .Style.Font.SetFontSize(fontSize)
                .Font.SetBold(bold == BoldOption.Right || bold == BoldOption.Both)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right)
                .Alignment.SetWrapText(false);
        }

        private static void FillTotalMachinesWorksheetData(IXLWorksheet ws, IEnumerable<IGrouping<string, Part>> machinesGroup, Dictionary<string, (int index, string header)> columns, List<Part> tempParts)
        {
            int row = 3;

            foreach (var mg in machinesGroup)
            {
                foreach (var pg in mg.OrderBy(p => p.PartName).GroupBy(p => p.PartName))
                {
                    var p = pg.ToList();
                    var tp = tempParts.Where(tp => tp.PartName == pg.Key).ToList();
                    ws.Cell(row, columns["machine"].index).Value = mg.Key;
                    ws.Cell(row, columns["part"].index).Value = pg.Key;
                    ws.Cell(row, columns["ordersCnt"].index).SetValue(pg.Select(p => p.Order).Distinct().Count());
                    ws.Cell(row, columns["partsCnt"].index).SetValue(pg.Where(p => p.Setup == 1).Sum(p => p.FinishedCount));
                    ws.Cell(row, columns["planSum"].index).SetValue(pg.Sum(p => p.PlanForBatch) + pg.SetupTimePlanForReport());
                    ws.Cell(row, columns["factSum"].index).SetValue(pg.FullWorkedTime().TotalMinutes);
                    ws.Cell(row, columns["ordersCntTotal"].index).SetValue(tp.Select(p => p.Order).Distinct().Count());
                    ws.Cell(row, columns["partsCntTotal"].index).SetValue(tp.Where(p => p.Setup == 1).Sum(p => p.FinishedCount));
                    ws.Cell(row, columns["planSumTotal"].index).SetValue(tp.Sum(p => p.PlanForBatch) + tp.SetupTimePlanForReport());
                    ws.Cell(row, columns["factSumTotal"].index).SetValue(tp.FullWorkedTime().TotalMinutes);
                    row++;
                }
            }

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Range(3, columns["machine"].index, row, columns["part"].index)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Columns(columns["ordersCnt"].index, columns["factSumTotal"].index).Width = 8;
            ws.Cell(1, 1).Value = $"Изготовленное с {tempParts.Min(p => p.StartSetupTime).ToString(Constants.ShortDateFormat)} по {tempParts.Max(p => p.EndMachiningTime).ToString(Constants.ShortDateFormat)} ";
            ws.Range(1, columns["machine"].index, 1, columns["factSumTotal"].index).Merge();
            ws.SheetView.FreezeRows(2);
        }

        private static void FillMachineWorksheetData(IXLWorksheet ws, IEnumerable<Part> wsParts, Dictionary<string, (int index, string header)> columns)
        {
            int row = 3;
            int ordersCnt = 0;

            foreach (var pg in wsParts.OrderBy(p => p.PartName).GroupBy(p => p.PartName))
            {
                var p = pg.ToList();
                ws.Cell(row, columns["part"].index).Value = pg.Key;
                var orders = pg.Select(p => p.Order).Distinct().Count();
                ordersCnt += orders;
                ws.Cell(row, columns["ordersCnt"].index).SetValue(orders);
                ws.Cell(row, columns["partsCnt"].index).SetValue(pg.Where(p => p.Setup == 1).Sum(p => p.FinishedCount));
                ws.Cell(row, columns["planSum"].index).SetValue(pg.Sum(p => p.PlanForBatch) + pg.SetupTimePlanForReport());
                ws.Cell(row, columns["factSum"].index).SetValue(pg.FullWorkedTime().TotalMinutes);
                row++;
            }

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Range(3, columns["machine"].index, row, columns["part"].index)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Columns(columns["ordersCnt"].index, columns["factSum"].index).Width = 8;
            ws.Column(1).Delete();
            ws.Columns(6, 9).Delete();
            ws.Cell(1, 1).Value = $"Изготовленное с {wsParts.Min(p => p.StartSetupTime).ToString(Constants.ShortDateFormat)} по {wsParts.Max(p => p.EndMachiningTime).ToString(Constants.ShortDateFormat)} " +
                $"на станке {ws.Name} (всего м/л: {ordersCnt}, " +
                $"деталей: {wsParts.Where(p => p.Setup == 1).Sum(p => p.FinishedCount)})";
            ws.Range(1, columns["part"].index, 1, columns["factSum"].index).Merge();
            ws.SheetView.FreezeRows(2);
        }
    }
}
