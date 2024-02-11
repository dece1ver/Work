using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neLog.Infrastructure
{
    public static class Util
    {
        /// <summary>
        /// Записывает лог с сообщением
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(string message)
            => _ = Logs.Write(AppSettings.LogFile, message, GetCopyDir());

        /// <summary>
        /// Записывает лог с исключением и сообщением.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="additionalMessage"></param>
        public static void WriteLog(Exception exception, string additionalMessage = "")
            => _ = Logs.Write(AppSettings.LogFile, exception, additionalMessage, GetCopyDir());

        /// <summary>
        /// Получает директорию для копирования логов, которой является директория таблицы.
        /// </summary>
        /// <returns></returns>
        private static string GetCopyDir()
        {
            if (Directory.Exists(AppSettings.Instance.StorageTablePath) && Directory.GetParent(AppSettings.Instance.StorageTablePath) is { Parent: not null } parent)
            {
                return parent.FullName;
            }
            return "";
        }
    }
}
