using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class HelpCase
    {
        public enum Type
        {
            ToolSearch, LongSetup
        }

        /// <summary>
        /// Деталь к которой относится оказание помощи
        /// </summary>
        public Part Part { get; init; }

        /// <summary>
        /// Причина оказания помощи
        /// </summary>
        public Type Reason { get; init; }

        /// <summary>
        /// Время начала оказания помощи.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Время завершения оказания помощи.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Имя или фамилия помогающего работника.
        /// </summary>
        public string HelperName { get; set; }

        /// <summary>
        /// Комментарий к случаю помощи.
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Продолжительность оказания помощи в виде временного интервала.
        /// </summary>

        public TimeSpan Duration => EndTime - StartTime;
        /// <summary>
        /// Отправлено ли уведомление
        /// </summary>
        public bool Sended { get; set; }

        /// <summary>
        /// Создает новый случай оказания помощи.
        /// </summary>
        /// <param name="helperName">Имя или фамилия помогающего работника.</param>
        public HelpCase(Part part, Type reason, string helperName)
        {
            Part = part;
            Reason = reason;
            HelperName = helperName;
        }
    }
}
