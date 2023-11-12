using ClosedXML.Excel;
using libeLog.Infrastructure;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    public static class Util
    {
        public static void WriteLog(string message)
            => Logs.Write(AppSettings.LogFile, message, GetCopyDir());

        public static void WriteLog(Exception exception, string additionalMessage = "")
            => Logs.Write(AppSettings.LogFile, exception, additionalMessage, GetCopyDir());

        /// <summary>
        /// Получает директорию для копирования логов, которой является директория таблицы.
        /// </summary>
        /// <returns></returns>
        private static string GetCopyDir()
        {
            if (Directory.Exists(AppSettings.Instance.SourcePath) && Directory.GetParent(AppSettings.Instance.SourcePath) is { Parent: not null } parent)
            {
                return parent.FullName;
            }
            return "";
        }

        public static ICollection<Part> ReadPartsFromXl()
        {
            List <Part> parts = new List<Part>();

            using (var fs = new FileStream(AppSettings.Instance.SourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });
                
                foreach (var xlRow in wb.Worksheet(1).Rows().Skip(1))
                {
                    if (!xlRow.Cell(1).Value.IsNumber) break;
                    parts.Add(new Part(
                        (int)xlRow.Cell(1).Value.GetNumber(),     // id
                        xlRow.Cell(2).Value.GetText(),            // наладка %
                        xlRow.Cell(3).Value.GetText(),            // изг %
                        xlRow.Cell(5).Value.GetText(),            // комментарий
                        xlRow.Cell(6).Value.GetDateTime(),        // дата
                        xlRow.Cell(7).Value.GetText(),            // станок
                        xlRow.Cell(8).Value.GetText(),            // смена
                        xlRow.Cell(9).Value.GetText(),            // оператор
                        xlRow.Cell(10).Value.GetText(),           // деталь
                        xlRow.Cell(11).Value.GetText(),           // м/л
                        (int)xlRow.Cell(12).Value.GetNumber(),    // количество
                        (int)xlRow.Cell(13).Value.GetNumber(),    // установка
                        xlRow.Cell(14).Value.GetDateTime(),       // начало наладки
                        xlRow.Cell(15).Value.GetDateTime(),       // начало изготовления
                        xlRow.Cell(21).Value.GetDateTime(),       // конец изготовления
                        xlRow.Cell(16).Value.GetNumber(),         // фактическая наладка
                        xlRow.Cell(17).Value.GetNumber(),         // плановая наладка
                        xlRow.Cell(22).Value.GetNumber(),         // фактическое изготовление
                        xlRow.Cell(23).Value.GetNumber(),         // плановое изготовление
                        xlRow.Cell(34).Value.GetNumber(),         // простои в наладке
                        xlRow.Cell(35).Value.GetNumber()          // простои в изготовлении
                        ));
                }
            }

            return parts;
        }
    }
}
