using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    public static class Util
    {
        public static void WriteLog(string message) 
            => Logs.Write(AppSettings.LogFile, message, GetCopyDir());

        public static void WriteLog(Exception exception, string additionalMessage = "") 
            => Logs.Write(AppSettings.LogFile, exception, additionalMessage, GetCopyDir());

        /// <summary>
    /// Получает директорию для копирования логов, которой является директория таблицы.
    /// </summary>
    /// <returns></returns>
    private static string GetCopyDir()
    {
        if (Directory.Exists(AppSettings.Instance.SourcePath) && Directory.GetParent(AppSettings.Instance.SourcePath) is { Parent: not null} parent)
        {
            return parent.FullName;
        }
        return "";
    }
    }
}
