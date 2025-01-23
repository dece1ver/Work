using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using libeLog;
using libeLog.Infrastructure;
using libeLog.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using remeLog.Infrastructure.Services;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace remeLog.Infrastructure
{
    public static class Util
    {
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

        public static async Task WriteLogAsync(string message)
            => await Logs.Write(AppSettings.LogFile, message, GetCopyDir());

        public static async Task WriteLogAsync(Exception exception, string additionalMessage = "")
            => await Logs.Write(AppSettings.LogFile, exception, additionalMessage, GetCopyDir());



        private static void LogWithErrorHandling(Func<Task> logAction)
        {
            Task.Run(async () =>
            {
                try
                {
                    await logAction();
                }
                catch (Exception ex)
                {
                    App.Current.Dispatcher.Invoke(() =>
                        MessageBox.Show($"Ошибка при логировании: {ex.Message}"));
                }
            });
        }

        public static void WriteLog(string message)
            => LogWithErrorHandling(() => WriteLogAsync(message));

        public static void WriteLog(Exception exception, string additionMessage = "")
            => LogWithErrorHandling(() => WriteLogAsync(exception, additionMessage));

        /// <summary>
        /// Открывает диалог для выбора или сохранения файла Excel и возвращает путь к выбранному файлу.
        /// </summary>
        /// <param name="save">
        /// Если <c>true</c>, открывается диалог сохранения; 
        /// если <c>false</c>, открывается диалог открытия.
        /// </param>
        /// <returns>Путь к выбранному файлу или пустая строка, если файл не был выбран.</returns>
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

        /// <summary>
        /// Рассчитывает количество рабочих дней между двумя датами, исключая выходные и праздничные дни.
        /// </summary>
        /// <param name="start">Дата начала периода.</param>
        /// <param name="end">Дата окончания периода.</param>
        /// <returns>Количество рабочих дней.</returns>
        public static int GetWorkDaysBeetween(DateTime start, DateTime end)
            => (int)(end - start).TotalDays + 1 - Constants.Dates.Holidays.Count(d => d >= start && d <= end);

        /// <summary>
        /// Создает функцию сравнения на основе заданного оператора сравнения и значения.
        /// </summary>
        /// <param name="op">Оператор сравнения: "=","≡",">","&gt;=","≥","&lt;","&lt;=","≤","!=","≠". 
        /// Пустая строка или null интерпретируются как "&gt;=".</param>
        /// <param name="value">Значение, с которым будет выполняться сравнение.</param>
        /// <returns>
        /// Лямбда-функция, которая принимает целое число и возвращает результат сравнения в виде <c>true</c> или <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Выбрасывается, если оператор сравнения пустой или не соответствует ни одному из поддерживаемых операторов.
        /// </exception>
        public static Func<int, bool> CreateComparisonFunc(string op, int value)
        {
            op ??= "";
            return op switch
            {
                "=" or "≡" => count => count == value,
                ">" => count => count > value,
                "" or ">=" or "≥" => count => count >= value,
                "<" => count => count < value,
                "<=" or "≤" => count => count <= value,
                "!=" or "≠" => count => count != value,
                _ => throw new ArgumentException($"Неизвестный оператор сравнения: {op}", nameof(op))
            };
        }

        /// <summary>
        /// Определяет, нужно ли использовать родительный падеж для оператора сравнения.
        /// </summary>
        /// <param name="op">Оператор сравнения.</param>
        /// <returns>true для операторов, требующих родительного падежа (>, ≥, &lt;, ≤); 
        /// false для операторов равенства (=, !=)</returns>
        public static bool ShouldUseGenitive(string op) => op switch
        {
            ">" or ">=" or "<" or "<=" => true,
            "=" or "!=" => false,
            _ => false
        };

        /// <summary>
        /// Пытается разобрать строку как оператор сравнения и значение.
        /// </summary>
        /// <param name="input">Строка для анализа (например, ">10").</param>
        /// <param name="comparisonOperator">Выходной параметр для оператора сравнения.</param>
        /// <param name="comparisonValue">Выходной параметр для значения сравнения.</param>
        /// <returns>
        /// <c>true</c>, если разбор успешен; <c>false</c>, если строка не соответствует ожидаемому формату.
        /// </returns>
        public static bool TryParseComparison(string input, out string comparisonOperator, out int comparisonValue)
        {
            comparisonOperator = null!;
            comparisonValue = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (int.TryParse(input, out int res))
            {
                comparisonOperator = "=";
                comparisonValue = res;
                return true;
            }

            foreach (string op in Constants.ComparisonOperators)
            {
                if (input.Contains(op))
                {
                    comparisonOperator = op;
                    string[] parts = input.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 1)
                        return false;

                    if (!int.TryParse(parts[0].Trim(), out comparisonValue))
                        return false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Возвращает математический символ, соответствующий оператору сравнения.
        /// </summary>
        /// <param name="op">Оператор сравнения (например, "=", "&gt;", "&lt;=").</param>
        /// <returns>
        /// Математический символ (например, "≤" для "&lt;=") или "?" для неизвестного оператора.
        /// </returns>
        public static string GetOperatorSymbol(string op)
        {
            return op switch
            {
                "=" => "=",
                ">" => ">",
                ">=" => "≥",
                "<" => "<",
                "<=" => "≤",
                "!=" => "≠",
                _ => "?" // Неизвестный оператор
            };
        }

        /// <summary>
        /// Асинхронно создает коллекцию фиктивных деталей для тестирования.
        /// </summary>
        /// <returns>Коллекция фиктивных деталей <see cref="ObservableCollection{Part}"/>.</returns>
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

        public static async Task<string> SearchInWindchill(string searchQuery, CancellationToken cancellationToken)
        {
            var dbResult = Database.GetWncConfig(out var wncConfig);
            Util.Debug(wncConfig);
            if (dbResult != DbResult.Ok || wncConfig == null)
            {
                throw new Exception("Не удалось получить конфигурацию Windchill");
            }

            var service = new WindchillService(wncConfig.Server, wncConfig.User, wncConfig.Password, wncConfig.LocalType);
            cancellationToken.ThrowIfCancellationRequested();
            return await service.SearchDocumentsAsync(searchQuery, cancellationToken);
        }

        public static List<WncObject> ExtractWncObjects(string inputString)
        {
            var objects = new List<WncObject>();
            if (Database.GetWncConfig(out var wncConfig) != DbResult.Ok)
            {
                MessageBox.Show("Не удалось выполнить поиск в Windchill из-за ошибки");
                return objects;
            }

            // Ищем все объекты в JSON
            var regex = new Regex(@"PTC\.ExtJSONTableConfig\.chunk\s*=\s*({.*?});", RegexOptions.Singleline);
            var match = regex.Match(inputString);
            if (!match.Success) return objects;

            // Парсим JSON объект
            var jsonObject = JObject.Parse(match.Groups[1].Value);
            var data = jsonObject["data"] as JArray;
            if (data == null) return objects;

            foreach (var obj in data)
            {
                // Извлекаем необходимые поля
                var name = obj["name"]?.ToString() ?? "";
                var partNumber = obj["number"]?["comparable"]?.ToString() ?? "";
                var version = obj["version"]?["gui"]?["html"]?.ToString() ?? "";
                var state = obj["state"]?.ToString() ?? "";
                var containerName = obj["containerName"]?["label"]?.ToString() ?? "";
                var type = obj["type_icon"]?["gui"]?["comparable"]?.ToString() ?? "";
                var modifyDate = obj["thePersistInfo_modifyStamp"]?["gui"]?["html"]?.ToString() ?? "";
                var createDate = obj["thePersistInfo_createStamp"]?["gui"]?["html"]?.ToString() ?? "";
                var oid = obj["oid"]?.ToString() ?? "";
                var containerOid = obj["nmActions"]?["params"]?["ContainerOid"]?.ToString() ?? "";
                var u8 = "1";

                // Формируем ссылку
                var link = $"{wncConfig.Server}/Windchill/app/#ptc1/tcomp/infoPage?ContainerOid={containerOid}&oid={oid}&u8={u8}";

                // Создаем объект с необходимыми полями
                objects.Add(new WncObject(name, partNumber, link, version, state, containerName, type, modifyDate.Replace("MSK", "").Trim(), createDate.Replace("MSK", "").Trim()));
            }

            return objects;
        }

        /// <summary>
        /// Настраивает лицензию Syncfusion, используя переменную окружения или данные из базы данных.
        /// </summary>
        internal static void TrySetupSyncfusionLicense()
        {
            var key = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE", EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(AppSettings.Instance.ConnectionString))
            {
                key = Database.GetLicenseKey("syncfusion");
                Environment.SetEnvironmentVariable("SYNCFUSION_LICENSE", key, EnvironmentVariableTarget.User);
            }
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
        }

        [Conditional("DEBUG")]
        public static void Debug(
        object obj,
        int indent = 0,
        int maxDepth = 2
            ,
        [CallerArgumentExpression("obj")] string expression = null!)
        {
#if DEBUG
            if (obj == null)
            {
                System.Diagnostics.Debug.WriteLine($"{expression} = null");
                return;
            }

            if (indent > maxDepth)
            {
                System.Diagnostics.Debug.WriteLine($"{new string(' ', indent * 2)}... (Max depth reached)");
                return;
            }

            var type = obj.GetType();
            var indentStr = new string(' ', indent * 2);

            if (type.IsPrimitive || obj is string)
            {
                System.Diagnostics.Debug.WriteLine($"{expression} = {obj}");
                return;
            }

            if (indent == 0)
            {
                System.Diagnostics.Debug.WriteLine($"{expression}:");
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    // Пропускаем свойства, которые требуют параметров
                    if (property.GetIndexParameters().Length > 0) continue;

                    var value = property.GetValue(obj);
                    if (value == null || property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                    {
                        System.Diagnostics.Debug.WriteLine($"{indentStr}{property.Name} = {value}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{indentStr}{property.Name}:");
                        Debug(value, indent + 1, maxDepth);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку при доступе к свойству
                    System.Diagnostics.Debug.WriteLine($"{indentStr}{property.Name} = <Error: {ex.Message}>");
                }
            }
#endif
        }
    }
}
