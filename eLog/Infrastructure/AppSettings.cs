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
using Tomlyn;
using Tomlyn.Model;

namespace eLog.Infrastructure
{
    public static class AppSettings
    {
        public const string LogReservedPath = "C:\\ProgramData\\dece1ver\\eLog\\FailedLogs";
        public const string ConfigPath = "C:\\ProgramData\\dece1ver\\eLog";
        public static readonly string ConfigFilePath = Path.Combine(ConfigPath, "config.json");
        public static Machine Machine { get; set; }
        public static string LogBasePath { get; set; }
        public static ObservableCollection<Operator> Operators { get; set; }
        public static Operator? CurrentOperator { get; set; }
        
        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
            Machine = new Machine(0);
            LogBasePath = "";
            Operators = new ObservableCollection<Operator>() {
                new Operator() {
                    LastName = "Бабохин",
                    FirstName = "Кирилл",
                    Patronymic = "Георгиевич",
                    },
                };
            RewriteConfig();
        }

        public static void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                AppSettingsModel appSettings = JsonConvert.DeserializeObject<AppSettingsModel>(File.ReadAllText(ConfigFilePath));
                if (appSettings == null) throw new ArgumentNullException();
                Machine = appSettings.Machine;
                LogBasePath = appSettings.LogBasePath;
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

        public static void RewriteConfig()
        {
            var appSettings = new AppSettingsModel(Machine, LogBasePath, Operators, CurrentOperator);
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(appSettings, Formatting.Indented));
        }
    }
}
