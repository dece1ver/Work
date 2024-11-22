using eLog.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eLog.Infrastructure
{
    /// <summary>
    /// Менеджер сохранения настроек с защитой от повреждения файла
    /// </summary>
    public class SettingsManager
    {
        private readonly object _lockObject = new();
        private readonly System.Timers.Timer _saveTimer;
        private volatile bool _needsSaving;
        private const int SAVE_DELAY_MS = 1000;

        public SettingsManager()
        {
            _saveTimer = new System.Timers.Timer(SAVE_DELAY_MS);
            _saveTimer.Elapsed += (s, e) => SaveIfNeeded();
            _saveTimer.AutoReset = false;
        }

        public void ScheduleSave()
        {
            _needsSaving = true;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveIfNeeded()
        {
            if (!_needsSaving) return;

            lock (_lockObject)
            {
                if (!_needsSaving) return;

                try
                {
                    var tempPath = Path.Combine(AppSettings.BasePath, $"config.temp.{Guid.NewGuid()}.json");

                    var settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    };

                    var json = JsonConvert.SerializeObject(AppSettings.Instance, Formatting.Indented, settings);
                    File.WriteAllText(tempPath, json);

                    try
                    {
                        JsonDocument.Parse(json);
                    }
                    catch
                    {
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        throw;
                    }

                    if (File.Exists(AppSettings.ConfigFilePath))
                    {
                        File.Copy(AppSettings.ConfigFilePath, AppSettings.ConfigBackupPath, true);
                    }

                    if (File.Exists(AppSettings.ConfigFilePath))
                    {
                        File.Replace(tempPath, AppSettings.ConfigFilePath, AppSettings.ConfigBackupPath);
                    }
                    else
                    {
                        File.Move(tempPath, AppSettings.ConfigFilePath);
                    }

                    _needsSaving = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving settings: {ex.Message}");
                    Task.Run(() => Util.WriteLogAsync(ex, "Ошибка при сохранении настроек"));

                    if (!IsValidJsonFile(AppSettings.ConfigFilePath) &&
                        IsValidJsonFile(AppSettings.ConfigBackupPath))
                    {
                        try
                        {
                            File.Copy(AppSettings.ConfigBackupPath, AppSettings.ConfigFilePath, true);
                            Debug.WriteLine("Восстановлен бэкап конфигурации");
                        }
                        catch (Exception backupEx)
                        {
                            Debug.WriteLine($"Failed to restore backup: {backupEx.Message}");
                        }
                    }
                }
            }
        }

        private bool IsValidJsonFile(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                using var stream = File.OpenRead(path);
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                while (jsonReader.Read()) { }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
