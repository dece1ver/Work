using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using eLog.Models;
using Tomlyn;
using Tomlyn.Model;

namespace eLog.Infrastructure
{
    public static class AppSettings
    {
        public static readonly string LogReservedPath = "C:\\ProgramData\\dece1ver\\eLog\\FailedLogs";
        public static readonly string ConfigPath = "C:\\ProgramData\\dece1ver\\eLog";
        public static readonly string ConfigFilePath = Path.Combine(ConfigPath, "config.toml");
        public static Machine Machine { get; set; }
        public static string LogBasePath { get; set; }
        public static string CurrentOperator { get; set; }
        public static List<string> OperatorsList { get; set; }


        private static void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
            var tomlBaseContent = "machine_id = 0\r\nlog_base_path = \"\"\r\noperators = []\r\ncurrent_operator = \"\"\r\n";
            File.WriteAllText(ConfigFilePath, tomlBaseContent);
        }

        public static void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath)) CreateBaseConfig();
            var content = File.ReadAllText(ConfigFilePath);
            
            bool needRapair = false;

            try
            {
                var config = Toml.ToModel(content);
                try
                {
                    Machine = new Machine(Convert.ToInt32(config["machine_id"]!));
                }
                catch
                {
                    needRapair = true;
                    Machine = new Machine(0);
                }

                try
                {
                    LogBasePath = (string)config["log_base_path"]!;
                }
                catch
                {
                    needRapair = true;
                    LogBasePath = (string)config["log_base_path"]!;
                }

                try
                {

                    TomlArray operators = (TomlArray)config["operators"]!;
                    OperatorsList = operators.Select(s => s!.ToString()!).ToList();
                }
                catch
                {
                    needRapair = true;
                    OperatorsList = new List<string>();
                }

                try
                {
                    CurrentOperator = (string)config["current_operator"]!;
                    if (string.IsNullOrWhiteSpace(CurrentOperator) && OperatorsList.Count > 0) CurrentOperator = OperatorsList[0];
                }
                catch
                {
                    needRapair = true;
                    CurrentOperator = OperatorsList.Count > 0 ? OperatorsList[0] : "";
                }
            }
            catch
            {
                CreateBaseConfig();
                ReadConfig();
                return;
            }

            if (needRapair) RewriteConfig();
        }

        public static void RewriteConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            var tomlContent = $"machine_id = {Machine.Id}\r\nlog_base_path = \"{LogBasePath}\"\r\noperators = [\"{string.Join("\", \"", OperatorsList)}\"]\r\ncurrent_operator = \"{CurrentOperator}\"\r\n";
            File.WriteAllText(ConfigFilePath, tomlContent);
        }
    }


}
