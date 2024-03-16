﻿using Newtonsoft.Json;
using remeLog.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Threading;
using JsonException = System.Text.Json.JsonException;

namespace remeLog.Infrastructure
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings
    {
        private AppSettings()
        {

        }

        [JsonIgnore] private static AppSettings? _Instance;
        [JsonIgnore] public static AppSettings Instance => _Instance ??= new AppSettings();

        /// <summary> Директория для хранения всякого </summary>
        [JsonIgnore] public const string BasePath = "C:\\ProgramData\\dece1ver\\remeLog";

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigFilePath = Path.Combine(BasePath, "config.json");

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigBackupPath = Path.Combine(BasePath, "config.backup.json");

        /// <summary> Путь к файлу конфигурации </summary>
        [JsonIgnore] public static readonly string ConfigTempPath = Path.Combine(BasePath, "config.temp.json");

        /// <summary> Путь к файлу логов </summary>
        [JsonIgnore] public static readonly string LogFile = Path.Combine(BasePath, "log");

        [JsonIgnore]
        public static readonly string[] ShiftTypes = new string[] { "День", "Ночь" };

        [JsonIgnore]
        public static readonly string[] SetupReasons = new string[] {
        "",
        "Освоение",
        "Работа ученика",
        "Неопытный оператор",
        "Некорректное занесение",
        "Некорректные нормативы",
        "Отсутствие нормативов",
        "Небрежное отношение к работе",
        "Другое" };

        [JsonIgnore]
        public static readonly string[] ProductionReasons = new string[] {
        "",
        "Работа ученика",
        "Неопытный оператор",
        "Особенности изготовления",
        "Штучная/длительная работа",
        "Некорректное занесение",
        "Некорректные нормативы",
        "Отсутствие нормативов",
        "Небрежное отношение к работе",
        "Другое" };
        [JsonIgnore]
        public static readonly string[] UnspecifiedDowntimesReasons = new string[] {
        "",
        "Ремонт оборудования",
        "Отсутствие оператора",
        "Отсутствие электричества",
        "Другое" };

        /// <summary> Режим отладки </summary>
        public bool DebugMode { get; set; }
        /// <summary> Источник информации </summary>
        public DataSource DataSource { get; set; }

        /// <summary> Путь к файлу занесения </summary>
        public string? SourcePath { get; set; }

        /// <summary> Путь к файлу отчетов </summary>
        public string? ReportsPath { get; set; }

        /// <summary> Путь к директории с суточными отчетами </summary>
        public string? DailyReportsDir { get; set; }
        /// <summary> Строка подключения к БД </summary>
        public string? ConnectionString { get; set; }

        public bool InstantUpdateOnMainWindow { get; set; }


        /// <summary> Создает конфиг с параметрами по-умолчанию </summary>
        private void CreateBaseConfig()
        {
            if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
            if (File.Exists(ConfigBackupPath)) File.Delete(ConfigBackupPath);
            if (!Directory.Exists(BasePath)) Directory.CreateDirectory(BasePath);
            DataSource = new DataSource(DataSource.Types.Database);
            InstantUpdateOnMainWindow = false;
            SourcePath = "";
            ReportsPath = "";
            DailyReportsDir = "";
            ConnectionString = "";
            Save();
        }

        /// <summary>
        /// Читает конфиг, если возникает исключение, то создает конфиг по-умолчанию и читает его.
        /// </summary>
        public void ReadConfig()
        {
            if (!File.Exists(ConfigFilePath) && File.Exists(ConfigBackupPath))
            {
                File.Copy(ConfigBackupPath, ConfigFilePath, true);
            }
            else if (!File.Exists(ConfigFilePath) && !File.Exists(ConfigBackupPath))
            {
                CreateBaseConfig();
            }
            var json = File.ReadAllText(ConfigFilePath);
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                JsonConvert.PopulateObject(json, Instance, settings);
            }
            catch (Exception exception)
            {
                switch (exception)
                {
                    case Newtonsoft.Json.JsonException:
                        Util.WriteLog("Некорректный файл конфигурации.");
                        break;
                    default:
                        Util.WriteLog(exception, "Ошибка при чтении конфигурации.");
                        break;
                }

                if (File.Exists(ConfigFilePath)) File.Copy(ConfigFilePath, Path.Combine(BasePath, $"{DateTime.Now:dd-mm-yyyy-hh-mm-ss}_r"), true);

                if (File.Exists(ConfigBackupPath))
                {
                    try
                    {
                        JsonDocument.Parse(File.ReadAllText(ConfigBackupPath));
                        File.Copy(ConfigBackupPath, ConfigFilePath, true);
                    }
                    catch (JsonException)
                    {
                        var msg = "Резервный файл конфигурации некорректен, установка конфигурации по умолчанию.";
                        Util.WriteLog(msg);
                        CreateBaseConfig();
                    }
                    catch (Exception ex)
                    {
                        var msg = "Неизвестная ошибка при чтении резервного файла конфигурации, установка конфигурации по умолчанию.";
                        Util.WriteLog(ex, msg);
                        CreateBaseConfig();
                    }

                }
                else
                {
                    CreateBaseConfig();
                }
                ReadConfig();
            }
        }

        /// <summary> Сохраняет конфиг </summary>
        public static void Save()
        {

            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            try
            {
                if (File.Exists(ConfigFilePath)) File.Copy(ConfigFilePath, ConfigTempPath, true);
                if (File.Exists(ConfigFilePath)) File.Delete(ConfigFilePath);
                var json = JsonConvert.SerializeObject(Instance, Formatting.Indented, settings);
                File.WriteAllText(ConfigFilePath, json);
                if (File.Exists(ConfigTempPath)) File.Delete(ConfigTempPath);
                try
                {
                    JsonDocument.Parse(json);
                    File.Copy(ConfigFilePath, ConfigBackupPath, true);
                }
                catch (JsonException ex)
                {
                    var msg = "Записан некорректный файл конфигурации, восстановление";
                    Util.WriteLog(ex, msg);
                    File.Copy(ConfigBackupPath, ConfigFilePath, true);
                }
                catch (Exception ex)
                {
                    var msg = "Неизвестная ошибка при создании бэкапа конфигурации";
                    Util.WriteLog(ex, msg);
                }

            }
            catch (UnauthorizedAccessException)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Доступ запрещен).";
                Util.WriteLog(msg);
                if (!File.Exists(ConfigFilePath) && File.Exists(ConfigTempPath)) File.Copy(ConfigTempPath, ConfigFilePath, true);
            }
            catch (IOException)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Ошибка ввода/вывода).";
                Util.WriteLog(msg);
                if (!File.Exists(ConfigFilePath) && File.Exists(ConfigTempPath)) File.Copy(ConfigTempPath, ConfigFilePath, true);
            }
            catch (Exception ex)
            {
                var msg = "Ошибка при сохранении файла конфигурации (Неизвестная ошибка).";
                Util.WriteLog(ex, msg);
                try
                {
                    if (File.Exists(ConfigTempPath)) File.Copy(ConfigTempPath, ConfigFilePath, true);
                }
                catch { }

                if (File.Exists(ConfigTempPath)) File.Copy(ConfigTempPath, Path.Combine(BasePath, $"{DateTime.Now:dd-mm-yyyy-hh-mm-ss}_w"), true);
                if (File.Exists(ConfigFilePath)) Debug.WriteLine("Восстановлен бэкап конфигурации.");
            }
        }
    }
}