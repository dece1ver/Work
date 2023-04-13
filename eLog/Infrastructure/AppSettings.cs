using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using eLog.Annotations;
using eLog.Infrastructure.Extensions;
using eLog.Models;
using eLog.ViewModels.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace eLog.Infrastructure
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings : ViewModel
    {
        private AppSettings()
        {
            Parts.CollectionChanged += (s, e) => Save();
            foreach (var part in Parts)
            {
                part.PropertyChanged += (s, e) => Save();
            }
        }

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

        private Machine _Machine = new (0);
        private string _XlPath = string.Empty;
        private string _OrdersSourcePath = string.Empty;
        private string[] _OrderQualifiers = Array.Empty<string>();
        private ObservableCollection<Operator> _Operators = new();
        private string _CurrentShift = string.Empty;
        private ObservableCollection<PartInfoModel> _Parts = new();
        private bool _IsShiftStarted;
        private Operator? _CurrentOperator;

        /// <summary> Текущий станок </summary>
        public Machine Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        /// <summary> Путь к общей таблице </summary>
        public string XlPath
        {
            get => _XlPath;
            set => Set(ref _XlPath, value);
        }

        /// <summary> Путь к таблице с номенклатурой </summary>
        public string OrdersSourcePath
        {
            get => _OrdersSourcePath;
            set => Set(ref _OrdersSourcePath, value);
        }

        /// <summary> Путь к таблице с номенклатурой </summary>
        public string[] OrderQualifiers
        {
            get => _OrderQualifiers;
            set => Set(ref _OrderQualifiers, value);
        }

        /// <summary> Список операторов </summary>
        public ObservableCollection<Operator> Operators
        {
            get => _Operators;
            set => Set(ref _Operators, value);
        }

        /// <summary> Текущая смена </summary>
        public string CurrentShift
        {
            get => _CurrentShift;
            set => Set(ref _CurrentShift, value);
        }

        /// <summary> Список деталей </summary>
        public ObservableCollection<PartInfoModel> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }

        /// <summary> Запущена ли смена </summary>
        public bool IsShiftStarted
        {
            get => _IsShiftStarted;
            set => Set(ref _IsShiftStarted, value);
        }

        /// <summary> Текущий оператор </summary>
        public Operator? CurrentOperator
        {
            get => _CurrentOperator;
            set
            => Set(ref _CurrentOperator, value);
        }

        /// <summary> Создает конфиг с параметрами по-умолчанию </summary>
        private void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            Machine = new Machine(0);
            Operators = new ObservableCollection<Operator>()
            {
                new()
                {
                    LastName = "Бабохин",
                    FirstName = "Кирилл",
                    Patronymic = "Георгиевич"
                },
            };
            CurrentShift = Text.DayShift;
            Parts = new ObservableCollection<PartInfoModel>();
            XlPath = string.Empty;
            OrdersSourcePath = string.Empty;
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
            };
            IsShiftStarted = false;
            Save();
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
        /// Читает конфиг, если возникает исключение, то создает конфиг по-умолчанию и читает его.
        /// </summary>
        public void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                JsonConvert.PopulateObject(json, this);
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
        public static void Save()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Instance, Formatting.Indented, settings));
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
        {
            var handlers = PropertyChanged;
            if (handlers is null) return;

            var invokationList = handlers.GetInvocationList();
            var args = new PropertyChangedEventArgs(PropertyName);

            foreach (var action in invokationList)
            {
                if (action.Target is DispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action, this, args);
                }
                else
                {
                    action.DynamicInvoke(this, args);
                }
            }
            Save();
            Debug.WriteLine("Rewrite config from PropertyChanged");
            // проверить
        }
    }
}
