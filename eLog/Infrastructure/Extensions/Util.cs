using eLog.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;

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
        /// Получение информации о детали с БД (пока имитация)
        /// </summary>
        /// <param name="barCode"></param>
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
        /// <param name="pathToXl">Путь к таблице</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static void WriteToXl(this PartInfoModel part)
        {
            try
            {
                var wb = new XLWorkbook(AppSettings.XlPath);
                foreach (var xlRow in wb.Worksheet("Для заполнения").Rows())
                {
                    if (xlRow is null || !xlRow.Cell(6).Value.IsBlank) continue;
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
                    xlRow.Cell(22).Value = part.MachineTimePlan;
                    xlRow.Cell(23).Value = part.MachineTime;
                    break;
                }
                wb.Save();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        /// <summary>
        /// Парсит строку в TimeSpan
        /// </summary>
        /// <param name="input">Строка с вводом</param>
        /// <param name="time">Спарсеный промежуток времени</param>
        /// <returns></returns>
        public static bool TimeParse(this string input, out TimeSpan time)
        {
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
                        double.TryParse(sTime[1], out var seconds))
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
    }
}
