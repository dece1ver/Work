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

        public static readonly string[] Shifts = { DayShift, NightShift };
    }
}
