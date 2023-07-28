using eLog.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using ClosedXML;
using eLog.Infrastructure.Extensions.Windows;
using eLog.Views.Windows.Dialogs;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using static eLog.Infrastructure.Extensions.Text;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;

namespace eLog.Infrastructure.Extensions
{
    internal static class Util
    {
        public enum WriteResult
        {
            Ok, IOError, NotFinded, Error, FileNotExist, DontNeed
        }
        /// <summary>
        /// Капитализация строки (для имен, фамилий и тд)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Capitalize(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            var letters = source.ToCharArray();
            for (int i = 0; i < letters.Length; i++)
            {
                if(i == 0)
                {
                    letters[i] = char.ToUpper(letters[i]);
                }
                else
                {
                    letters[i] = char.ToLower(letters[i]);
                }
            }
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        /// <summary>
        /// Получение информации о заказе по номеру М/Л (имитация)
        /// </summary>
        /// <param name="orderNumber">Номер М/Л</param>
        public static List<Part> GetPartsFromOrder(this string orderNumber)
        {
            try
            {
                var wb = new XLWorkbook(AppSettings.LocalOrdersFile);
                return (from xlRow in wb.Worksheet(1).Rows() 
                    where xlRow is { } && xlRow.Cell(1).Value.IsText && xlRow.Cell(1).Value.GetText().Contains(orderNumber.ToUpper()) 
                    select new Part() { Name = xlRow.Cell(4).Value.GetText(), TotalCount = (int)xlRow.Cell(5).Value.GetNumber(), }).ToList();
            }
            catch (Exception ex1)
            {
                try
                {
                    var wb = new XLWorkbook(AppSettings.BackupOrdersFile);
                    return (from xlRow in wb.Worksheet(1).Rows()
                        where xlRow is { } && xlRow.Cell(1).Value.IsText && xlRow.Cell(1).Value.GetText().Contains(orderNumber.ToUpper())
                        select new Part() { Name = xlRow.Cell(4).Value.GetText(), TotalCount = (int)xlRow.Cell(5).Value.GetNumber(), }).ToList();
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message);
                }
                MessageBox.Show(ex1.Message);
                
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
            var names = new[] {"Ниппель", "Корпус", "Гайка", "Фланец", "Штуцер", "Седло", "Крышка", "Корпус приводной камеры", "Корпус проточной части" };
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
                _ = new XLWorkbook(AppSettings.Instance.XlPath);
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
        public static bool SetPartialState(ref Part part)
        {
            var index = AppSettings.Instance.Parts.IndexOf(part);
            var prevPart = index != -1 && AppSettings.Instance.Parts.Count > index + 1 ? AppSettings.Instance.Parts[index + 1] : null;
            var partial = part.IsFinished == Part.State.PartialSetup ||
                          prevPart is { IsFinished: Part.State.PartialSetup, FinishedCount: 0 };
            if (partial && part.SetupTimeFact.Ticks > 0)
            {
                part.DownTimes = new DeepObservableCollection<DownTime>(part.DownTimes.Where(x => x.Relation == DownTime.Relations.Machining))
                {
                    new DownTime(part, DownTime.Types.PartialSetup)
                    {

                        StartTimeText = part.StartSetupTime.ToString(Text.DateTimeFormat),
                        EndTimeText = part.StartMachiningTime.ToString(Text.DateTimeFormat)
                    }
                };
            }
            return partial;
        }


        /// <summary>
        /// Запись изготовления в Excel таблицу
        /// </summary>
        /// <param name="part">Информация об изготовлении</param>
        /// <returns>Int32 - Id записи в таблице</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int WriteToXl(this Part part)
        {
            var id = -1;
            if (!File.Exists(AppSettings.Instance.XlPath)) return id;
            try
            {
                if (!BackupXl()) throw new IOException("Ошибка при создании бэкапа таблицы.");
                using (var fs = new FileStream(AppSettings.Instance.XlPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });
                    var ws = wb.Worksheet(1);
                    ws.LastRowUsed().InsertRowsBelow(1);
                    IXLRow? prevRow = null;
                    var partial = SetPartialState(ref part);
                    var combinedDownTimes = part.DownTimes.Combine();
                    foreach (var xlRow in ws.Rows())
                    {
                        if (xlRow is null) continue;
                        var num = xlRow.Cell(1).Value.IsNumber ? (int)xlRow.Cell(1).Value.GetNumber() : 0;
                        if (id <= num) id = num + 1;
                        if (!xlRow.Cell(6).Value.IsBlank)
                        {
                            prevRow = xlRow;
                            continue;
                        }
                        xlRow.Style = prevRow!.Style;
                        xlRow.Cell(1).Value = id;
                        xlRow.Cell(2).FormulaR1C1 = prevRow.Cell(2).FormulaR1C1;
                        xlRow.Cell(3).FormulaR1C1 = prevRow.Cell(3).FormulaR1C1;
                        xlRow.Cell(4).FormulaR1C1 = prevRow.Cell(4).FormulaR1C1;
                        xlRow.Cell(5).Value = $"{part.OperatorComments}\n{combinedDownTimes.Report()}".Trim();
                        // если время завершения раньше 07:10, то отнимаем сутки для корректности отчетов
                        //xlRow.Cell(6).Value = part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(7).AddMinutes(10)
                        //    ? part.EndMachiningTime.AddDays(-1).ToString("dd.MM.yyyy")
                        //    : part.EndMachiningTime.ToString("dd.MM.yyyy");
                        xlRow.Cell(6).Value = part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(7).AddMinutes(10)
                            ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                            : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                        xlRow.Cell(7).Value = AppSettings.Instance.Machine.Name;
                        xlRow.Cell(8).Value = part.Operator.FullName.Trim();
                        xlRow.Cell(9).Value = part.FullName;
                        xlRow.Cell(10).Value = part.Order;
                        xlRow.Cell(11).Value = part.FinishedCount;
                        xlRow.Cell(12).Value = part.Setup;
                        xlRow.Cell(13).Value = part.StartSetupTime.ToString("HH:mm");
                        xlRow.Cell(14).Value = part.StartMachiningTime.ToString("HH:mm");
                        xlRow.Cell(15).Value = partial ? 0 : part.SetupTimeFact.ToString(@"hh\:mm");
                        xlRow.Cell(16).Value = part.SetupTimePlan;
                        xlRow.Cell(17).FormulaR1C1 = prevRow.Cell(17).FormulaR1C1;
                        xlRow.Cell(18).FormulaR1C1 = prevRow.Cell(18).FormulaR1C1;
                        xlRow.Cell(19).Value = part.StartMachiningTime.ToString("HH:mm");
                        xlRow.Cell(20).Value = part.EndMachiningTime.ToString("HH:mm");
                        xlRow.Cell(21).Value = part.ProductionTimeFact.ToString(@"hh\:mm");
                        xlRow.Cell(22).Value = part.SingleProductionTimePlan;
                        xlRow.Cell(23).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                        for (var i = 24; i <= 32; i++)
                        {
                            xlRow.Cell(i).FormulaR1C1 = prevRow.Cell(i).FormulaR1C1;
                        }

                        var setupDownTimes = part.DownTimes.Any(x => x is
                        { Relation: DownTime.Relations.Setup, Type: DownTime.Types.PartialSetup })
                            ? part.DownTimes.First(x => x.Type is DownTime.Types.PartialSetup).Time.TotalMinutes
                            : Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                        var shiftTime = AppSettings.Instance.CurrentShift == Text.DayShift ? 660 : 630;
                        xlRow.Cell(33).Value = setupDownTimes > shiftTime ? shiftTime : setupDownTimes;
                        xlRow.Cell(34).Value = part.Shift;
                        xlRow.Cell(35).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0);
                        for (var i = 1; i <= 35; i++)
                        {
                            xlRow.Cell(i).Style = prevRow.Cell(i).Style;
                        }

                        break;
                    }
                    wb.Save(true);
                }

            }
            catch (IOException)
            {
                
            }
            catch (KeyNotFoundException)
            {
                WriteLog($"Не удалось открыть XL файл для записи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Неверно указан путь к таблице.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                WriteLog(e, $"   Ошибка при записи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
                TryCopyLog();
            }
            return id;
        }

