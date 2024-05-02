using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using libeLog;
using remeLog.Infrastructure.Extensions;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
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


        public static string ExportOperatorReport(ICollection<Part> parts, DateTime fromDate, DateTime toDate)
        {
            var path = Util.GetXlsxPath();
            if (string.IsNullOrEmpty(path)) return "Выбор файла отменен";
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Отчет по операторам");

            var operatorColInd = 1;
            var machineColInd = 2;
            var setupColInd = 3;
            var prodColInd = 4;
            var avrReplTimeColInd = 5;
            var maintenanceTimeColInd = 6;
            var toolSrchTimeColInd = 7;
            var mentoringTimeColInd = 8;
            var contactingDepartsTimeColInd = 9;
            var fixtMakingTimeColInd = 10;
            var hardwFailTimeColInd = 11;
            var specDowntimesColInd = 12;
            ws.Cell(2, operatorColInd).Value = "Оператор";
            ws.Cell(2, machineColInd).Value = "Станок";
            ws.Cell(2, setupColInd).Value = "Наладка";
            ws.Cell(2, prodColInd).Value = "Изготовление";
            ws.Cell(2, avrReplTimeColInd).Value = "Среднее время замены";
            ws.Cell(2, maintenanceTimeColInd).Value = "Обслуживание";
            ws.Cell(2, toolSrchTimeColInd).Value = "Инструмент";
            ws.Cell(2, mentoringTimeColInd).Value = "Обучение";
            ws.Cell(2, contactingDepartsTimeColInd).Value = "Другие службы";
            ws.Cell(2, fixtMakingTimeColInd).Value = "Оснастка";
            ws.Cell(2, hardwFailTimeColInd).Value = "Отказ оборудования";
            ws.Cell(2, specDowntimesColInd).Value = "Простои";
            var lastCol = specDowntimesColInd;
            var headerRange = ws.Range(2, operatorColInd, 2, lastCol);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, setupColInd, 2, lastCol).Style.Alignment.TextRotation = 90;
            var row = 3;
            foreach (var partGroup in parts.GroupBy(p => p.Operator))
            {
                parts = partGroup.ToList();
                ws.Cell(row, operatorColInd).Value = partGroup.Key;
                ws.Cell(row, machineColInd).Value = parts.First().Machine;
                ws.Cell(row, setupColInd).Value = parts.SetupRatio();
                ws.Cell(row, setupColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, prodColInd).Value = parts.ProductionRatio();
                ws.Cell(row, prodColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                ws.Cell(row, avrReplTimeColInd).Value = parts.AverageReplacementTimeRatio();
                ws.Cell(row, avrReplTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Precision2;
                ws.Cell(row, maintenanceTimeColInd).Value = parts.Sum(p => p.MaintenanceTime);
                ws.Cell(row, maintenanceTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, toolSrchTimeColInd).Value = parts.Sum(p => p.ToolSearchingTime);
                ws.Cell(row, toolSrchTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, mentoringTimeColInd).Value = parts.Sum(p => p.MentoringTime);
                ws.Cell(row, mentoringTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, contactingDepartsTimeColInd).Value = parts.Sum(p => p.ContactingDepartmentsTime);
                ws.Cell(row, contactingDepartsTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, fixtMakingTimeColInd).Value = parts.Sum(p => p.FixtureMakingTime);
                ws.Cell(row, fixtMakingTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, hardwFailTimeColInd).Value = parts.Sum(p => p.HardwareFailureTime);
                ws.Cell(row, hardwFailTimeColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.Integer;
                ws.Cell(row, specDowntimesColInd).Value = parts.SpecifiedDowntimesRatio(fromDate, toDate, Types.ShiftType.All);
                ws.Cell(row, specDowntimesColInd).Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger;
                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Columns(5, 11).Group(true);
            ws.Cell(1, 1).Value = $"Отчёт по операторам за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            ws.Range(1, operatorColInd, 1, lastCol).Merge();
            ws.Range(1, operatorColInd, 1, 1).Style.Font.FontSize = 16;
            ws.Columns(3, lastCol).Width = 8;
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                == MessageBoxResult.Yes) Process.Start( new ProcessStartInfo() { UseShellExecute = true, FileName = path});
            return $"Файл сохранен в \"{path}\"";
        }

        public static string ExportDataset(ICollection<Part> parts, DateTime fromDate, DateTime toDate)
        {
            var path = Util.GetXlsxPath();
            if (string.IsNullOrEmpty(path)) return "Выбор файла отменен";
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Экспорт");

            
            ws.Cell(2, 1).Value = "Дата";
            ws.Cell(2, 2).Value = "Смена";
            ws.Cell(2, 3).Value = "Оператор";
            ws.Cell(2, 4).Value = "Деталь";
            ws.Cell(2, 5).Value = "М/Л";
            ws.Cell(2, 6).Value = "Всего по М/Л";
            ws.Cell(2, 7).Value = "Выполнено";
            ws.Cell(2, 8).Value = "Установка";
            ws.Cell(2, 9).Value = "Начало наладки";
            ws.Cell(2, 10).Value = "Начало изготовления";
            ws.Cell(2, 11).Value = "Конец изготовления";
            ws.Cell(2, 12).Value = "Норматив наладки";
            ws.Cell(2, 13).Value = "Фактическая наладка";
            ws.Cell(2, 14).Value = "Норматив штучный";
            ws.Cell(2, 15).Value = "Машинное время";
            ws.Cell(2, 16).Value = "Штучное фактическое";
            ws.Cell(2, 17).Value = "Время замены";
            ws.Cell(2, 18).Value = "Фактическое изготовление";
            ws.Cell(2, 19).Value = "Норматив на партию";
            ws.Cell(2, 20).Value = "Комментарий оператора";
            ws.Cell(2, 21).Value = "Простои в наладке";
            ws.Cell(2, 22).Value = "Простои в изготовлении";
            ws.Cell(2, 23).Value = "Частичная наладка";
            ws.Cell(2, 24).Value = "Обслуживание";
            ws.Cell(2, 25).Value = "Поиск инструмента";
            ws.Cell(2, 26).Value = "Обучение";
            ws.Cell(2, 27).Value = "Другие службы";
            ws.Cell(2, 28).Value = "Изготовление оснастки";
            ws.Cell(2, 29).Value = "Отказ оборудования";
            ws.Cell(2, 30).Value = "Отмеченные простои";
            ws.Cell(2, 31).Value = "Комментарий к простоям";
            ws.Cell(2, 32).Value = "Наладка";
            ws.Cell(2, 33).Value = "Невыполнение норматива наладки";
            ws.Cell(2, 34).Value = "Изготовление";
            ws.Cell(2, 35).Value = "Невыполнение норматива изготовления";
            ws.Cell(2, 36).Value = "Комментарий мастера";
            ws.Cell(2, 37).Value = "Норматив наладки (И)";
            ws.Cell(2, 38).Value = "Норматив изготовления (И)";
            ws.Cell(2, 39).Value = "Комментарий техотдела";
            var headerRange = ws.Range(2, 1, 2, 39);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(2, 1, 2, 34).Style.Alignment.TextRotation = 90;
            var row = 3;
            foreach (var part in parts)
            {
                row++;
            }
            ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            ws.RangeUsed().SetAutoFilter(true);
            ws.Columns().AdjustToContents();
            ws.Columns(5, 11).Group(true);
            ws.Cell(1, 1).Value = $"Экспорт за период с {fromDate.ToString(Constants.ShortDateFormat)} по {toDate.ToString(Constants.ShortDateFormat)}";
            ws.Range(1, 1, 1, 34).Merge();
            ws.Range(1, 1, 1, 1).Style.Font.FontSize = 16;
            ws.Columns(3, 34).Width = 8;
            wb.SaveAs(path);
            if (MessageBox.Show("Открыть сохраненный файл?", "Вопросик", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes) Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = path });
            return $"Файл сохранен в \"{path}\"";
        }
    }
}
