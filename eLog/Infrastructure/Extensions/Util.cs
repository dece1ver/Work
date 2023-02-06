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

namespace eLog.Infrastructure.Extensions
{
    internal static class Util
    {
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
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        /// <summary>
        /// Получение информации о заказе по номеру М/Л (имитация)
        /// </summary>
        /// <param name="orderNumber">Номер М/Л</param>
        public static PartInfoModel? GetPartFromOrder(this string orderNumber)
        {
            try
            {
                var wb = new XLWorkbook(AppSettings.LocalOrdersFile);
                foreach (var xlRow in wb.Worksheet(1).Rows())
                {
                    if (xlRow is {} && xlRow.Cell(1).Value.IsText && xlRow.Cell(1).Value.GetText().Contains(orderNumber.ToUpper()))
                    {
                        return new PartInfoModel()
                        {
                            Name = xlRow.Cell(4).Value.GetText(),
                            PartsCount = (int)xlRow.Cell(5).Value.GetNumber(),
                        };
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                
            }
            return null;
        }

        /// <summary>
        /// Получение информации о детали с БД (пока имитация)
        /// </summary>
        /// <param name="barCode">Шрихкод</param>
        /// <returns></returns>
        public static PartInfoModel GetPartFromBarCode(this string barCode)
        {
            var names = new[] {"Ниппель", "Корпус", "Гайка", "Фланец", "Штуцер", "Седло", "Крышка", "Корпус приводной камеры", "Корпус проточной части" };
            var numbers = new[] { "АР110-01-001", "АР110-01-002", "АР110-01-003", "АР110-01-004", "АР110-01-005", "АРМ2-31.4-02-340-Х6-081-01", "АРМ2-31.4-02-340-Х6-071" };
            var orders = new[] { "УЧ-1/0001.1.1", "УЧ-1/0001.1.2", "УЧ-1/0001.1.3", "УЧ-1/0001.1.4", "УЧ-1/0001.1.5", "УЧ-1/0001.1.6", "УЧ-1/0001.1.7", "УЧ-1/0001.1.8" };

            return new PartInfoModel(
                names[new Random().Next(0, names.Length)],
                numbers[new Random().Next(0, numbers.Length)],
                new Random().Next(1, 4),
                orders[new Random().Next(0, orders.Length)],
                new Random().Next(1, 20) * 10,
                new Random().Next(4, 18) * 10,
                new Random().Next(1, 5));
        }

        /// <summary>
        /// Запись изготовления в Excel таблицу
        /// </summary>
        /// <param name="part">Информация об изготовлении</param>
        /// <returns>Int32 - Id записи в таблице</returns>
        /// <exception cref="NotImplementedException"></exception>
        public static int WriteToXl(this PartInfoModel part)
        {
            var id = -1;
            try
            {
                var wb = new XLWorkbook(AppSettings.XlPath);
                var ws = wb.Worksheet("Для заполнения");
                ws.LastRow().InsertRowsAbove(1);
                IXLRow? prevRow = null;
                
                foreach (var xlRow in ws.Rows())
                {
                    if (xlRow is null) continue;
                    if (!xlRow.Cell(6).Value.IsBlank)
                    {
                        prevRow = xlRow;
                        continue;
                    }

                    id = (int)prevRow!.Cell(1).Value.GetNumber() + 1;
                    xlRow.Style = prevRow!.Style;
                    xlRow.Cell(1).Value = id;
                    xlRow.Cell(2).FormulaR1C1 = prevRow.Cell(2).FormulaR1C1;
                    xlRow.Cell(3).FormulaR1C1 = prevRow.Cell(3).FormulaR1C1;
                    xlRow.Cell(4).FormulaR1C1 = prevRow.Cell(4).FormulaR1C1;
                    xlRow.Cell(5).Value = part.DownTimes.Report(); // потом добавить добавление этого отчета по простоям к комментариям
                    xlRow.Cell(6).Value = DateTime.Today.ToString("dd.MM.yyyy");
                    xlRow.Cell(7).Value = AppSettings.Machine.Name;
                    xlRow.Cell(8).Value = AppSettings.CurrentOperator?.FullName;
                    xlRow.Cell(9).Value = part.FullName;
                    xlRow.Cell(10).Value = part.Order;
                    xlRow.Cell(11).Value = part.PartsFinished;
                    xlRow.Cell(13).Value = part.StartSetupTime.ToString("HH:mm");
                    xlRow.Cell(14).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(15).FormulaR1C1 = prevRow.Cell(15).FormulaR1C1;
                    xlRow.Cell(16).Value = part.SetupTimePlan;
                    xlRow.Cell(17).FormulaR1C1 = prevRow.Cell(17).FormulaR1C1;
                    xlRow.Cell(18).FormulaR1C1 = prevRow.Cell(18).FormulaR1C1;
                    xlRow.Cell(19).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(20).Value = part.EndMachiningTime.ToString("HH:mm");
                    xlRow.Cell(21).FormulaR1C1 = prevRow.Cell(21).FormulaR1C1;
                    xlRow.Cell(22).Value = part.PartProductionTimePlan;
                    xlRow.Cell(23).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                    for (var i = 24; i <= 32; i++)
                    {
                        xlRow.Cell(i).FormulaR1C1 = prevRow.Cell(i).FormulaR1C1;
                    }

                    for (var i = 1; i <= 32; i++)
                    {
                        xlRow.Cell(i).Style = prevRow.Cell(i).Style;
                    }

                    break;
                }
                wb.Save();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return id;
        }

        public static bool RewriteToXl(this PartInfoModel part)
        {
            var result = false;
            try
            {
                var wb = new XLWorkbook(AppSettings.XlPath);
                foreach (var xlRow in wb.Worksheet("Для заполнения").Rows())
                {
                    if (!xlRow.Cell(1).Value.IsNumber || (int)xlRow.Cell(1).Value.GetNumber() != part.Id) continue;
                    xlRow.Cell(6).Value = DateTime.Today.ToString("dd.MM.yyyy");
                    xlRow.Cell(7).Value = AppSettings.Machine.Name;
                    xlRow.Cell(8).Value = AppSettings.CurrentOperator?.FullName;
                    xlRow.Cell(9).Value = part.FullName;
                    xlRow.Cell(10).Value = part.Order;
                    xlRow.Cell(11).Value = part.PartsFinished;
                    xlRow.Cell(13).Value = part.StartSetupTime.ToString("HH:mm");
                    xlRow.Cell(14).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(16).Value = part.SetupTimePlan;
                    xlRow.Cell(19).Value = part.StartMachiningTime.ToString("HH:mm");
                    xlRow.Cell(20).Value = part.EndMachiningTime.ToString("HH:mm");
                    xlRow.Cell(22).Value = part.PartProductionTimePlan;
                    xlRow.Cell(23).Value = Math.Round(part.MachineTime.TotalMinutes, 2);
                    result = true;
                    break;

                }
                wb.Save();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return result;
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
                time = TimeSpan.Zero;;
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
        /// <param name="downtimes"></param>
        /// <returns></returns>
        public static string Report(this ObservableCollection<DownTime> downTimes) => 
            downTimes.Aggregate(string.Empty, (current, downTime) => current + $"{downTime.Name}: {Math.Round(downTime.Time.TotalMinutes, 2)} мин\n");

        public static double TotalMinutes(this ObservableCollection<DownTime> downTimes) =>
            downTimes.Aggregate(0.0, (sum, downTime) => sum + downTime.Time.TotalMinutes);
    }
}
