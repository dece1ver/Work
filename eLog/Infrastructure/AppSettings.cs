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
using System.Windows.Interop;
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
    public class AppSettings : INotifyPropertyChanged
    {
        private AppSettings()
        {
            
        }

        [JsonIgnore] private static AppSettings? _Instance;
        [JsonIgnore] public static AppSettings Instance => _Instance ??= new AppSettings();

        /// <summary> Директория для хранения всякого </summary>
        [JsonIgnore] public const string BasePath = "C:\\ProgramData\\dece1ver\\eLog";

        /// <summary> Локальный путь для бэкапа таблицы. </summary>
        [JsonIgnore] public static readonly string XlReservedPath = Path.Combine(BasePath, "backup.xlsx");

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigFilePath = Path.Combine(BasePath, "config.json");

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigBackupPath = Path.Combine(BasePath, "config.backup.json");

        /// <summary> Путь к локальному списку заказов </summary>
        [JsonIgnore] public static readonly string LocalOrdersFile = Path.Combine(BasePath, "orders.xlsx");

        /// <summary> Путь к резервному списку заказов </summary>
        [JsonIgnore] public static readonly string BackupOrdersFile = Path.Combine(BasePath, "orders-backup.xlsx");

        /// <summary> Путь к файлу логов </summary>
        [JsonIgnore] public static readonly string LogFile = Path.Combine(BasePath, "log");

        private Machine _Machine;
        private string _XlPath;
        private string _OrdersSourcePath;
        private string[] _OrderQualifiers;
        private DeepObservableCollection<Operator> _Operators;
        private string _CurrentShift;
        private DeepObservableCollection<Part> _Parts;
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
        public DeepObservableCollection<Operator> Operators
        {
            get => _Operators;
            set => Set(ref _Operators, value);
        }

        /// <summary> Текущий оператор </summary>
        public Operator? CurrentOperator
        {
            get => _CurrentOperator;
            set
                => Set(ref _CurrentOperator, value);
        }
        /// <summary> Запущена ли смена </summary>
        public bool IsShiftStarted
        {
            get => _IsShiftStarted;
            set => Set(ref _IsShiftStarted, value);
        }
        /// <summary> Текущая смена </summary>
        public string CurrentShift
        {
            get => _CurrentShift;
            set => Set(ref _CurrentShift, value);
        }
        /// <summary> Список деталей </summary>
        public DeepObservableCollection<Part> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }

        
        

        /// <summary> Создает конфиг с параметрами по-умолчанию </summary>
        private void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            Machine = new Machine(0);
            Operators = new DeepObservableCollection<Operator>()
            {
                new()
                {
                    LastName = "Бабохин",
                    FirstName = "Кирилл",
                    Patronymic = "Георгиевич"
                },
            };
            CurrentShift = Text.DayShift;
            Parts = new DeepObservableCollection<Part>();
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

        /// <summary>
        /// Читает конфиг, если возникает исключение, то создает конфиг по-умолчанию и читает его.
        /// </summary>
        public void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects, 
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                var json = File.ReadAllText(ConfigFilePath);
                JsonConvert.PopulateObject(json, Instance, settings);
                //Parts.CollectionChanged += (_, _) => Save();
                //foreach (var part in Parts)
                //{
                //    part.PropertyChanged += SaveEvent;
                //    foreach (var downTime in part.DownTimes)
                //    {
                //        downTime.PropertyChanged += (_, _) => Save();
                //    }
                //}
            }
            catch (Exception exception)
            {
                Util.WriteLog(exception, "Ошибка при чтении конфигурации.");
                if (File.Exists(ConfigFilePath)) File.Copy(ConfigFilePath, Path.Combine(BasePath, DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss"), "_r"), true);
                CreateBaseConfig();
                ReadConfig();
            }
        }

        /// <summary>
        /// Обновляет конфиг
        /// </summary>
        public static void Save()
        {
            if (File.Exists(ConfigFilePath)) File.Copy(ConfigFilePath, ConfigBackupPath, true);
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            try
            {
                File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Instance, Formatting.Indented, settings));
                Debug.WriteLine("Записаны настройки");
            }
            catch (UnauthorizedAccessException ex)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Доступ запрещен).";
                Debug.WriteLine(msg);
                Util.WriteLog(ex, msg);
            }
            catch (IOException ex)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Ошибка ввода/вывода).";
                Debug.WriteLine(msg);
                Util.WriteLog(ex, msg);
            }
            catch (Exception ex)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Неизвестная ошибка).";
                Debug.WriteLine(msg);
                Util.WriteLog(ex, msg);
                if (File.Exists(ConfigBackupPath)) File.Copy(ConfigBackupPath, Path.Combine(BasePath, DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss"), "_w"), true);
                if (File.Exists(ConfigBackupPath)) File.Copy(ConfigBackupPath, ConfigFilePath, true);
                if (File.Exists(ConfigFilePath)) Debug.WriteLine("Восстановлен бэкап конфигурации.");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
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
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            Save();
            Debug.WriteLine("Rewrite config from Set notification");
            return true;
        }
    }
}
