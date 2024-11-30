using ClosedXML.Excel;
using libeLog;
using libeLog.Infrastructure;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace remeLog.Infrastructure
{
    public static class Util
    {
        public static async void WriteLogAsync(string message)
            => await Logs.Write(AppSettings.LogFile, message, GetCopyDir());

        public static async void WriteLogAsync(Exception exception, string additionalMessage = "")
            => await Logs.Write(AppSettings.LogFile, exception, additionalMessage, GetCopyDir());

        /// <summary>
        /// Получает директорию для копирования логов, которой является директория таблицы.
        /// </summary>
        /// <returns></returns>
        private static string GetCopyDir()
        {
            if (Directory.Exists(AppSettings.Instance.QualificationSourcePath) && Directory.GetParent(AppSettings.Instance.QualificationSourcePath) is { Parent: not null } parent)
            {
                return parent.FullName;
            }
            return "";
        }

        public static string GetXlsxPath(bool save = true)
        {
            FileDialog fileDialog;
            if (save)
            {
                fileDialog = new SaveFileDialog()
                {
                    Filter = "Книга Excel (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx"
                };
            }
            else
            {
                fileDialog = new OpenFileDialog()
                {
                    Filter = "Книга Excel (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx"
                };
            }
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                return fileDialog.FileName;
            }
            return "";
        }

        public static int GetWorkDaysBeetween(DateTime start, DateTime end) 
            => (int)(end - start).TotalDays + 1 - Constants.Dates.Holidays.Count(d => d >= start && d <= end);

        public static async Task<ObservableCollection<Part>> GenerateMockPartsAsync()
        {
            return await Task.Run(() =>
            {
                var random = new Random();
                var mockParts = Enumerable.Range(1, 50).Select(i =>
                {
                    var shiftDate = DateTime.Today;
                    return new Part(
                        Guid.NewGuid(),
                        $"Machine_{random.Next(1, 5)}",
                        random.Next(0, 2) == 0 ? "День" : "Ночь",
                        shiftDate,
                        $"Operator_{random.Next(1, 10)}",
                        $"Part_{random.Next(1, 20)}",
                        $"Order_{random.Next(1, 100)}",
                        random.Next(0, 2),
                        random.Next(1, 100),
                        random.Next(101, 200),
                        shiftDate.AddHours(random.Next(1, 8)),
                        shiftDate.AddHours(random.Next(8, 16)),
                        random.NextDouble() * 10,
                        shiftDate.AddHours(random.Next(16, 24)),
                        random.NextDouble() * 10,
                        random.NextDouble() * 10,
                        random.NextDouble() * 10,
                        random.NextDouble(),
                        TimeSpan.FromMinutes(random.Next(10, 200)),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        random.NextDouble(),
                        $"Operator Comment {i}",
                        $"Master Setup Comment {i}",
                        $"Master Machining Comment {i}",
                        $"Specified Downtime {i}",
                        $"Unspecified Downtime {i}",
                        $"Master Comment {i}",
                        random.NextDouble(),
                        random.NextDouble(),
                        $"Engineer Comment {i}",
                        random.Next(0, 2) == 1
                    );
                }).ToList();

                return new ObservableCollection<Part>(mockParts); // Возвращаем результат
            });
        }
    }
}
