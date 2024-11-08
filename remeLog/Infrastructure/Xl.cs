﻿using ClosedXML.Excel;
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
        const double Coefficient1 = 1.2;
        const double Coefficient2 = 1.4;
        public enum ExportOperatorReportType
        {
            Under, Below
        }

        public enum HeaderRotateOption
        {
            Horizontal, Vertical
        }

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
            var machines = parts.Where(p => !p.ExcludeFromReports).Select(p => p.Machine).Distinct().ToArray();
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

            foreach (var partGroup in parts.Where(p => !p.ExcludeFromReports).GroupBy(p => p.Machine).OrderBy(pg => pg.Key))
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
                { "generalRatio", (15, "Эффективность") },
                { "cntSetups", (16, $"Количество{Environment.NewLine}наладок") },
                { "cntProds", (17, $"Количество{Environment.NewLine}изготовлений") },
                { "cntWorkedShifts", (18, "Отработано смен") },
                { "coefficient", (19, "Коэффициент") }
            };

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

            foreach (var (index, header) in columns.Values)
            {
                ws.Cell(2, index).Value = header;
            }
            var lastCol = columns.Count;
            var headerRange = ws.Range(2, columns["operator"].index, 2, lastCol);
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.TextRotation = 90;
            ws.Row(2).Height = 100;
            var row = 3;
            // .Where(p => p.FinishedCountFact >= minPartsCount && p.FinishedCountFact < maxPartsCount && !p.ExcludeFromReports)
            foreach (var partGroup in parts
                .Where(p => !p.ExcludeFromReports)
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

                var productionRatio = filteredParts.Where(p => p.FinishedCountFact >= minPartsCount && p.FinishedCountFact < maxPartsCount && !p.ExcludeFromReports).ProductionRatio();
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
                if (qualificationNumber == 1 && productionRatio != 0) generalRatio = productionRatio;

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
                    ws.Cell(row, columns["coefficient"].index).SetValue(Coefficient(qualificationNumber, specDowntimesEx, generalRatio, averageSetupRatio, workedShifts));

                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Column(columns["qualification"].index).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(columns["qualification"].index).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Columns(columns["maintenanceTime"].index, columns["specDowntimes"].index).Group(true);
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)} (изготовление от {minPartsCount}{(maxPartsCount == int.MaxValue ? "" : $" до {maxPartsCount}")} шт.)";
            ws.Range(1, columns["operator"].index, 1, lastCol).Merge();
            ws.Range(1, columns["operator"].index, 1, 1).Style.Font.FontSize = 14;
            ws.Columns(columns["setup"].index, lastCol).Width = 7;
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

            switch (Database.GetShiftsByPeriod(machines, fromDate, toDate, out var shifts))
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
                var vvv = row.Cell(2).Value;
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


        public static string ExportDataset(ICollection<Part> parts, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Экспорт");
            ws.Style.Font.FontSize = 10;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            var columns = new Dictionary<string, (int index, string header)>
            {
                { "machine", (1, "Станок") },
                { "date", (2, "Дата") },
                { "shift", (3, "Смена") },
                { "operator", (4, "Оператор") },
                { "part", (5, "Деталь") },
                { "order", (6, "М/Л") },
                { "totalByOrder", (7, "Всего по М/Л") },
                { "finished", (8, "Выполнено") },
                { "setup", (9, "Установка") },
                { "startSetupTime", (10, "Начало наладки") },
                { "startMachiningTime", (11, $"Начало{Environment.NewLine}изготовления") },
                { "endMachiningTime", (12, $"Конец{Environment.NewLine}изготовления") },
                { "setupTimePlan", (13, $"Норматив{Environment.NewLine}наладки") },
                { "setupTimeFact", (14, $"Фактическая{Environment.NewLine}наладка") },
                { "singleProductionTimePlan", (15, $"Норматив{Environment.NewLine}штучный") },
                { "machiningTime", (16, "Машинное время") },
                { "singleProductionTime", (17, $"Штучное{Environment.NewLine}фактическое") },
                { "partReplacementTime", (18, "Время замены") },
                { "productionTimeFact", (19, $"Фактическое{Environment.NewLine}изготовление") },
                { "planForBatch", (20, $"Норматив{Environment.NewLine}на партию") },
                { "operatorComment", (21, $"Комментарий{Environment.NewLine}оператора") },
                { "setupDowntimes", (22, $"Простои{Environment.NewLine}в наладке") },
                { "machiningDowntimes", (23, $"Простои{Environment.NewLine}в изготовлении") },
                { "partialSetupTime", (24, $"Частичная{Environment.NewLine}наладка") },
                { "maintenanceTime", (25, "Обслуживание") },
                { "toolSearchingTime", (26, $"Поиск{Environment.NewLine}инструмента") },
                { "mentoringTime", (27, "Обучение") },
                { "contactingDepartmentsTime", (28, "Другие службы") },
                { "fixtureMakingTime", (29, $"Изготовление{Environment.NewLine}оснастки") },
                { "hardwareFailureTime", (30, $"Отказ{Environment.NewLine}оборудования") },
                { "specifiedDowntimesRatio", (31, $"Отмеченные{Environment.NewLine}простои") },
                { "specifiedDowntimesComment", (32, $"Комментарий{Environment.NewLine}к простоям") },
                { "setupRatioTitle", (33, "Наладка") },
                { "masterSetupComment", (34, $"Невыполнение{Environment.NewLine}норматива{Environment.NewLine}наладки") },
                { "productionRatioTitle", (35, "Изготовление") },
                { "masterMachiningComment", (36, $"Невыполнение{Environment.NewLine}норматива{Environment.NewLine}изготовления") },
                { "masterComment", (37, $"Комментарий{Environment.NewLine}мастера") },
                { "fixedSetupTimePlan", (38, $"Норматив{Environment.NewLine}наладки (И)") },
                { "fixedProductionTimePlan", (39, $"Норматив{Environment.NewLine}изготовления (И)") },
                { "engineerComment", (40, $"Комментарий{Environment.NewLine}техотдела") }
            };

            foreach (var (index, header) in columns.Values)
            {
                ws.Cell(1, index).Value = header;
            }

            var headerRange = ws.Range(1, 1, 1, columns.Count);
            headerRange.Style.Alignment.TextRotation = 90;
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            ws.Row(1).Height = 100;

            var row = 2;
            foreach (var part in parts)
            {
                ws.Cell(row, columns["machine"].index).SetValue(part.Machine);

                ws.Cell(row, columns["date"].index)
                    .SetValue(part.ShiftDate)
                    .Style.DateFormat.Format = "dd.MM.yy";

                ws.Cell(row, columns["shift"].index).SetValue(part.Shift);

                ws.Cell(row, columns["operator"].index).SetValue(part.Operator);

                ws.Cell(row, columns["part"].index).SetValue(part.PartName);

                ws.Cell(row, columns["order"].index).SetValue(part.Order);

                ws.Cell(row, columns["totalByOrder"].index).SetValue(part.TotalCount);

                ws.Cell(row, columns["finished"].index).SetValue(part.FinishedCount);

                ws.Cell(row, columns["setup"].index).SetValue(part.Setup);

                ws.Cell(row, columns["startSetupTime"].index)
                    .SetValue(part.StartSetupTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, columns["startMachiningTime"].index)
                    .SetValue(part.StartMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, columns["endMachiningTime"].index)
                    .SetValue(part.EndMachiningTime)
                    .Style.DateFormat.Format = "HH:mm";

                ws.Cell(row, columns["setupTimePlan"].index).SetValue(part.SetupTimePlan);

                ws.Cell(row, columns["setupTimeFact"].index).SetValue(part.SetupTimeFact);

                ws.Cell(row, columns["singleProductionTimePlan"].index).SetValue(part.SingleProductionTimePlan);

                ws.Cell(row, columns["machiningTime"].index).SetValue(part.MachiningTime);

                if (part.SingleProductionTime is double spt && spt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, columns["singleProductionTime"].index)
                        .SetValue(spt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                if (part.PartReplacementTime is double prt && prt is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, columns["partReplacementTime"].index)
                        .SetValue(prt)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;

                ws.Cell(row, columns["productionTimeFact"].index).SetValue(part.ProductionTimeFact);

                ws.Cell(row, columns["planForBatch"].index).SetValue(part.PlanForBatch);

                ws.Cell(row, columns["operatorComment"].index)
                    .SetValue(part.OperatorComment)
                    .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                ws.Cell(row, columns["setupDowntimes"].index).SetValue(part.SetupDowntimes);

                ws.Cell(row, columns["machiningDowntimes"].index).SetValue(part.MachiningDowntimes);

                ws.Cell(row, columns["partialSetupTime"].index).SetValue(part.PartialSetupTime);

                ws.Cell(row, columns["maintenanceTime"].index).SetValue(part.MaintenanceTime);

                ws.Cell(row, columns["toolSearchingTime"].index).SetValue(part.ToolSearchingTime);

                ws.Cell(row, columns["mentoringTime"].index).SetValue(part.MentoringTime);

                ws.Cell(row, columns["contactingDepartmentsTime"].index).SetValue(part.ContactingDepartmentsTime);

                ws.Cell(row, columns["fixtureMakingTime"].index).SetValue(part.FixtureMakingTime);

                ws.Cell(row, columns["hardwareFailureTime"].index).SetValue(part.HardwareFailureTime);

                if (part.SpecifiedDowntimesRatio is not (double.NaN or double.NegativeInfinity or double.PositiveInfinity))
                    ws.Cell(row, columns["specifiedDowntimesRatio"].index)
                        .SetValue(part.SpecifiedDowntimesRatio)
                        .Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;

                ws.Cell(row, columns["specifiedDowntimesComment"].index).SetValue(part.SpecifiedDowntimesComment);

                ws.Cell(row, columns["setupRatioTitle"].index).SetValue(part.SetupRatioTitle);

                ws.Cell(row, columns["masterSetupComment"].index).SetValue(part.MasterSetupComment);

                ws.Cell(row, columns["productionRatioTitle"].index).SetValue(part.ProductionRatioTitle);

                ws.Cell(row, columns["masterMachiningComment"].index).SetValue(part.MasterMachiningComment);

                ws.Cell(row, columns["masterComment"].index).SetValue(part.MasterComment);

                ws.Cell(row, columns["fixedSetupTimePlan"].index).SetValue(part.FixedSetupTimePlan);

                ws.Cell(row, columns["fixedProductionTimePlan"].index).SetValue(part.FixedProductionTimePlan);

                ws.Cell(row, columns["engineerComment"].index).SetValue(part.EngineerComment);

                row++;
            }

            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();

            ws.Range(2, columns["machine"].index, row, columns["machine"].index)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(columns["operator"].index).Width = 15;

            ws.Column(columns["part"].index).Width = 25;

            ws.Range(2, columns["operatorComment"].index, row, columns["operatorComment"].index)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            ws.Column(columns["operatorComment"].index).Width = 35;

            ws.Column(columns["masterSetupComment"].index).Width = 20;
            ws.Column(columns["masterMachiningComment"].index).Width = 20;
            ws.Column(columns["masterComment"].index).Width = 20;

            ws.SheetView.FreezeRows(1);

            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return path;
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
                Util.WriteLogAsync(ex);
                return "Н/Д";
            }
        }


        private static void ConfigureWorksheetStyles(IXLWorksheet ws)
        {
            ws.Style.Font.FontSize = 12;
            ws.Style.Alignment.WrapText = true;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void ConfigureWorksheetHeader(IXLWorksheet ws, Dictionary<string, (int index, string header)> columns, HeaderRotateOption headerRotateOption = HeaderRotateOption.Vertical)
        {
            foreach (var (index, header) in columns.Values)
            {
                ws.Cell(2, index).Value = header;
            }

            var headerRange = ws.Range(2, 1, 2, columns.Count);
            if (headerRotateOption == HeaderRotateOption.Vertical) headerRange.Style.Alignment.TextRotation = 90;
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Font.FontSize = 10;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 100;
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
