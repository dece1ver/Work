using ClosedXML.Excel;
using libeLog;
using remeLog.Infrastructure.Extensions;
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
        public static ICollection<Models.Part> ReadParts()
        {
            List<Part> parts = new List<Part>();
            if (!Directory.Exists(AppSettings.Instance.SourcePath)) { return parts; }

            using (var fs = new FileStream(AppSettings.Instance.SourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });

                foreach (var xlRow in wb.Worksheet(1).Rows().Skip(1))
                {
                    if (!xlRow.Cell(1).Value.IsNumber) break;
                    parts.Add(new Part(
                        Guid.Parse(xlRow.Cell(36).Value.GetText()),
                        xlRow.Cell(7).Value.GetText(),
                        xlRow.Cell(6).Value.GetText(),
                        xlRow.Cell(8).Value.GetDateTime(),
                        xlRow.Cell(9).Value.GetText(),
                        xlRow.Cell(10).Value.GetText(),
                        xlRow.Cell(11).Value.GetText(),
                        (int)xlRow.Cell(13).Value.GetNumber(),
                        (int)xlRow.Cell(12).Value.GetNumber(),
                        0,
                        xlRow.Cell(14).Value.GetDateTime(),
                        xlRow.Cell(15).Value.GetDateTime(),
                        xlRow.Cell(16).Value.GetNumber(),
                        xlRow.Cell(21).Value.GetDateTime(),
                        xlRow.Cell(17).Value.GetNumber(),
                        xlRow.Cell(44).Value.GetNumber(),
                        xlRow.Cell(23).Value.GetNumber(),
                        xlRow.Cell(22).Value.GetNumber(),
                        TimeSpan.FromMinutes(xlRow.Cell(24).Value.GetNumber()),
                        xlRow.Cell(34).Value.GetNumber(),
                        xlRow.Cell(35).Value.GetNumber(),
                        xlRow.Cell(37).Value.GetNumber(),
                        xlRow.Cell(38).Value.GetNumber(),
                        xlRow.Cell(39).Value.GetNumber(),
                        xlRow.Cell(40).Value.GetNumber(),
                        xlRow.Cell(41).Value.GetNumber(),
                        xlRow.Cell(42).Value.GetNumber(),
                        xlRow.Cell(43).Value.GetNumber(),
                        xlRow.Cell(5).Value.GetText()
                        ));
                }
            }

            return parts;
        }

        /// <summary>
        /// Отчёт за период
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExportReportForPeroid(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path)
        {
            var machines = parts.Select(p => $"'{p.Machine}'").Distinct().ToArray();
            
            Database.GetShiftsByPeriod(machines, fromDate, toDate, out List<ShiftInfo> shifts);

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет за период");
            ws.Style.Font.FontSize = 12;

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
            var unspecDownTimes = 25;

            ws.Cell(2, machineColId).Value = "Станок";
            ws.Cell(2, workedShiftsColId).Value = "Отработанные смены";
            ws.Cell(2, noOperatorShiftsColId).Value = "Смены без операторов";
            ws.Cell(2, hrdwRepairShiftsColId).Value = "Смены с ремонтом оборудования";
            ws.Cell(2, noPowerShifts).Value = "Смены без электропитания";
            ws.Cell(2, unspecOtherShiftsColId).Value = "Смены без работы по другим причинам";
            ws.Cell(2, setupRatioColId).Value = "Коэффициент наладки";
            ws.Cell(2, productionRatioColId).Value = "Коэффициент изготовления";
            ws.Cell(2, setupRatioUnderColId).Value = "Коэффициент наладки на штучке";
            ws.Cell(2, productionRatioUnderColId).Value = "Коэффициент изготовления на штучке";
            ws.Cell(2, setupRatioOverColId).Value = "Коэффициент наладки на серийке";
            ws.Cell(2, productionRatioOverColId).Value = "Коэффициент изготовления на серийке";
            ws.Cell(2, setupUnderOverRatioColId).Value = "Соотношение штучки к серийке при наладке";
            ws.Cell(2, productionUnderOverRatioColId).Value = "Соотношение штучки к серийке при изготовлении";
            ws.Cell(2, productionToTotalRatioColId).Value = "Отношение изготовления к общему времени";
            ws.Cell(2, productionEffToTotalRatioColId).Value = "Отношение нормативов к общему времени";
            ws.Cell(2, avrReplTimeColId).Value = "Среднее время замены детали";
            ws.Cell(2, specDowntimesColId).Value = "Отмеченные простои";
            ws.Cell(2, maintenanceTimeColId).Value = "Обслуживание";
            ws.Cell(2, toolSrchTimeColId).Value = "Поиск инструмента";
            ws.Cell(2, mentoringTimeColId).Value = "Обучение";
            ws.Cell(2, contactingDepartsTimeColId).Value = "Другие службы";
            ws.Cell(2, fixtMakingTimeColId).Value = "Изготовление оснастки";
            ws.Cell(2, hardwFailTimeColId).Value = "Поломка оборудования";
            ws.Cell(2, unspecDownTimes).Value = "Неуказанные простои";

            var lastCol = unspecDownTimes;
            var firstCol = machineColId;

            var headerRange = ws.Range(2, firstCol, 2, lastCol);
            var row = 3;

            foreach (var partGroup in parts.GroupBy(p => p.Machine).OrderBy(pg => pg.Key))
            {
                parts = partGroup.ToList();
                ws.Cell(row, machineColId).Value = partGroup.Key;
                ws.Cell(row, workedShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s is not ({ Shift: "День", UnspecifiedDowntimes: 660 } or { Shift: "Ночь", UnspecifiedDowntimes: 630 }));
                ws.Cell(row, noOperatorShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие оператора");
                ws.Cell(row, hrdwRepairShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Ремонт оборудования");
                ws.Cell(row, noPowerShifts).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Отсутствие электричества");
                ws.Cell(row, unspecOtherShiftsColId).Value = shifts.Count(s => s.Machine == partGroup.Key && s.DowntimesComment == "Другое");
                ws.Cell(row, setupRatioColId).Value = parts.SetupRatio();
                ws.Cell(row, productionRatioColId).Value = parts.ProductionRatio();
                ws.Cell(row, setupRatioUnderColId).Value = parts.Where(p => p.FinishedCount < 10).SetupRatio();
                ws.Cell(row, productionRatioUnderColId).Value = parts.Where(p => p.FinishedCount < 10).ProductionRatio();
                ws.Cell(row, setupRatioOverColId).Value = parts.Where(p => p.FinishedCount >= 10).SetupRatio();
                ws.Cell(row, productionRatioOverColId).Value = parts.Where(p => p.FinishedCount >= 10).ProductionRatio();


                ws.Range(row, setupRatioColId, row, productionRatioOverColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                row++;
            }

            var dataRange = ws.RangeUsed();
           
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
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
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }

        public static string ExportOperatorReport(ICollection<Part> parts, DateTime fromDate, DateTime toDate, string path)
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет по операторам");

            var operatorColId = 1;
            var machineColId = 2;
            var setupColId = 3;
            var prodColId = 4;
            var avrReplTimeColId = 5;
            var maintenanceTimeColId = 6;
            var toolSrchTimeColId = 7;
            var mentoringTimeColId = 8;
            var contactingDepartsTimeColId = 9;
            var fixtMakingTimeColId = 10;
            var hardwFailTimeColId = 11;
            var specDowntimesColId = 12;
            ws.Cell(2, operatorColId).Value = "Оператор";
            ws.Cell(2, machineColId).Value = "Станок";
            ws.Cell(2, setupColId).Value = "Наладка средняя";
            ws.Cell(2, prodColId).Value = "Изготовление";
            ws.Cell(2, avrReplTimeColId).Value = "Среднее время замены";
            ws.Cell(2, maintenanceTimeColId).Value = "Обслуживание";
            ws.Cell(2, toolSrchTimeColId).Value = "Инструмент";
            ws.Cell(2, mentoringTimeColId).Value = "Обучение";
            ws.Cell(2, contactingDepartsTimeColId).Value = "Другие службы";
            ws.Cell(2, fixtMakingTimeColId).Value = "Оснастка";
            ws.Cell(2, hardwFailTimeColId).Value = "Отказ оборудования";
            ws.Cell(2, specDowntimesColId).Value = "Простои";
            var lastCol = specDowntimesColId;
            var headerRange = ws.Range(2, operatorColId, 2, lastCol);
            headerRange.Style.Font.FontName = "Segoe UI Semibold";
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Alignment.TextRotation = 90;
            var row = 3;
            foreach (var partGroup in parts.GroupBy(p => p.Operator).OrderBy(pg => pg.Key))
            {
                parts = partGroup.ToList();
                ws.Cell(row, operatorColId).Value = partGroup.Key;
                ws.Cell(row, machineColId).Value = parts.First().Machine;
                ws.Cell(row, setupColId).Value = parts.AverageSetupRatio();
                ws.Cell(row, setupColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, prodColId).Value = parts.ProductionRatio();
                ws.Cell(row, prodColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, avrReplTimeColId).Value = parts.AverageReplacementTimeRatio();
                ws.Cell(row, avrReplTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, maintenanceTimeColId).Value = parts.Sum(p => p.MaintenanceTime);
                ws.Cell(row, maintenanceTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, toolSrchTimeColId).Value = parts.Sum(p => p.ToolSearchingTime);
                ws.Cell(row, toolSrchTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, mentoringTimeColId).Value = parts.Sum(p => p.MentoringTime);
                ws.Cell(row, mentoringTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, contactingDepartsTimeColId).Value = parts.Sum(p => p.ContactingDepartmentsTime);
                ws.Cell(row, contactingDepartsTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, fixtMakingTimeColId).Value = parts.Sum(p => p.FixtureMakingTime);
                ws.Cell(row, fixtMakingTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, hardwFailTimeColId).Value = parts.Sum(p => p.HardwareFailureTime);
                ws.Cell(row, hardwFailTimeColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, specDowntimesColId).Value = parts.SpecifiedDowntimesRatio(fromDate, toDate, Types.ShiftType.All);
                ws.Cell(row, specDowntimesColId).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Columns(5, 11).Group(true);
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            ws.Range(1, operatorColId, 1, lastCol).Merge();
            ws.Range(1, operatorColId, 1, 1).Style.Font.FontSize = 16;
            ws.Columns(3, lastCol).Width = 8;
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
    }
}
