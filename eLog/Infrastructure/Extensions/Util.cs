using ClosedXML.Excel;
using eLog.Models;
using eLog.Views.Windows.Dialogs;
using libeLog;
using libeLog.Extensions;
using libeLog.Infrastructure;
using libeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using static eLog.Infrastructure.Extensions.Text;

namespace eLog.Infrastructure.Extensions;

internal static class Util
{
    
    public enum WriteResult
    {
        Ok, IOError, NotFinded, Error, FileNotExist, DontNeed
    }

    /// <summary>
    /// Получение информации о заказе по номеру М/Л (имитация)
    /// </summary>
    /// <param name="orderNumber">Номер М/Л</param>
    public static List<Part> GetPartsFromOrder(this string orderNumber)
    {
        try
        {
            using var wb = new XLWorkbook(AppSettings.LocalOrdersFile);
            return (from xlRow in wb.Worksheet(1).Rows()
                    where xlRow is { } && xlRow.Cell(1).Value.IsText && xlRow.Cell(1).Value.GetText().Contains(orderNumber.ToUpper())
                    select new Part()
                    {
                        Name = xlRow.Cell(2).Value.GetText() + (xlRow.Cell(3).Value.IsText ? " " + xlRow.Cell(3).Value.GetText() : ""),
                        TotalCount = (int)xlRow.Cell(4).Value.GetNumber(),
                    }).ToList();
        }
        catch (Exception ex1)
        {
            try
            {
                var wb = new XLWorkbook(AppSettings.BackupOrdersFile);
                return (from xlRow in wb.Worksheet(1).Rows()
                        where xlRow is { } && xlRow.Cell(1).Value.IsText && xlRow.Cell(1).Value.GetText().Contains(orderNumber.ToUpper())
                        select new Part()
                        {
                            Name = xlRow.Cell(2).Value.GetText() + (xlRow.Cell(3).Value.IsText ? " " + xlRow.Cell(3).Value.GetText() : ""),
                            TotalCount = (int)xlRow.Cell(4).Value.GetNumber(),
                        }).ToList();
            }
            catch (Exception ex2)
            {
                WriteLog(ex2);
            }
            WriteLog(ex1);

        }
        return new List<Part>();
    }

    public static bool GetBarCode(ref string barCode)
    {
        var dlg = new ReadBarCodeWindow()
        {
            BarCode = string.Empty,
            Owner = Application.Current.MainWindow,
        };
        if (dlg.ShowDialog() != true) return false;
        barCode = dlg.BarCode;
        return true;
    }

    /// <summary>
    /// Получение информации о детали с БД (пока имитация)
    /// </summary>
    /// <param name="barCode">Шрихкод</param>
    /// <returns></returns>
    public static Part GetPartFromBarCode(this string barCode)
    {
        var names = new[] { "Ниппель", "Корпус", "Гайка", "Фланец", "Штуцер", "Седло", "Крышка", "Корпус приводной камеры", "Корпус проточной части" };
        var numbers = new[] { "АР110-01-001", "АР110-01-002", "АР110-01-003", "АР110-01-004", "АР110-01-005", "АРМ2-31.4-02-340-Х6-081-01", "АРМ2-31.4-02-340-Х6-071" };
        var orders = new[] { "УЧ-1/00001.1.1", "УЧ-1/00001.1.2", "УЧ-1/00001.1.3", "УЧ-1/00001.1.4", "УЧ-1/00001.1.5", "УЧ-1/00001.1.6", "УЧ-1/00001.1.7", "УЧ-1/00001.1.8" };

        return new Part(
            names[new Random().Next(0, names.Length)],
            numbers[new Random().Next(0, numbers.Length)],
            (byte)new Random().Next(1, 4),
            orders[new Random().Next(0, orders.Length)],
            new Random().Next(1, 20) * 10,
            new Random().Next(4, 18) * 10,
            new Random().Next(1, 5));
    }

