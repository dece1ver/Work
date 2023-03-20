using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    public class Text
    {
        public const string WithoutOrderItem = " ";
        public const string WithoutOrderDescription = "Без М/Л";
        public const string DayShift = "День";
        public const string NightShift = "Ночь";
        public const string DateTimeFormat = "dd.MM.yyyy HH:mm";
        public const string TimeSpanFormat = @"hh\:mm\:ss";

        public const string Maintenance = "Обслуживание";
        public const string ToolSearching = "Поиск и получение инструмента";
        public const string Mentoring = "Помощь / наставничество / обучение";
        public const string ContactingDepartments = "Обращение в другие службы";
        public const string FixtureMaking = "Изготовление оснастки и калибров";
        public const string HardwareFailure = "Отказ оборудования";
        public const string PartialSetup = "Частичная наладка";

        public static readonly string[] Shifts = { DayShift, NightShift };
    }
}