        private static bool BackupXl()
        {
            try
            {
                using (var wb = new XLWorkbook(AppSettings.Instance.XlPath, new LoadOptions() { RecalculateAllFormulas = false }))
                {
                    File.Copy(AppSettings.Instance.XlPath, AppSettings.XlReservedPath, true);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static WriteResult RewriteToXl(this Part part)
        {
            if (!File.Exists(AppSettings.Instance.XlPath)) return WriteResult.FileNotExist;
            if (part.IsFinished == Part.State.InProgress) return WriteResult.DontNeed;
            var result = WriteResult.NotFinded;
            try
            {
                var partial = SetPartialState(ref part);

                if (!BackupXl()) throw new IOException("Ошибка при создании бэкапа таблицы.");

                using (var fs = new FileStream(AppSettings.Instance.XlPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });
                    
                    
                    var combinedDownTimes = part.DownTimes.Combine();

                    foreach (var xlRow in wb.Worksheet(1).Rows())
                    {
                        if (xlRow.Cell(1).Value.IsNumber && xlRow.Cell(1).Value.GetNumber() is { } numb && numb >= 1620) {
                            Debug.Print(numb.ToString());
                        }
                        if (!xlRow.Cell(1).Value.IsNumber || (int)xlRow.Cell(1).Value.GetNumber() != part.Id || xlRow.Cell(10).Value.GetText() != part.Order) continue;
                        xlRow.Cell(5).Value = $"{part.OperatorComments}\n{combinedDownTimes.Report()}".Trim();
                        // если время завершения раньше 07:10, то отнимаем сутки для корректности отчетов
                        //xlRow.Cell(6).Value = part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(7).AddMinutes(10)
                        //    ? part.EndMachiningTime.AddDays(-1).ToString("dd.MM.yyyy")
                        //    : part.EndMachiningTime.ToString("dd.MM.yyyy");
                        xlRow.Cell(6).Value = part.EndMachiningTime < new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddHours(7).AddMinutes(10)
                            ? new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day).AddDays(-1)
                            : new DateTime(part.EndMachiningTime.Year, part.EndMachiningTime.Month, part.EndMachiningTime.Day);
                        xlRow.Cell(7).Value = AppSettings.Instance.Machine.Name;
                        xlRow.Cell(8).Value = part.Operator.FullName.Trim();
                        xlRow.Cell(9).Value = part.FullName;
                        xlRow.Cell(10).Value = part.Order;
                        xlRow.Cell(11).Value = part.FinishedCount;
                        xlRow.Cell(12).Value = part.Setup;
                        xlRow.Cell(13).Value = part.StartSetupTime.ToString("HH:mm");
                        xlRow.Cell(14).Value = part.StartMachiningTime.ToString("HH:mm");
                        xlRow.Cell(15).Value = partial ? 0 : part.SetupTimeFact.ToString(@"hh\:mm");
                        xlRow.Cell(16).Value = part.SetupTimePlan;
                        xlRow.Cell(19).Value = part.StartMachiningTime.ToString("HH:mm");
                        xlRow.Cell(20).Value = part.EndMachiningTime.ToString("HH:mm");
                        xlRow.Cell(21).Value = part.ProductionTimeFact.ToString(@"hh\:mm");
                        xlRow.Cell(22).Value = part.SingleProductionTimePlan;
                        xlRow.Cell(23).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                        var setupDownTimes = part.DownTimes.Any(x => x is
                        { Relation: DownTime.Relations.Setup, Type: DownTime.Types.PartialSetup })
                            ? part.DownTimes.First(x => x.Type is DownTime.Types.PartialSetup).Time.TotalMinutes
                            : Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0);

                        var shiftTime = AppSettings.Instance.CurrentShift == Text.DayShift ? 660 : 630;
                        xlRow.Cell(33).Value = setupDownTimes > shiftTime ? shiftTime : setupDownTimes;
                        //xlRow.Cell(33).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Setup, Type: not DownTime.Types.PartialSetup }).TotalMinutes(), 0);
                        xlRow.Cell(34).Value = part.Shift;
                        xlRow.Cell(35).Value = Math.Round(part.DownTimes.Where(x => x is { Relation: DownTime.Relations.Machining }).TotalMinutes(), 0);
                        result = WriteResult.Ok;
                        break;
                    }
                    wb.Save(true);
                }
            }
            catch (IOException)
            {
                return WriteResult.IOError;
            }
            catch (KeyNotFoundException)
            {
                WriteLog($"Не удалось открыть XL файл для записи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
                return WriteResult.Error;
            }
            catch (Exception e)
            {
                WriteLog(e, $"   Ошибка при перезаписи детали {part.FullName} по заказу {part.Order}.\n   Оператор - {part.Operator.FullName}");
                TryCopyLog();
                return WriteResult.Error;
            }
            return result;
        }

        public static void WriteLog(Exception exception, string additionMessage = "")
        {
            try
            {
                File.AppendAllText(AppSettings.LogFile, $"[{DateTime.Now.ToString(Text.DateTimeFormat)}]: " +
                                                        $"{(string.IsNullOrEmpty(additionMessage) ? string.Empty : $"{additionMessage}\n")}" +
                                                        $"{exception.Message}{(exception.TargetSite is null ? string.Empty : $"\n   Caller: {exception.TargetSite}")}\n" +
                                                        $"{exception.GetType()}\n" +
                                                        $"{exception.StackTrace}\n\n");
                if (!string.IsNullOrWhiteSpace(AppSettings.Instance.XlPath)) TryCopyLog();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Отправь этот текст разработчику:\n{e.GetBaseException()}", $"{e.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(AppSettings.LogFile, $"[{DateTime.Now.ToString(DateTimeFormat)}]: {message}\n\n");
                if (!string.IsNullOrWhiteSpace(AppSettings.Instance.XlPath)) TryCopyLog();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Отправь этот текст разработчику:\n{e.GetBaseException()}", $"{e.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void TryCopyLog()
        {
            if (Directory.GetParent(AppSettings.Instance.XlPath) is not { Exists: true } parent) return;
            var logsPath = Path.Combine(parent.FullName, "logs");
            if (!Directory.Exists(logsPath)) { Directory.CreateDirectory(logsPath); }
            File.Copy(AppSettings.LogFile, Path.Combine(logsPath, $"{Environment.UserName}.log"), true);

        }


        /// <summary>
        /// Округляет DateTime с точностью до минут
        /// </summary>
        /// <param name="value">Исходное время</param>
        /// <returns></returns>
        public static DateTime Rounded(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        /// <summary>
        /// Парсит строку в TimeSpan
        /// </summary>
        /// <param name="input">Строка с вводом</param>
        /// <param name="time">Спарсеный промежуток времени</param>
        /// <returns></returns>
        public static bool TimeParse(this string input, out TimeSpan time)
        {
            if (string.IsNullOrEmpty(input))
            {
                time = TimeSpan.Zero;
                return false;
            }
            input = input.Replace(",", ".");
            if (input.Length > 0)
            {
                if (input.Count(x => x == ':') == 1)
                {
                    var sTime = input.Split(':');
                    if (double.TryParse(sTime[0], out var minutes) && 
                        double.TryParse(sTime[1], out var seconds))
                    {
                        time = TimeSpan.FromSeconds(minutes * 60 + seconds);
                        return true;
                    }
                }
                else if (input.Count(x => x == ':') == 2)
                {
                    var sTime = input.Split(':');
                    if (double.TryParse(sTime[0], out var hours) && 
                        double.TryParse(sTime[1], out var minutes) &&
                        double.TryParse(sTime[2], out var seconds))
                    {
                        time = TimeSpan.FromSeconds(hours * 3600 + minutes * 60 + seconds);
                        return true;
                    }
                }
                else
                {
                    if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var minutes))
                    {
                        time = TimeSpan.FromMinutes(minutes);
                        return true;
                    }
                    time = TimeSpan.Zero;
                    return false;
                }
            }
            time = TimeSpan.Zero;
            return false;
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
            ? downTimes.Aggregate("Отмеченные простои:\n", (current, downTime) => current + $"{downTime.Name}: {Math.Round(downTime.Time.TotalMinutes, 1)} м\n") 
            : string.Empty;

        public static double TotalMinutes(this IEnumerable<DownTime> downTimes) =>
            downTimes.Aggregate(0.0, (sum, downTime) => sum + downTime.Time.TotalMinutes);

        public static double TotalMinutes(this IEnumerable<CombinedDownTime> downTimes) =>
            downTimes.Aggregate(0.0, (sum, downTime) => sum + downTime.Time.TotalMinutes);

        public static IEnumerable<CombinedDownTime> Combine(this ICollection<DownTime> downTimes)
        {
            return downTimes
                .GroupBy(s => new { s.Name, s.Type, s.Relation })
                .Select(g => new CombinedDownTime()
                {
                    Name = (g.Key.Relation is DownTime.Relations.Setup ? "(н) " : "(и) ") + g.Key.Name,
                    Type = g.Key.Type,
                    Relation = g.Key.Relation,
                    Time = new TimeSpan(g.Sum(d => d.Time.Ticks))
                }).OrderBy(x => x.Relation);
        }

        public enum GetNumberOption { Any, OnlyPositive }

        /// <summary>
        /// Получает число из строки
        /// </summary>
        /// <param name="stringNumber">Строка для получения</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <param name="numberOption">Возвращаемое значение: Any для любого, OnlyPositive для положительных</param>
        /// <returns>Значение Double, при неудаче возвращает значение по умолчанию</returns>
        public static double GetDouble(this string stringNumber, double defaultValue = 0, GetNumberOption numberOption = GetNumberOption.OnlyPositive)
        {
            //if (stringNumber is "-") return double.NegativeInfinity;
            NumberFormatInfo numberFormat = new() { NumberDecimalSeparator = "," };
            if (!double.TryParse(stringNumber, NumberStyles.Any, numberFormat, out var result)) return defaultValue;
            return numberOption switch
            {
                GetNumberOption.OnlyPositive when result >= 0 => result,
                GetNumberOption.Any => result,
                _ => defaultValue
            };
        }

        /// <summary>
        /// Получает число из строки
        /// </summary>
        /// <param name="stringNumber">Строка для получения</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <param name="numberOption">Возвращаемое значение: только положительное или любое</param>
        /// <returns>Значение Int32, при неудаче возвращает значение по умолчанию</returns>
        public static int GetInt(this string stringNumber, int defaultValue = 0, GetNumberOption numberOption = GetNumberOption.OnlyPositive)
        {
            NumberFormatInfo numberFormat = new() { NumberDecimalSeparator = "," };
            if (!int.TryParse(stringNumber, NumberStyles.Any, numberFormat, out var result)) return defaultValue;
            if (numberOption == GetNumberOption.OnlyPositive && result > 0)
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Суммарное время перерывов между двумя датами.
        /// </summary>
        /// <param name="startDateTime">Начало</param>
        /// <param name="endDateTime">Завершение</param>
        /// <param name="calcOnEnd">Считать по времени завершения перерывов. Это вариант по-умолчанию для выполняемых действий, по началам перерывов считается при расчете планируемого времени.</param>
        /// <returns>TimeSpan с суммарным временем перерывов</returns>
        public static TimeSpan GetBreaksBetween(DateTime startDateTime, DateTime endDateTime, bool calcOnEnd = true)
        {
            var dayShiftFirstBreak = WorkTime.DayShiftFirstBreak;
            var dayShiftSecondBreak = WorkTime.DayShiftSecondBreak;
            var dayShiftThirdBreak = WorkTime.DayShiftThirdBreak;
            var nightShiftFirstBreak = WorkTime.NightShiftFirstBreak;
            var nightShiftSecondBreak = WorkTime.NightShiftSecondBreak;
            var nightShiftThirdBreak = WorkTime.NightShiftThirdBreak;

            if (!calcOnEnd)
            {
                dayShiftFirstBreak = dayShiftFirstBreak.AddMinutes(-15);
                dayShiftSecondBreak = dayShiftSecondBreak.AddMinutes(-30);
                dayShiftThirdBreak = dayShiftThirdBreak.AddMinutes(-15);
                nightShiftFirstBreak = nightShiftFirstBreak.AddMinutes(-30);
                nightShiftSecondBreak = nightShiftSecondBreak.AddMinutes(-30);
                nightShiftThirdBreak = dayShiftThirdBreak.AddMinutes(-30);
            }

            var breaks = TimeSpan.Zero;
            var startTime = new DateTime(1,1,1, startDateTime.Hour, startDateTime.Minute, startDateTime.Second);
            var endTime = new DateTime(1, 1, 1, endDateTime.Hour, endDateTime.Minute, endDateTime.Second);
            if (startTime > endTime)
            {
                nightShiftSecondBreak = nightShiftSecondBreak.AddDays(1);
                nightShiftThirdBreak = nightShiftThirdBreak.AddDays(1);
                endTime = endTime.AddDays(1);
            }
            
            if (dayShiftFirstBreak >= startTime && dayShiftFirstBreak <= endTime) breaks += TimeSpan.FromMinutes(15);
            if (dayShiftSecondBreak >= startTime && dayShiftSecondBreak <= endTime) breaks += TimeSpan.FromMinutes(30);
            if (dayShiftThirdBreak >= startTime && dayShiftThirdBreak <= endTime) breaks += TimeSpan.FromMinutes(15);
            if (nightShiftFirstBreak >= startTime && nightShiftFirstBreak <= endTime) breaks += TimeSpan.FromMinutes(30);
            if (nightShiftSecondBreak >= startTime && nightShiftSecondBreak <= endTime) breaks += TimeSpan.FromMinutes(30);
            if (nightShiftThirdBreak >= startTime && nightShiftThirdBreak <= endTime) breaks += TimeSpan.FromMinutes(30);

            return breaks;
        }

        public static double GetPartialBreakBetween(DateTime startDateTime, DateTime endDateTime)
        {
            if (endDateTime == DateTime.MinValue) return 0;
            var dayShiftFirstBreakDuration = 15;
            var dayShiftFirstBreak = WorkTime.DayShiftFirstBreak.AddMinutes(-dayShiftFirstBreakDuration);

            var dayShiftSecondBreakDuration = 30;
            var dayShiftSecondBreak = WorkTime.DayShiftSecondBreak.AddMinutes(-dayShiftSecondBreakDuration);

            var dayShiftThirdBreakDuration = 15;
            var dayShiftThirdBreak = WorkTime.DayShiftThirdBreak.AddMinutes(-dayShiftThirdBreakDuration);

            var nightShiftFirstBreakDuration = 30;
            var nightShiftFirstBreak = WorkTime.NightShiftFirstBreak.AddMinutes(-nightShiftFirstBreakDuration);

            var nightShiftSecondBreakDuration = 30;
            var nightShiftSecondBreak = WorkTime.NightShiftSecondBreak.AddMinutes(-nightShiftSecondBreakDuration);

            var nightShiftThirdDuration = 30;
            var nightShiftThirdBreak = WorkTime.NightShiftThirdBreak.AddMinutes(-nightShiftThirdDuration);

            var startTime = new DateTime(1, 1, 1, startDateTime.Hour, startDateTime.Minute, startDateTime.Second);
            var endTime = new DateTime(1, 1, 1, endDateTime.Hour, endDateTime.Minute, endDateTime.Second);
            if (startTime > endTime)
            {
                nightShiftSecondBreak = nightShiftSecondBreak.AddDays(1);
                nightShiftThirdBreak = nightShiftThirdBreak.AddDays(1);
                endTime = endTime.AddDays(1);
            }

            var breaks = 0.0;

            breaks += GetPartial(startTime, endTime, dayShiftFirstBreak, dayShiftFirstBreakDuration);
            breaks += GetPartial(startTime, endTime, dayShiftSecondBreak, dayShiftSecondBreakDuration);
            breaks += GetPartial(startTime, endTime, dayShiftThirdBreak, dayShiftThirdBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftFirstBreak, nightShiftFirstBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftSecondBreak, nightShiftSecondBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftThirdBreak, nightShiftThirdDuration);

            return breaks;
        }

        private static double GetPartial(DateTime startDateTime, DateTime endDateTime, DateTime breakTime, double duration)
        {
            var result = 0.0;
            var endBreakTime = breakTime.AddMinutes(duration);
            if (startDateTime < breakTime && endDateTime > breakTime && endDateTime <= endBreakTime) {
                result = (endDateTime - breakTime).TotalMinutes;
            } else if (startDateTime > breakTime && startDateTime < endBreakTime && endDateTime >= endBreakTime) {
                result = (endBreakTime - startDateTime).TotalMinutes;
            } else if (startDateTime > breakTime && endDateTime < endBreakTime) {
                result = (endDateTime - startDateTime).TotalMinutes;
            } else if (startDateTime <= breakTime && endDateTime >= endBreakTime) {
                result = (endBreakTime - breakTime).TotalMinutes;
            }

            return result;
        }

        public static DateTime GetStartShiftTime()
        {
            return AppSettings.Instance.CurrentShift == Text.DayShift ? DateTime.Today.AddHours(7) :
                DateTime.Now.Hour < 7 ? DateTime.Today.AddDays(-1).AddHours(19) : DateTime.Today.AddHours(19);
        }

        public static DateTime GetEndShiftTime()
        {
            return AppSettings.Instance.CurrentShift == Text.DayShift ? DateTime.Today.AddHours(19) :
                DateTime.Now.Hour < 7 ? DateTime.Today.AddHours(7) : DateTime.Today.AddDays(1).AddHours(7);
        }

        public static void AddDownTime(this Part part, DownTime.Types type)
        {
            part.DownTimes.Add(new DownTime(part, type));
        }

        /// <summary>
        /// Получить массив клавиш для набора времени
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static Keys[] GetKeys(this DateTime dateTime)
        {
            // d  d  .  M  M  .  y  y  y  y     H  H  :  m  m
            // 00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15
            var text = dateTime.ToString(Text.DateTimeFormat);
            var res = new Keys[16];
            for (var i = 0; i < 16; i++)
            {
                res[i] = i switch
                {
                    2 or 5 => KeyboardLayout.Current is KeyboardLayout.En ? Keys.OemPeriod : Keys.Oem2,
                    10 => Keys.Space,
                    13 => KeyboardLayout.Current is KeyboardLayout.En ? Keys.Oem1 : Keys.D6,
                    _ => text[i].DKeyFromChar()
                };
            }
            return res;
        }

        public static Keys DKeyFromChar(this char ch)
        {
            return ch switch
            {
                '0' => Keys.D0,
                '1' => Keys.D1,
                '2' => Keys.D2,
                '3' => Keys.D3,
                '4' => Keys.D4,
                '5' => Keys.D5,
                '6' => Keys.D6,
                '7' => Keys.D7,
                '8' => Keys.D8,
                '9' => Keys.D9,
                _ => throw new ArgumentException("Переданный символ не является арабской цифрой."),
            };
        }
    }
}