    public static bool ValidXl()
    {
        try
        {
            using var wb = new XLWorkbook(AppSettings.Instance.XlPath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Устанавливает простой "Частичная наладка" на наладку данной детали, если предыдущая деталь была завершена с таким же простоем без выполненных деталей. 
    /// </summary>
    /// <param name="part">Текущая деталь</param>
    /// <returns>Является ли наладка данной детали "доналадкой" после частичной наладки.</returns>
    public static bool SetPartialState(ref Part part, bool update = true)
    {
        var index = AppSettings.Instance.Parts.IndexOf(part);
        var prevPart = index != -1 && AppSettings.Instance.Parts.Count > index + 1 ? AppSettings.Instance.Parts[index + 1] : null;
        if (prevPart is null) return false;
        var partial = part.IsFinished == Part.State.PartialSetup ||
                      prevPart is { IsFinished: Part.State.PartialSetup, FinishedCount: 0 }
                      && part.SetupTimeFact.Ticks > 0
                      && part.FullName == prevPart.FullName
                      && part.Order == prevPart.Order;
        if (partial)
        {
            if (!update) return partial;
            if (part.DownTimes.Any(x => x.Type is not DownTime.Types.PartialSetup))
            {
                part.DownTimes = new DeepObservableCollection<DownTime>(
                    part.DownTimes.Where(downtime => downtime.Type != DownTime.Types.PartialSetup));
                var sortedDowntimes = part.DownTimes.Where(d => d.Relation is DownTime.Relations.Setup).OrderBy(d => d.StartTime).ToList();
                DateTime currentStartTime = part.StartSetupTime;
                foreach (var downtime in sortedDowntimes)
                {
                    if (currentStartTime < downtime.StartTime)
                    {
                        part.DownTimes.Add(new DownTime(part, DownTime.Types.PartialSetup, currentStartTime, downtime.StartTime));
                    }
                    currentStartTime = downtime.EndTime;
                }
                if (currentStartTime < part.StartMachiningTime)
                {
                    part.DownTimes.Add(new DownTime(part, DownTime.Types.PartialSetup, currentStartTime, part.StartMachiningTime));
                }
            } else if (part.DownTimes.Count == 0)
            {
                part.DownTimes.Add(new DownTime(part, DownTime.Types.PartialSetup, part.StartSetupTime, part.StartMachiningTime));
            }

            //part.DownTimes = new DeepObservableCollection<DownTime>(part.DownTimes.Where(x => x.Relation == DownTime.Relations.Machining))
            //{
            //    new DownTime(part, DownTime.Types.PartialSetup)
            //    {

            //        StartTimeText = part.StartSetupTime.ToString(Constants.DateTimeFormat),
            //        EndTimeText = part.StartMachiningTime.ToString(Constants.DateTimeFormat)
            //    }
            //};
        }
        return partial;
    }


    /// <summary>
    /// Запись изготовления в Excel таблицу
    /// </summary>
    /// <param name="part">Информация об изготовлении</param>
    /// <returns>Int32 - Id записи в таблице</returns>
    public static int WriteToXl(this Part part)
    {
        if (AppSettings.Instance.DebugMode) { WriteLog(part, $"Новая запись информации о детали."); }
        var id = -1;
        if (!File.Exists(AppSettings.Instance.XlPath)) return -3;
        var partIndex = AppSettings.Instance.Parts.IndexOf(part);
        var prevPart = partIndex != -1 && AppSettings.Instance.Parts.Count > partIndex + 1 ? AppSettings.Instance.Parts[partIndex + 1] : null;
        try
        {
            if (!BackupXl()) throw new IOException("Ошибка при создании бэкапа таблицы.");
            using (var fs = new FileStream(AppSettings.Instance.XlPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });
                var ws = wb.Worksheet(1);
                ws.LastRowUsed().InsertRowsBelow(1);
                IXLRow? prevRow = null;
                var partial = SetPartialState(ref part, false);
                var combinedDownTimes = part.DownTimes.Combine();
                foreach (var xlRow in ws.Rows())
                {
                    if (xlRow is null) continue;
                    var num = xlRow.Cell(1).Value.IsNumber ? (int)xlRow.Cell(1).Value.GetNumber() : 0;
                    var hasGuidValue = xlRow.Cell(36).Value.IsText;
                    var stringGuid = hasGuidValue ? xlRow.Cell(36).Value.GetText() : "";
                    if (!Guid.TryParse(stringGuid, out Guid guid))
                    {
                        guid = Guid.Empty;
                    }
                    if (guid == part.Guid && part.Guid != Guid.Empty)
                    {
                        if (AppSettings.Instance.DebugMode) { WriteLog($"Найден совпадающий GUID - {part.Guid}\n\tОтмена записи."); }
                        part.Id = num;
                        return -2;
                    }
                    if (id <= num) id = num + 1;
                    if (!xlRow.Cell(6).Value.IsBlank)
                    {
                        prevRow = xlRow;
                        continue;
                    }
                    xlRow.Style = prevRow!.Style;
                    // номер 
                    xlRow.Cell(1).Value = id;
                    // выработка по наладке
                    xlRow.Cell(2).FormulaR1C1 = prevRow.Cell(2).FormulaR1C1;
                    // выработка штучная 1С
                    xlRow.Cell(3).FormulaR1C1 = prevRow.Cell(3).FormulaR1C1;
                    // выработка штучная м/в + 2м
                    xlRow.Cell(4).FormulaR1C1 = prevRow.Cell(4).FormulaR1C1;
                    // комментарий оператора + отчет по простоям
                    xlRow.Cell(5).Value = $"{part.OperatorComments}\n{combinedDownTimes.Report()}".Trim();
                    // если время завершения раньше 07:10, то отнимаем сутки для корректности отчетов
                    var needDiscrease = part.Shift == NightShift && part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(8);
                    // дата смены
                    xlRow.Cell(6).Value = needDiscrease
                        ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                        : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                    // станок
                    xlRow.Cell(7).Value = AppSettings.Instance.Machine.Name;
                    // смена день/ночь
                    xlRow.Cell(8).Value = part.Shift;
                    // оператор
                    xlRow.Cell(9).Value = part.Operator.FullName.Trim();
                    // деталь
                    xlRow.Cell(10).Value = part.FullName;
                    // номер м/л
                    xlRow.Cell(11).Value = part.Order;
                    // количетсво изготовлено
                    xlRow.Cell(12).Value = part.FinishedCount;
                    // номер установки
                    xlRow.Cell(13).Value = part.Setup;
                    // начало наладки
                    xlRow.Cell(14).Value = part.StartSetupTime.ToString("HH:mm");
                    // завешение наладки = начало изготовления
                    xlRow.Cell(15).Value = part.StartMachiningTime.ToString("HH:mm");
                    // фактическое фремя наладки
                    xlRow.Cell(16).Value = partial ? 0 : part.SetupTimeFact.ToString(@"hh\:mm");
                    // норматив на наладку
                    xlRow.Cell(17).Value = part.SetupTimePlan;
                    // фактическое время наладки в минутах
                    xlRow.Cell(18).FormulaR1C1 = prevRow.Cell(18).FormulaR1C1;
                    // разница план/факт наладки
                    xlRow.Cell(19).FormulaR1C1 = prevRow.Cell(19).FormulaR1C1;
                    // завешение наладки = начало изготовления
                    xlRow.Cell(20).Value = part.StartMachiningTime.ToString("HH:mm");
                    // завершение изготовления
                    xlRow.Cell(21).Value = part.EndMachiningTime.ToString("HH:mm");
                    // фактическое время изготовления
                    xlRow.Cell(22).Value = part.ProductionTimeFact.ToString(@"hh\:mm");
                    // норматив штучный
                    xlRow.Cell(23).Value = part.SingleProductionTimePlan;
                    // машинное время
                    xlRow.Cell(24).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                    // вычисления на 1 шт
                    for (var i = 25; i <= 33; i++)
                    {
                        xlRow.Cell(i).FormulaR1C1 = prevRow.Cell(i).FormulaR1C1;
                    }

                    var shiftTime = part.Shift == Text.DayShift ? 660 : 630;
                    // все простои в наладке кроме частичной
                    xlRow.Cell(34).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                    // все простои в изготовлении
                    xlRow.Cell(35).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0);
                    // guid
                    xlRow.Cell(36).Value = part.Guid.ToString();
                    var partialSetupTime = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                    // частичная наладка
                    xlRow.Cell(37).Value = partialSetupTime > shiftTime ? shiftTime : partialSetupTime;
                    // обслуживание
                    xlRow.Cell(38).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Maintenance }).TotalMinutes(), 0);
                    // поиск и получение инструмента
                    xlRow.Cell(39).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ToolSearching }).TotalMinutes(), 0);
                    // обучение и все такое
                    xlRow.Cell(40).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Mentoring }).TotalMinutes(), 0);
                    // обращение в другие службы
                    xlRow.Cell(41).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ContactingDepartments }).TotalMinutes(), 0);
                    // изготовление оснастки
                    xlRow.Cell(42).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.FixtureMaking }).TotalMinutes(), 0);
                    // отказ оборудования
                    xlRow.Cell(43).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.HardwareFailure }).TotalMinutes(), 0);
                    // если текущая деталь уже записывалась, то норматив наладки 0, чтобы они не суммировались при подсчете выработки
                    var partSetupTimePlanReport = prevPart != null && prevPart.Order == part.Order && prevPart.Setup == part.Setup ? 0 : part.SetupTimePlan;
                    if (partSetupTimePlanReport == 0 && part.SetupTimeFact.TotalMinutes > 0) partSetupTimePlanReport = part.SetupTimeFact.TotalMinutes;
                    if (partSetupTimePlanReport == 0 && part.SetupTimePlan == 0)
                    {
                        var partialTime = part.DownTimes.Where(x => x.Type == DownTime.Types.PartialSetup).TotalMinutes();
                        if (partialTime > 0) partSetupTimePlanReport = partialTime;
                    }
                    // норматив наладки для отчета
                    xlRow.Cell(44).Value = partSetupTimePlanReport;
                    // норматив наладки для исправления
                    xlRow.Cell(45).Value = part.SetupTimePlan;
                    // норматив изготовления для исправления
                    xlRow.Cell(46).Value = part.SingleProductionTimePlan;

                    for (var i = 1; i <= 46; i++)
                    {
                        xlRow.Cell(i).Style = prevRow.Cell(i).Style;
                    }
                    break;
                }
                if (AppSettings.Instance.DebugMode) { WriteLog($"Присвоен номер {id}. Запись в файл..."); }
                Debug.Print("Write");
                wb.Save(true);
                if (AppSettings.Instance.DebugMode) { WriteLog($"Записано."); }
                Debug.Print("Ok");
            }
        }
        catch (IOException ioEx)
        {
            Debug.Print("IOError");
            if (AppSettings.Instance.DebugMode) { WriteLog(ioEx); }
            return -4;
        }
        catch (KeyNotFoundException keyNotFoundEx)
        {
            WriteLog(keyNotFoundEx, $"(KeyNotFoundException) Не удалось открыть XL файл для записи детали {part.FullName} по заказу {part.Order}.\n\tОператор - {part.Operator.FullName}");
            Debug.Print("KeyNotFoundException");
            TryRestoreXl();
        }
        catch (OutOfMemoryException outOfMemEx)
        {
            WriteLog(outOfMemEx, $"Нехватка оперативной памяти при работе с XL файлом для записи детали {part.FullName} по заказу {part.Order}.\n\tОператор - {part.Operator.FullName}");
            Debug.Print("OutOfMemoryException");
            TryRestoreXl();
        }
        catch (ArgumentException argEx)
        {
            if (AppSettings.Instance.DebugMode) { WriteLog(argEx); }
            MessageBox.Show($"{argEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception e)
        {
            WriteLog(e, $"Ошибка при записи детали {part.FullName} по заказу {part.Order}.\n\tОператор - {part.Operator.FullName}");
        }
        if (!ValidXl())
        {
            WriteLog("Файл невалидный, требуется восстановление.");
            TryRestoreXl();
            part.Id = -1;
            return -1;
        }
        return id;
    }

    public static WriteResult RewriteToXl(this Part part, bool doBackup = true)
    {
        var partIndex = AppSettings.Instance.Parts.IndexOf(part);
        var prevPart = partIndex != -1 && AppSettings.Instance.Parts.Count > partIndex + 1 ? AppSettings.Instance.Parts[partIndex + 1] : null;

        if (AppSettings.Instance.DebugMode) { WriteLog(part, $"Обновление информации о детали."); }

        if (AppSettings.Instance.DebugMode && prevPart != null) { WriteLog(prevPart, $"Прошлая деталь."); }

        if (!File.Exists(AppSettings.Instance.XlPath))
        {
            if (AppSettings.Instance.DebugMode) { WriteLog($"Путь к таблице не существует."); }
            return WriteResult.FileNotExist;
        }
        if (part.IsFinished == Part.State.InProgress)
        {
            if (AppSettings.Instance.DebugMode) { WriteLog($"Изготовление в процессе, запись не требуется."); }
            return WriteResult.DontNeed;
        }
        var result = WriteResult.NotFinded;
        try
        {
            //Thread.Sleep(5000);
            var partial = SetPartialState(ref part, false);
            //Thread.Sleep(5000);
            if (doBackup)
            {
                if (!BackupXl())
                {
                    throw new IOException("Ошибка при создании бэкапа таблицы.");
                }
            }
            
            using (var fs = new FileStream(AppSettings.Instance.XlPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });


                var combinedDownTimes = part.DownTimes.Combine();
                if (AppSettings.Instance.DebugMode) { WriteLog($"Поиск позиции..."); }
                foreach (var xlRow in wb.Worksheet(1).Rows())
                {
                    var rowWithPart = xlRow.Cell(1).Value.IsNumber;
                    var rowNum = rowWithPart ? (int)xlRow.Cell(1).Value.GetNumber() : 0;
                    var hasGuidValue = xlRow.Cell(36).Value.IsText;
                    var stringGuid = hasGuidValue ? xlRow.Cell(36).Value.GetText() : "";
                    if (!Guid.TryParse(stringGuid, out Guid guid))
                    {
                        guid = Guid.Empty;
                    }
                    if (!rowWithPart || (rowNum == part.Id && guid == Guid.Empty) || guid != part.Guid) continue;
                    if (AppSettings.Instance.DebugMode) { WriteLog($"Позиция найдена на строке №{rowNum}."); }
                    xlRow.Cell(5).Value = $"{part.OperatorComments}\n{combinedDownTimes.Report()}".Trim();
                    var needDiscrease = part.Shift == NightShift && part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(8);
                    xlRow.Cell(6).Value = needDiscrease
                        ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                        : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                    xlRow.Cell(7).Value = AppSettings.Instance.Machine.Name;
                    xlRow.Cell(8).Value = part.Shift;
                    xlRow.Cell(9).Value = part.Operator.FullName.Trim();
                    xlRow.Cell(10).Value = part.FullName;
                    xlRow.Cell(11).Value = part.Order;
                    xlRow.Cell(12).Value = part.FinishedCount;
                    xlRow.Cell(13).Value = part.Setup;
                    xlRow.Cell(14).Value = part.StartSetupTime.ToString("HH:mm");
                    xlRow.Cell(15).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(16).Value = partial ? 0 : part.SetupTimeFact.ToString(@"hh\:mm");
                    xlRow.Cell(17).Value = part.SetupTimePlan;
                    xlRow.Cell(20).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(21).Value = part.EndMachiningTime.ToString("HH:mm");
                    xlRow.Cell(22).Value = part.ProductionTimeFact.ToString(@"hh\:mm");
                    xlRow.Cell(23).Value = part.SingleProductionTimePlan;
                    xlRow.Cell(24).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                    var shiftTime = part.Shift == Text.DayShift ? 660 : 630;
                    xlRow.Cell(34).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                    xlRow.Cell(35).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0);
                    xlRow.Cell(36).Value = part.Guid.ToString();
                    var partialSetupTime = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                    xlRow.Cell(37).Value = partialSetupTime > shiftTime ? shiftTime : partialSetupTime;
                    xlRow.Cell(38).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Maintenance }).TotalMinutes(), 0);
                    xlRow.Cell(39).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ToolSearching }).TotalMinutes(), 0);
                    xlRow.Cell(40).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.Mentoring }).TotalMinutes(), 0);
                    xlRow.Cell(41).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.ContactingDepartments }).TotalMinutes(), 0);
                    xlRow.Cell(42).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.FixtureMaking }).TotalMinutes(), 0);
                    xlRow.Cell(43).Value = Math.Round(part.DownTimes.Where(x => x is { Type: DownTime.Types.HardwareFailure }).TotalMinutes(), 0);
                    var partSetupTimePlanReport = prevPart != null && prevPart.Order == part.Order && prevPart.Setup == part.Setup ? 0 : part.SetupTimePlan;
                    if (partSetupTimePlanReport == 0 && part.SetupTimeFact.TotalMinutes > 0) partSetupTimePlanReport = part.SetupTimeFact.TotalMinutes;
                    if (partSetupTimePlanReport == 0 && part.SetupTimePlan == 0)
                    {
                        var partialTime = part.DownTimes.Where(x => x.Type == DownTime.Types.PartialSetup).TotalMinutes();
                        if (partialTime > 0) partSetupTimePlanReport = partialTime;
                    }
                    xlRow.Cell(44).Value = partSetupTimePlanReport;
                    xlRow.Cell(45).Value = part.SetupTimePlan;
                    xlRow.Cell(46).Value = part.SingleProductionTimePlan;
                    result = WriteResult.Ok;
                    if (AppSettings.Instance.DebugMode) { WriteLog($"Запись в файл..."); }
                    Debug.Print("Rewrite");
                    wb.Save(true);
                    Debug.Print("Ok");
                    if (AppSettings.Instance.DebugMode) { WriteLog($"Записано."); }
                    break;
                }
                if (AppSettings.Instance.DebugMode && result is WriteResult.NotFinded) { WriteLog($"Деталь не найдена."); }
            }
        }

        catch (IOException ioEx)
        {
            if (AppSettings.Instance.DebugMode) { WriteLog(ioEx); }
            return WriteResult.IOError;
        }
        catch (KeyNotFoundException keyNotFoundEx)
        {
            WriteLog(keyNotFoundEx, $"(KeyNotFoundException) Не удалось открыть XL файл для записи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
            Debug.Print("KeyNotFoundException");
            TryRestoreXl();
            return WriteResult.Error;
        }
        catch (OutOfMemoryException outOfMemEx)
        {
            WriteLog(outOfMemEx, $"Нехватка оперативной памяти при работе с XL файлом для записи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
            Debug.Print("OutOfMemoryException");
            TryRestoreXl();
            return WriteResult.Error;
        }
        catch (Exception e)
        {
            Debug.Print("Необработанное исключение");
            WriteLog(e, $"Ошибка при перезаписи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
            TryRestoreXl();
            return WriteResult.Error;
        }
        if (!ValidXl())
        {
            WriteLog("Файл невалидный, требуется восстановление.");
            TryRestoreXl();
            return WriteResult.Error;
        }
        return result;
    }

    private static bool BackupXl(int count = 3)
    {
        for (int i = 1; i <= count; i++)
        {
            if (AppSettings.Instance.DebugMode) WriteLog($"Попытка бэкапа таблицы...{i}");
            try
            {
                using var wb = new XLWorkbook(AppSettings.Instance.XlPath, new LoadOptions() { RecalculateAllFormulas = false });
                File.Copy(AppSettings.Instance.XlPath, AppSettings.XlReservedPath, true);
                if (AppSettings.Instance.DebugMode) WriteLog("Успешно.");
                Thread.Sleep(200);
                return true;
            }
            catch
            {
                Thread.Sleep(5000);
            }
        }
        return false;

    }

    private static void TryRestoreXl()
    {
        try
        {
            Debug.Print("Restore");
            WriteLog("Попытка восстановления XL файла...");
            File.Copy(AppSettings.XlReservedPath, AppSettings.Instance.XlPath, true);
            WriteLog("Успешно.");
            Debug.Print("Ok");
        }
        catch (Exception ex)
        {
            Debug.Print("Failed");
            WriteLog(ex, "Неудачно.");
        }
    }


    /// <summary>
    /// Получает директорию для копирования логов, которой является директория таблицы.
    /// </summary>
    /// <returns></returns>
    private static string GetCopyDir()
    {
        if (File.Exists(AppSettings.Instance.XlPath) && Directory.GetParent(AppSettings.Instance.XlPath) is { Exists: true} parent)
        {
            return parent.FullName;
        }
        return "";
    }

    /// <summary>
    /// Записывает информацию об исключении в лог.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="additionMessage"></param>
    public static void WriteLog(Exception exception, string additionMessage = "") 
        => _ = Logs.Write(AppSettings.LogFile, exception, additionMessage, GetCopyDir());


    /// <summary>
    /// Записывает сообщение в лог.
    /// </summary>
    /// <param name="message"></param>
    public static void WriteLog(string message)
        => _ = Logs.Write(AppSettings.LogFile, message, GetCopyDir());


    /// <summary>
    /// Записывает информацию о детали в лог.
    /// </summary>
    /// <param name="part"></param>
    /// <param name="message"></param>
    public static void WriteLog(Part part, string message)
    {
        var content = $"[{DateTime.Now.ToString(Constants.DateTimeWithSecsFormat)}]: {message}\n\t" +
                    $"Оператор: {AppSettings.Instance.CurrentOperator?.DisplayName}\n\t" +
                    $"Деталь №{part.Id}: {part.Name} | {part.Setup} уст.\n\t" +
                    $"М/Л: {part.Order} | {part.TotalCountInfo}\n\t" +
                    $"GUID: {part.Guid}\n\n";
        _ = Logs.Write(AppSettings.LogFile, content, GetCopyDir());
    }


    /// <summary>
    /// Список простоев с временами.
    /// </summary>
    /// <param name="downTimes"></param>
    /// <returns></returns>
    public static string Report(this IEnumerable<DownTime> downTimes) => downTimes.Any()
        ? downTimes.Aggregate("Отмеченные простои:\n", (current, downTime) => current + $"{downTime.Name}: {Math.Round(downTime.Time.TotalMinutes, 1)} м\n")
        : string.Empty;

    public static string Report(this IEnumerable<CombinedDownTime> downTimes) => downTimes.Any()
        ? downTimes.Aggregate("Отмеченные простои:\n", (current, downTime) => current + $"{downTime.Description}")
        : string.Empty;

    public static double TotalMinutes(this IEnumerable<DownTime> downTimes) =>
        downTimes.Aggregate(0.0, (sum, downTime) => sum + downTime.Time.TotalMinutes);

    public static double TotalMinutes(this IEnumerable<CombinedDownTime> downTimes) =>
        downTimes.Aggregate(0.0, (sum, downTime) => sum + downTime.Time.TotalMinutes);

    public static IEnumerable<CombinedDownTime> Combine(this ICollection<DownTime> downTimes)
    {
        if (downTimes == null || downTimes.Count < 1)
        {
            return new List<CombinedDownTime>();
        }

        var groupedDownTimes = downTimes.GroupBy(x => new { x.Type, x.Relation });

        return groupedDownTimes.Select(group =>
        {
            var combinedDownTime = new CombinedDownTime(group.ToList())
            {
                Type = group.Key.Type,
                Relation = group.Key.Relation
            };
            return combinedDownTime;
        });
    }




    public static DateTime GetStartShiftTime()
    {
        return AppSettings.Instance.CurrentShift == Text.DayShift ? DateTime.Today.AddHours(7) :
            DateTime.Now.Hour < 7 ? DateTime.Today.AddDays(-1).AddHours(19) : DateTime.Today.AddHours(19);
    }

    public static DateTime GetEndShiftTime()
    {
        return AppSettings.Instance.CurrentShift == Text.DayShift
            ? DateTime.Today.AddHours(19)
            : DateTime.Now.Hour < 8
                ? DateTime.Today.AddHours(7)
                : DateTime.Today.AddDays(1).AddHours(7);
    }

    public static void AddDownTime(this Part part, DownTime.Types type)
    {
        part.DownTimes.Add(new DownTime(part, type));
    }


}
