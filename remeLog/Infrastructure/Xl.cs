using ClosedXML.Excel;
using libeLog;
using remeLog.Infrastructure.Extensions;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Part = remeLog.Models.Part;

namespace remeLog.Infrastructure
{
    public static class Xl
    {
        /// <summary>
        /// Отчёт за период
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExportReportForPeroid(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path, int? underOverBorder)
        {
            underOverBorder ??= 10;
            var machines = parts.Select(p => $"'{p.Machine}'").Distinct().ToArray();
            Database.GetShiftsByPeriod(machines, fromDate, toDate, out List<ShiftInfo> shifts);
            var totalDays = Util.GetWorkDaysBeetween(fromDate, toDate);
            double totalWorkedMinutes;

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет за период");
            ws.Style.Font.FontSize = 12;

            var headerRow = 2;

            var machineColId = 1;
            var workedShiftsColId = 2;
            var noOperatorShiftsColId = 3;
            var hrdwRepairShiftsColId = 4;
            var noPowerShifts = 5;
            var unspecOtherShiftsColId = 6;
            var setupRatioColId = 7;
            var productionRatioColId = 8;
            var setupRatioUnderColId = 9;
            var productionRatioUnderColId = 10;
            var setupRatioOverColId = 11;
            var productionRatioOverColId = 12;
            var setupUnderOverRatioColId = 13;
            var productionUnderOverRatioColId = 14;
            var productionToTotalRatioColId = 15;
            var productionEffToTotalRatioColId = 16;
            var avrReplTimeColId = 17;
            var specDowntimesColId = 18;
            var maintenanceTimeColId = 19;
            var toolSrchTimeColId = 20;
            var mentoringTimeColId = 21;
            var contactingDepartsTimeColId = 22;
            var fixtMakingTimeColId = 23;
            var hardwFailTimeColId = 24;
            var unspecDownTimesColId = 25;

            ws.Cell(headerRow, machineColId).Value = "Станок";
            ws.Cell(headerRow, workedShiftsColId).Value = "Отработанные смены";
            ws.Cell(headerRow, noOperatorShiftsColId).Value = "Смены без операторов";
            ws.Cell(headerRow, hrdwRepairShiftsColId).Value = "Смены с ремонтом оборудования";
            ws.Cell(headerRow, noPowerShifts).Value = "Смены без электропитания";
            ws.Cell(headerRow, unspecOtherShiftsColId).Value = "Смены без работы по другим причинам";
            ws.Cell(headerRow, setupRatioColId).Value = "Коэффициент наладки";
            ws.Cell(headerRow, productionRatioColId).Value = "Коэффициент изготовления";
            ws.Cell(headerRow, setupRatioUnderColId).Value = "Коэффициент наладки на штучке";
            ws.Cell(headerRow, productionRatioUnderColId).Value = "Коэффициент изготовления на штучке";
            ws.Cell(headerRow, setupRatioOverColId).Value = "Коэффициент наладки на серийке";
            ws.Cell(headerRow, productionRatioOverColId).Value = "Коэффициент изготовления на серийке";
            ws.Cell(headerRow, setupUnderOverRatioColId).Value = "Соотношение штучки к серийке при наладке";
            ws.Cell(headerRow, productionUnderOverRatioColId).Value = "Соотношение штучки к серийке при изготовлении";
            ws.Cell(headerRow, productionToTotalRatioColId).Value = "Отношение изготовления к общему времени";
            ws.Cell(headerRow, productionEffToTotalRatioColId).Value = "Отношение нормативов к общему времени";
            ws.Cell(headerRow, avrReplTimeColId).Value = "Среднее время замены детали (серия)";
            ws.Cell(headerRow, specDowntimesColId).Value = "Отмеченные простои";
            ws.Cell(headerRow, maintenanceTimeColId).Value = "Обслуживание";
            ws.Cell(headerRow, toolSrchTimeColId).Value = "Поиск инструмента";
            ws.Cell(headerRow, mentoringTimeColId).Value = "Обучение";
            ws.Cell(headerRow, contactingDepartsTimeColId).Value = "Другие службы";
            ws.Cell(headerRow, fixtMakingTimeColId).Value = "Изготовление оснастки";
            ws.Cell(headerRow, hardwFailTimeColId).Value = "Поломка оборудования";
            ws.Cell(headerRow, unspecDownTimesColId).Value = "Неуказанные простои";

            var lastCol = unspecDownTimesColId;
            var firstCol = machineColId;

            var headerRange = ws.Range(2, firstCol, 2, lastCol);
            var row = 3;
            var firstDataRow = row;

            foreach (var partGroup in parts.GroupBy(p => p.Machine).OrderBy(pg => pg.Key))
            {
                parts = partGroup.ToList();
                totalWorkedMinutes = parts.FullWorkedTime().TotalMinutes;
                ws.Cell(row, machineColId).Value = partGroup.Key;
                ws.Cell(row, workedShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s is not ({ Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 }));
                ws.Cell(row, noOperatorShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие оператора" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, hrdwRepairShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Ремонт оборудования" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, noPowerShifts).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие электричества" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is { Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 });
                ws.Cell(row, unspecOtherShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Другое" && !Constants.Dates.Holidays.Contains(s.ShiftDate) && s is ({ Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 }));
                ws.Cell(row, setupRatioColId).Value = parts.SetupRatio();
                ws.Cell(row, productionRatioColId).Value = parts.ProductionRatio();
                var setupUnderRatio = parts.Where(p => p.FinishedCountFact < underOverBorder).AverageSetupRatio();
                ws.Cell(row, setupRatioUnderColId).Value = setupUnderRatio;
                var productionUnderRatio = parts.Where(p => p.FinishedCountFact < underOverBorder).ProductionRatio();
                ws.Cell(row, productionRatioUnderColId).Value = productionUnderRatio;
                var setupOverRatio = parts.Where(p => p.FinishedCountFact >= underOverBorder).AverageSetupRatio();
                ws.Cell(row, setupRatioOverColId).Value = setupOverRatio;
                var productionOverRatio = parts.Where(p => p.FinishedCountFact >= underOverBorder).ProductionRatio();
                ws.Cell(row, productionRatioOverColId).Value = productionOverRatio;
                ws.Cell(row, setupUnderOverRatioColId).Value = setupUnderRatio == 0 ? 0 : setupOverRatio / setupUnderRatio;
                ws.Cell(row, productionUnderOverRatioColId).Value = productionUnderRatio == 0 ? 0 : productionOverRatio / productionUnderRatio;
                var prodTimeFactSum = parts.Sum(p => p.ProductionTimeFact);
                ws.Cell(row, productionToTotalRatioColId).Value = prodTimeFactSum / totalWorkedMinutes;
                var prodTimePlanSum = parts.Sum(p => p.PlanForBatch);
                ws.Cell(row, productionEffToTotalRatioColId).Value = prodTimePlanSum / totalWorkedMinutes;
                ws.Range(row, setupRatioColId, row, productionEffToTotalRatioColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, avrReplTimeColId).Value = parts.Where(p => p.FinishedCountFact >= underOverBorder).AverageReplacementTimeRatio();
                ws.Cell(row, avrReplTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, specDowntimesColId).Value = parts.SpecifiedDowntimesRatio(fromDate, toDate, ShiftType.All);
                ws.Cell(row, maintenanceTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.Maintenance);
                ws.Cell(row, toolSrchTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.ToolSearching);
                ws.Cell(row, mentoringTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.Mentoring);
                ws.Cell(row, contactingDepartsTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.ContactingDepartments);
                ws.Cell(row, fixtMakingTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.FixtureMaking);
                ws.Cell(row, hardwFailTimeColId).Value = parts.SpecifiedDowntimeRatio(Downtime.HardwareFailure);
                ws.Cell(row, unspecDownTimesColId).Value = parts.UnspecifiedDowntimesRatio(fromDate, toDate, ShiftType.All);
                ws.Range(row, specDowntimesColId, row, unspecDownTimesColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;


                row++;
            }
            var lastDataRow = row - 1;
            //ws.Cell(row, workedShiftsColId).Value = totalDays;
            var dataRange = ws.RangeUsed();
           
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            ws.Range(headerRow, machineColId, lastDataRow, machineColId).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, setupRatioColId, lastDataRow, productionRatioColId).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, setupRatioColId, lastDataRow, productionRatioColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.8);
            ws.Range(headerRow, setupRatioUnderColId, lastDataRow, productionRatioUnderColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent2, 0.8);
            ws.Range(headerRow, setupRatioOverColId, lastDataRow, productionRatioOverColId).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, setupRatioOverColId, lastDataRow, productionRatioOverColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent3, 0.8);
            ws.Range(headerRow, setupUnderOverRatioColId, lastDataRow, productionUnderOverRatioColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent4, 0.8);
            ws.Range(headerRow, productionToTotalRatioColId, lastDataRow, productionEffToTotalRatioColId).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, productionToTotalRatioColId, lastDataRow, productionEffToTotalRatioColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent5, 0.8);
            ws.Range(headerRow, specDowntimesColId, lastDataRow, unspecDownTimesColId).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.Range(headerRow, specDowntimesColId, lastDataRow, unspecDownTimesColId).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent6, 0.8);
            ws.Range(firstDataRow, workedShiftsColId, lastDataRow, workedShiftsColId).Style.Font.FontColor = XLColor.Red;
            ws.Range(firstDataRow, workedShiftsColId, lastDataRow, workedShiftsColId).AddConditionalFormat().WhenEquals($"=$B${lastDataRow + 2}").Font.FontColor = XLColor.Green;
            dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            dataRange.SetAutoFilter(true);
            ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Range(1, 1, 2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.TextRotation = 90;
            headerRange.Style.Alignment.WrapText = true;
            ws.Columns().AdjustToContents();
            ws.RowsUsed().Height = 20;
            ws.Row(2).Height = 130;
            ws.Cell(1, 1).Value = $"Отчёт за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            ws.Range(1, firstCol, 1, lastCol).Merge();
            ws.Range(1, firstCol, 1, 1).Style.Font.FontSize = 16;
            ws.Columns(3, lastCol).Width = 8;
           
            ws.Cell(row, machineColId).Value = "Соотношение:";
            ws.Cell(row, machineColId).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, machineColId, row, unspecOtherShiftsColId).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(row, noOperatorShiftsColId, row, unspecOtherShiftsColId).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, workedShiftsColId).FormulaA1 = $"AVERAGE(B{firstDataRow}:B{lastDataRow})/$B${lastDataRow + 2}";
            ws.Cell(row, noOperatorShiftsColId).FormulaA1 = $"AVERAGE(C{firstDataRow}:C{lastDataRow})/$B${lastDataRow + 2}";
            ws.Cell(row, hrdwRepairShiftsColId).FormulaA1 = $"AVERAGE(D{firstDataRow}:D{lastDataRow})/$B${lastDataRow + 2}";
            ws.Cell(row, noPowerShifts).FormulaA1 = $"AVERAGE(E{firstDataRow}:E{lastDataRow})/$B${lastDataRow + 2}";
            ws.Cell(row, unspecOtherShiftsColId).FormulaA1 = $"AVERAGE(F{firstDataRow}:F{lastDataRow})/$B${lastDataRow + 2}";
            ws.Range(row, workedShiftsColId, row, unspecOtherShiftsColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
            ws.Cell(row, workedShiftsColId).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, workedShiftsColId).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            row++;
            ws.Cell(row, machineColId).Value = "Рабочих смен:";
            ws.Cell(row, machineColId).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, machineColId).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell(row, workedShiftsColId).Value = totalDays * 2;
            ws.Cell(row, workedShiftsColId).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, workedShiftsColId).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }

        public static string ExportOperatorReport(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path, int? underOverBorder)
        {
            underOverBorder ??= 10;
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет по операторам");
            var workDays = Util.GetWorkDaysBeetween(fromDate, toDate);
            var columns = new Dictionary<string, (int index, string header)>
            {
                { "operator", (1, "Оператор") },
                { "qualification", (2, "Разряд") },
                { "machine", (3, "Станок") },
                { "setup", (4, "Наладка средняя") },
                { "prod", (5, "Изготовление") },
                { "avrReplTime", (6, $"Среднее{Environment.NewLine}время замены") },
                { "maintenanceTime", (7, "Обслуживание") },
                { "toolSrchTime", (8, "Инструмент") },
                { "mentoringTime", (9, "Обучение") },
                { "contactingDepartsTime", (10, "Другие службы") },
                { "fixtMakingTime", (11, "Оснастка") },
                { "hardwFailTime", (12, "Отказ оборудования") },
                { "specDowntimes", (13, $"Простои") },
                { "specDowntimesEx", (14, $"Простои{Environment.NewLine}(без отказа оборудования)") },
                { "generalRatio", (15, "Общая выработка") },
                { "cntSetups", (16, $"Количество{Environment.NewLine}наладок") },
                { "cntProds", (17, $"Количество{Environment.NewLine}изготовлений") },
                { "cntWorkedShifts", (18, "Отработано смен") },
                { "coefficient", (19, "Коэффициент") }
            };

            double? Coefficient(int qualification, double specDowntimesEx, double generalRatio, int workedShifts)
            {
                return specDowntimesEx > 0.1 || workedShifts < workDays / 4 ? null :
                    (qualification, generalRatio) switch
                    {
                        (1 or 2, > 1) => 1.2,
                        (1 or 2, > 0.9) => 1.1,
                        (3 or 4, > 1.05) => 1.2,
                        (3 or 4, > 0.95) => 1.1,
                        (5, > 1) => 1.1,
                        _ => null
                    };
            }

            foreach (var (index, header) in columns.Values)
            {
                ws.Cell(2, index).Value = header;
            }
            var lastCol = columns["coefficient"].index;
            var headerRange = ws.Range(2, columns["operator"].index, 2, lastCol);
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.TextRotation = 90;
            ws.Row(2).Height = 100;
            var row = 3;
            foreach (var partGroup in parts.Where(p => p.FinishedCountFact >= underOverBorder)
                               .GroupBy(p => new { p.Operator, p.Machine })
                               .OrderBy(g => g.Key.Machine)
                               .ThenBy(g => g.Key.Operator))
            {
                if (partGroup.Key.Operator == "Ученик") continue;
                var filteredParts = partGroup.ToList();

                ws.Cell(row, columns["operator"].index)
                  .SetValue(partGroup.Key.Operator);
                var qualification = partGroup.Key.Operator.GetOperatorQualification();
                var validQualification = int.TryParse(qualification, out int qualificationNumber);
                ws.Cell(row, columns["qualification"].index)
                  .SetValue(validQualification ? qualificationNumber : qualification);
                ws.Cell(row, columns["machine"].index)
                  .SetValue(filteredParts.First().Machine);

                var averageSetupRatio = filteredParts.AverageSetupRatio();
                ws.Cell(row, columns["setup"].index)
                  .SetValue(averageSetupRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var productionRatio = filteredParts.ProductionRatio();
                ws.Cell(row, columns["prod"].index)
                  .SetValue(productionRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, columns["avrReplTime"].index)
                  .SetValue(filteredParts.AverageReplacementTimeRatio())
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                ws.Cell(row, columns["maintenanceTime"].index)
                  .SetValue(filteredParts.Sum(p => p.MaintenanceTime));

                ws.Cell(row, columns["toolSrchTime"].index)
                  .SetValue(filteredParts.Sum(p => p.ToolSearchingTime));

                ws.Cell(row, columns["mentoringTime"].index)
                  .SetValue(filteredParts.Sum(p => p.MentoringTime));

                ws.Cell(row, columns["contactingDepartsTime"].index)
                  .SetValue(filteredParts.Sum(p => p.ContactingDepartmentsTime));

                ws.Cell(row, columns["fixtMakingTime"].index)
                  .SetValue(filteredParts.Sum(p => p.FixtureMakingTime));

                ws.Cell(row, columns["hardwFailTime"].index)
                  .SetValue(filteredParts.Sum(p => p.HardwareFailureTime));

                ws.Cell(row, columns["specDowntimes"].index)
                  .SetValue(filteredParts.SpecifiedDowntimesRatio(fromDate, toDate, ShiftType.All))
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var specDowntimesEx = filteredParts.SpecifiedDowntimesRatioExcluding(Downtime.HardwareFailure);
                ws.Cell(row, columns["specDowntimesEx"].index)
                  .SetValue(specDowntimesEx)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                var generalRatio = (averageSetupRatio != 0 && productionRatio != 0)
                    ? (averageSetupRatio + productionRatio) / 2
                    : (averageSetupRatio != 0 ? averageSetupRatio : productionRatio);

                ws.Cell(row, columns["generalRatio"].index)
                  .SetValue(generalRatio)
                  .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, columns["cntSetups"].index)
                  .SetValue(filteredParts.Count(p => p.SetupRatio is not (0 or double.NaN or double.NegativeInfinity or double.PositiveInfinity)));

                ws.Cell(row, columns["cntProds"].index)
                  .SetValue(filteredParts.Count(p => p.ProductionRatio != 0));

                var workedShifts = parts.Where(p => p.Operator == partGroup.Key.Operator && p.Machine == partGroup.Key.Machine)
                                 .Select(p => p.ShiftDate)
                                 .Distinct()
                                 .Count();
                ws.Cell(row, columns["cntWorkedShifts"].index)
                  .SetValue(workedShifts);

                if (validQualification)
                    ws.Cell(row, columns["coefficient"].index).SetValue(Coefficient(qualificationNumber, specDowntimesEx, generalRatio, workedShifts));

                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Column(columns["qualification"].index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(columns["qualification"].index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Columns(columns["maintenanceTime"].index, columns["specDowntimes"].index).Group(true);
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)} (изготовление от {underOverBorder} шт.)";
            ws.Range(1, columns["operator"].index, 1, lastCol).Merge();
            ws.Range(1, columns["operator"].index, 1, 1).Style.Font.FontSize = 14;
            ws.Columns(columns["setup"].index, lastCol).Width = 7;
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                == MessageBoxResult.Yes) Process.Start( new ProcessStartInfo() { UseShellExecute = true, FileName = path});
            return $"Файл сохранен в \"{path}\"";
        }

        public static string ExportDataset(ICollection<Part> parts, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Экспорт");
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell(1, 1).Value = "Дата";
            ws.Column(1).Width = 7;
            ws.Cell(1, 2).Value = "Смена";
            ws.Column(2).Width = 4;
            ws.Cell(1, 3).Value = "Оператор";
            ws.Column(3).Width = 15;
            ws.Cell(1, 4).Value = "Деталь";
            ws.Column(4).Width = 25;
            ws.Cell(1, 5).Value = "М/Л";
            ws.Column(5).Width = 15;
            ws.Cell(1, 6).Value = "Всего по М/Л";
            ws.Column(6).Width = 4;
            ws.Cell(1, 7).Value = "Выполнено";
            ws.Column(7).Width = 4;
            ws.Cell(1, 8).Value = "Установка";
            ws.Column(8).Width = 4;
            ws.Cell(1, 9).Value = "Начало наладки";
            ws.Column(9).Width = 5;
            ws.Cell(1, 10).Value = "Начало изготовления";
            ws.Column(10).Width = 5;
            ws.Cell(1, 11).Value = "Конец изготовления";
            ws.Column(11).Width = 5;
            ws.Cell(1, 12).Value = "Норматив наладки";
            ws.Column(12).Width = 5;
            ws.Cell(1, 13).Value = "Фактическая наладка";
            ws.Column(13).Width = 5;
            ws.Cell(1, 14).Value = "Норматив штучный";
            ws.Column(14).Width = 5;
            ws.Cell(1, 15).Value = "Машинное время";
            ws.Column(15).Width = 8;
            ws.Cell(1, 16).Value = "Штучное фактическое";
            ws.Column(16).Width = 5;
            ws.Cell(1, 17).Value = "Время замены";
            ws.Column(17).Width = 5;
            ws.Cell(1, 18).Value = "Фактическое изготовление";
            ws.Column(18).Width = 5;
            ws.Cell(1, 19).Value = "Норматив на партию";
            ws.Column(19).Width = 5;
            ws.Cell(1, 20).Value = "Комментарий оператора";
            ws.Column(20).Width = 30;
            ws.Cell(1, 21).Value = "Простои в наладке";
            ws.Column(21).Width = 5;
            ws.Cell(1, 22).Value = "Простои в изготовлении";
            ws.Column(22).Width = 5;
            ws.Cell(1, 23).Value = "Частичная наладка";
            ws.Column(23).Width = 5;
            ws.Cell(1, 24).Value = "Обслуживание";
            ws.Column(24).Width = 5;
            ws.Cell(1, 25).Value = "Поиск инструмента";
            ws.Column(25).Width = 5;
            ws.Cell(1, 26).Value = "Обучение";
            ws.Column(26).Width = 5;
            ws.Cell(1, 27).Value = "Другие службы";
            ws.Column(27).Width = 5;
            ws.Cell(1, 28).Value = "Изготовление оснастки";
            ws.Column(28).Width = 5;
            ws.Cell(1, 29).Value = "Отказ оборудования";
            ws.Column(29).Width = 5;
            ws.Cell(1, 30).Value = "Отмеченные простои";
            ws.Column(30).Width = 5;
            ws.Cell(1, 31).Value = "Комментарий к простоям";
            ws.Column(31).Width = 5;
            ws.Cell(1, 32).Value = "Наладка";
            ws.Column(32).Width = 5;
            ws.Cell(1, 33).Value = "Невыполнение норматива наладки";
            ws.Column(33).Width = 16;
            ws.Cell(1, 34).Value = "Изготовление";
            ws.Column(34).Width = 5;
            ws.Cell(1, 35).Value = "Невыполнение норматива изготовления";
            ws.Column(35).Width = 15;
            ws.Cell(1, 36).Value = "Комментарий мастера";
            ws.Column(36).Width = 15;
            ws.Cell(1, 37).Value = "Норматив наладки (И)";
            ws.Column(37).Width = 4;
            ws.Cell(1, 38).Value = "Норматив изготовления (И)";
            ws.Column(38).Width = 4;
            ws.Cell(1, 39).Value = "Комментарий техотдела";
            ws.Column(39).Width = 15;
            var headerRange = ws.Range(1, 1, 1, 39);
            headerRange.Style.Alignment.TextRotation = 90;
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            var row = 2;
            foreach (var part in parts)
            {
                ws.Cell(row, 1).Value = part.ShiftDate;
                ws.Cell(row, 1).Style.DateFormat.Format = "dd.mm.yy";
                ws.Cell(row, 2).Value = part.Shift;
                ws.Cell(row, 3).Value = part.Operator;
                ws.Cell(row, 4).Value = part.PartName;
                ws.Cell(row, 5).Value = part.Order;
                ws.Cell(row, 6).Value = part.TotalCount;
                ws.Cell(row, 7).Value = part.FinishedCount;
                ws.Cell(row, 8).Value = part.Setup;
                ws.Cell(row, 9).Value = part.StartSetupTime;
                ws.Cell(row, 9).Style.DateFormat.Format = "hh:mm";
                ws.Cell(row, 10).Value = part.StartMachiningTime;
                ws.Cell(row, 10).Style.DateFormat.Format = "hh:mm";
                ws.Cell(row, 11).Value = part.EndMachiningTime;
                ws.Cell(row, 11).Style.DateFormat.Format = "hh:mm";
                ws.Cell(row, 12).Value = part.SetupTimePlan;
                ws.Cell(row, 13).Value = part.SetupTimeFact;
                ws.Cell(row, 14).Value = part.SingleProductionTimePlan;
                ws.Cell(row, 15).Value = part.MachiningTime;
                if (part.SingleProductionTime is double spt && spt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity)) ws.Cell(row, 16).Value = spt;
                ws.Cell(row, 16).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                if (part.PartReplacementTime is double prt && prt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity)) ws.Cell(row, 17).Value = prt;
                ws.Cell(row, 17).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, 18).Value = part.ProductionTimeFact;
                ws.Cell(row, 19).Value = part.PlanForBatch;
                ws.Cell(row, 20).Value = part.OperatorComment;
                ws.Cell(row, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell(row, 21).Value = part.SetupDowntimes;
                ws.Cell(row, 22).Value = part.MachiningDowntimes;
                ws.Cell(row, 23).Value = part.PartialSetupTime;
                ws.Cell(row, 24).Value = part.MaintenanceTime;
                ws.Cell(row, 25).Value = part.ToolSearchingTime;
                ws.Cell(row, 26).Value = part.MentoringTime;
                ws.Cell(row, 27).Value = part.ContactingDepartmentsTime;
                ws.Cell(row, 28).Value = part.FixtureMakingTime;
                ws.Cell(row, 29).Value = part.HardwareFailureTime;
                ws.Cell(row, 30).Value = part.SpecifiedDowntimesRatio;
                ws.Cell(row, 30).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, 31).Value = part.SpecifiedDowntimesComment;
                ws.Cell(row, 32).Value = part.SetupRatioTitle;
                ws.Cell(row, 33).Value = part.MasterSetupComment;
                ws.Cell(row, 34).Value = part.ProductionRatioTitle;
                ws.Cell(row, 35).Value = part.MasterMachiningComment;
                ws.Cell(row, 36).Value = part.MasterComment;
                ws.Cell(row, 37).Value = part.FixedSetupTimePlan;
                ws.Cell(row, 38).Value = part.FixedProductionTimePlan;
                ws.Cell(row, 39).Value = part.EngineerComment;
                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns(24, 29).Group(true);
            ws.Columns(18, 19).Group(true);
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }

        public static string GetOperatorQualification(this string operatorName)
        {
            if (!File.Exists(AppSettings.Instance.QualificationSourcePath)) return "Н/Д";
            try
            {
                using (var fs = new FileStream(AppSettings.Instance.QualificationSourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var wb = new XLWorkbook(fs);
                    foreach (var xlRow in wb.Worksheet(1).Rows().Skip(2))
                    {
                        if (xlRow.Cell(2).Value.IsText && xlRow.Cell(2).Value.GetText() == operatorName)
                        {
                            if (xlRow.Cell(4).Value.IsText) return xlRow.Cell(4).Value.GetText();
                            else if (xlRow.Cell(4).Value.IsNumber) return xlRow.Cell(4).Value.GetNumber().ToString();
                        }
                    }
                    return "Н/Д";
                }
            }
            catch (Exception ex)
            {
                Util.WriteLog(ex);
                return "Н/Д";
            }
        }
    }
}
