﻿using System;
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
            AttendingTheEvent,
            FixtureMaking,
            EquipmentFailure
        }

        public Types Type { get; set; }

        public string Name => Type switch
        {
            Types.Maintenance => "Обслуживание",
            Types.WarmingUp => "Прогрев",
            Types.ToolSearching => "Поиск и получение инструмента",
            Types.Mentoring => "Помощь / наставничество / обучение",
            Types.AttendingTheEvent => "Мероприятия",
            Types.FixtureMaking => "Изготовление оснастки и калибров",
            Types.EquipmentFailure => "Отказ оборудования",
            _ => throw new ArgumentOutOfRangeException()
        };

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Time => EndTime - StartTime;
        public bool IsFinished => EndTime > StartTime;

        public DownTime(Types type)
        {
            Type = type;
            StartTime = DateTime.Now;
            EndTime = DateTime.MinValue;
        }
    }
}