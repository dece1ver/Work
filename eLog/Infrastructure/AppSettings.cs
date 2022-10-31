using System;
using System.Collections.Generic;
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
    public class AppSettings
    {
        private static AppSettings appSettings;
        private static bool needRewrite = false;

        protected AppSettings()
        {
            ReadConfig();
        }
        
        public const string LogReservedPath = "C:\\ProgramData\\dece1ver\\eLog\\FailedLogs";
        public const string ConfigPath = "C:\\ProgramData\\dece1ver\\eLog";
        public static readonly string ConfigFilePath = Path.Combine(ConfigPath, "config.json");
        public static Machine Machine { get; set; }
        public static string LogBasePath { get; set; }
        public static List<Operator> Operators { get; set; }
        public static Operator CurrentOperator { get; set; }
        


        public static AppSettings GetInstance()
        {
            appSettings ??= new AppSettings();
            if (needRewrite) RewriteConfig();
            return appSettings;
        }


        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
            Machine = new Machine(0);
            LogBasePath = "";
            Operators = new List<Operator>() {new Operator("Бабохин", "Кирилл", "Георгиевич")};
        }

        public static void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            
            try
            {
                appSettings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(ConfigFilePath));
                if (appSettings == null) throw new ArgumentNullException();
            }
            catch
            {
                needRewrite = true;
            }
        }

        public static void RewriteConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(appSettings));
        }
    }
}
