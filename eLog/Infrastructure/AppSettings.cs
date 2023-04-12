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
using eLog.Infrastructure.Extensions;
using eLog.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace eLog.Infrastructure
{
    /// <summary>
    /// Основной класс с настройками, статический для доступа откуда угодно, потом синглтон сделаю наверно.
    /// </summary>
    public class AppSettings
    {
        private static AppSettings _Instance;
        [JsonIgnore] public static AppSettings Instance => _Instance ??= new AppSettings();

        /// <summary> Директория для хранения всякого </summary>
        [JsonIgnore] public const string BasePath = "C:\\ProgramData\\dece1ver\\eLog";

        /// <summary> Локальный путь для бэкапа таблицы. </summary>
        [JsonIgnore] public static readonly string XlReservedPath = Path.Combine(BasePath, "backup.xlsx");

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigFilePath = Path.Combine(BasePath, "config.json");

        /// <summary> Путь к локальному списку заказов </summary>
        [JsonIgnore] public static readonly string LocalOrdersFile = Path.Combine(BasePath, "orders.xlsx");

        /// <summary> Путь к резервному списку заказов </summary>
        [JsonIgnore] public static readonly string BackupOrdersFile = Path.Combine(BasePath, "orders-backup.xlsx");

        /// <summary> Путь к файлу логов </summary>
        [JsonIgnore] public static readonly string LogFile = Path.Combine(BasePath, "log");

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

        /// <summary> Текущая смена </summary>
        public static string CurrentShift { get; set; } = string.Empty;

        /// <summary> Список деталей </summary>
        public static ObservableCollection<PartInfoModel> Parts { get; set; } = new();

        /// <summary> Запущена ли смена </summary>
        public static bool IsShiftStarted { get; set; }

        /// <summary> Текущий оператор </summary>
        public static Operator? CurrentOperator { get; set; }

        /// <summary> Создает конфиг с параметрами по-умолчанию </summary>
        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            var tempAppSettings = new AppSettings()
            {
                Machine = new Machine(0),
                Operators = new ObservableCollection<Operator>()
                {
                    new()
                    {
                        LastName = "Бабохин",
                        FirstName = "Кирилл",
                        Patronymic = "Георгиевич"
                    },
                },
                CurrentShift = Text.DayShift,
                Parts = new ObservableCollection<PartInfoModel>(),
                XlPath = string.Empty,
                OrdersSourcePath = string.Empty,
                OrderQualifiers = new[]
                {
                    Text.WithoutOrderItem,
                    "УЧ",
                    "ФЛ",
                    "БП",
                    "СУ",
                    "УУ",
                    "ЗУ",
                    "СЛ",
                },
                IsShiftStarted = false,
            };
            RewriteConfig();
        }

        public static Dictionary<string, string> GetPropertiesValues()
        {
            return typeof(AppSettings)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.PropertyType == typeof(string))
                .ToDictionary(f => f.Name,
                    f => (string)f.GetValue(null)!);
        }

        /// <summary>
        /// Читает конфиг, если возникает исключение, то создает конфиг по-умолчанию.
        /// </summary>
        public static AppSettings ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                var json = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(ConfigFilePath));
                return json ?? throw new NullReferenceException();
            }
            catch
            {
                CreateBaseConfig();
                return ReadConfig();
            }
        }

        /// <summary>
        /// Обновляет конфиг
        /// </summary>
        public static void RewriteConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Instance, Formatting.Indented, settings));
        }
    }
}
