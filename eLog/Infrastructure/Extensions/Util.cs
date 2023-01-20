using eLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    internal static class Util
    {
        public static string Capitalize(this string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            char[] letters = source.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        public static PartInfoModel GetPartFromBarCode(this string barCode)
        {
            var names = new string[] {"Ниппель", "Корпус", "Гайка", "Фланец", "Штуцер", "Седло", "Крышка", "Корпус приводной камеры", "Корпус проточной части" };
            var numbers = new string[] { "АР110-01-001", "АР110-01-002", "АР110-01-003", "АР110-01-004", "АР110-01-005", "АРМ2-31.4-02-340-Х6-081-01", "АРМ2-31.4-02-340-Х6-071" };
            var orders = new string[] { "УЧ-1/0001.1.1", "УЧ-1/0001.1.2", "УЧ-1/0001.1.3", "УЧ-1/0001.1.4", "УЧ-1/0001.1.5", "УЧ-1/0001.1.6", "УЧ-1/0001.1.7", "УЧ-1/0001.1.8" };

            return new PartInfoModel(
                names[new Random().Next(0, names.Length)],
                numbers[new Random().Next(0, numbers.Length)],
                new Random().Next(1, 4),
                orders[new Random().Next(0, orders.Length)],
                new Random().Next(1, 20) * 10,
                new Random().Next(4, 18) * 10,
                new Random().Next(1, 20)); // имитация получения иформации из БД
        }
    }
}
