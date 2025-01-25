using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Models
{
    /// <summary>
    /// Результат проверки доступности файла
    /// </summary>
    public enum FileCheckResult
    {
        /// <summary> Файл доступен для чтения </summary>
        Success,
        /// <summary> Файл не найден по указанному пути </summary>
        FileNotFound,
        /// <summary> Нет прав доступа к файлу </summary>
        AccessDenied,
        /// <summary> Файл занят другим процессом </summary>
        FileInUse,
        /// <summary> Общая ошибка ввода-вывода </summary>
        GeneralError,
        /// <summary> Недопустимый путь к файлу </summary>
        InvalidPath
    }
}
