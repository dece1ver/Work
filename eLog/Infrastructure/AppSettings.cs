using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using eLog.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace eLog.Infrastructure
{
    /// <summary>
    /// Основной класс с настройками, статический для доступа откуда угодно.
    /// </summary>
    public static class AppSettings
    {
        /// <summary> Локальный путь для записи изготовлений при неудачной записи в общую таблицу на случай отсутствия интернета, занятости файла и тд. </summary>
        public const string XlReservedPath = "C:\\ProgramData\\dece1ver\\eLog\\XL";

        /// <summary> Директория для хранения всякого </summary>
        public const string BasePath = "C:\\ProgramData\\dece1ver\\eLog";

        /// <summary> Путь к файлу конфигурации </summary>
        public static readonly string ConfigFilePath = Path.Combine(BasePath, "config.json");

        /// <summary> Путь к локальному списку заказов </summary>
        public static readonly string LocalOrdersFile = Path.Combine(BasePath, "orders.xlsx");

        /// <summary> Путь к резервному списку заказов </summary>
        public static readonly string BackupOrdersFile = Path.Combine(BasePath, "orders-backup.xlsx");

        /// <summary> Текущий станок </summary>
        public static Machine Machine { get; set; } = new (0);

        /// <summary> Путь к общей таблице </summary>
        public static string XlPath { get; set; } = string.Empty;

        /// <summary> Путь к таблице с номенклатурой </summary>
        public static string OrdersSourcePath { get; set; } = string.Empty;

        /// <summary> Путь к таблице с номенклатурой </summary>
        public static string[] OrderQualifiers { get; set; } = Array.Empty<string>();

        /// <summary> Список операторов </summary>
        public static ObservableCollection<Operator> Operators { get; set; } = new();

        /// <summary> Текущий оператор </summary>
        public static Operator? CurrentOperator { get; set; }

        /// <summary> Создает конфиг с параметрами по-умолчанию </summary>
        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            Machine = new Machine(0);
            Operators = new ObservableCollection<Operator>() {
                new() {
                    LastName = "Бабохин",
                    FirstName = "Кирилл",
                    Patronymic = "Георгиевич",
                    },
                };
            XlPath = string.Empty;
            OrdersSourcePath = string.Empty;
            OrderQualifiers = new[]
            {
                "УЧ",
                "ФЛ",
                "БП",
                "СУ",
                "УУ",
                "ЗУ",
                "СЛ",
            };
            RewriteConfig();
        }


        /// <summary>
        /// Читает конфиг, если возникает исключение, то создает конфиг по-умолчанию.
        /// </summary>
        public static void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                var appSettings = JsonConvert.DeserializeObject<AppSettingsModel>(File.ReadAllText(ConfigFilePath));
                if (appSettings is null) throw new ArgumentNullException();
                Machine = appSettings.Machine;
                XlPath = appSettings.XlPath;
                OrdersSourcePath = appSettings.OrdersSourcePath;
                OrderQualifiers = appSettings.OrderQualifiers;
                Operators = appSettings.Operators;
                CurrentOperator = appSettings.CurrentOperator;
                if (appSettings.CurrentOperator is null && Operators.Count > 0) CurrentOperator = Operators[0];
            }
            catch
            {
                CreateBaseConfig();
                ReadConfig();
            }
        }

        /// <summary>
        /// Обновляет конфиг
        /// </summary>
        public static void RewriteConfig()
        {
            var appSettings = new AppSettingsModel(Machine, XlPath, OrdersSourcePath, OrderQualifiers, Operators, CurrentOperator);
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }
    }
}
