using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class DownTime
    {
        public enum Types
        {
            Maintenance,
            WarmingUp,
            ToolSearching,
            Mentoring,
            ContactingDepartments,
            FixtureMaking,
            HardwareFailure
        }

        public Types Type { get; set; }

        public string Name => Type switch
        {
            Types.Maintenance => "Обслуживание",
            Types.WarmingUp => "Прогрев",
            Types.ToolSearching => "Поиск и получение инструмента",
            Types.Mentoring => "Помощь / наставничество / обучение",
            Types.ContactingDepartments => "Участие в мероприятиях",
            Types.FixtureMaking => "Изготовление оснастки и калибров",
            Types.HardwareFailure => "Отказ оборудования",
            _ => throw new ArgumentOutOfRangeException()
        };

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Time => EndTime - StartTime;
        public bool InProgress => EndTime < StartTime;

        public DownTime(Types type)
        {
            Type = type;
            StartTime = DateTime.Now;
            EndTime = DateTime.MinValue;
        }
    }
}
