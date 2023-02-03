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
        /// <summary>
        /// Локальный путь для записи изготовлений при неудачной записи в общую таблицу на случай отсутствия интернета, занятости файла и тд. 
        /// </summary>
        public const string XlReservedPath = "C:\\ProgramData\\dece1ver\\eLog\\XL";

        /// <summary>
        /// Директория с файлом конфигурации
        /// </summary>
        public const string ConfigPath = "C:\\ProgramData\\dece1ver\\eLog";

        /// <summary>
        /// Путь к файлу конфигурации
        /// </summary>
        public static readonly string ConfigFilePath = Path.Combine(ConfigPath, "config.json");

        /// <summary>
        /// Текущий станок
        /// </summary>
        public static Machine Machine { get; set; } = new (0);

        /// <summary>
        /// Путь к общей таблице
        /// </summary>
        public static string XlPath { get; set; } = string.Empty;

        /// <summary>
        /// Путь к таблице с номенклатурой
        /// </summary>
        public static string OrdersSourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Список операторов
        /// </summary>
        public static ObservableCollection<Operator> Operators { get; set; } = new();

        /// <summary>
        /// Текущий оператор
        /// </summary>
        public static Operator? CurrentOperator { get; set; }

        /// <summary>
        /// Создает конфиг с параметрами по-умолчанию
        /// </summary>
        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
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
            var appSettings = new AppSettingsModel(Machine, XlPath, OrdersSourcePath, Operators, CurrentOperator);
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }
    }
}
