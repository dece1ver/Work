using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure.Extensions;
using eLog.ViewModels.Base;
using Newtonsoft.Json;

namespace eLog.Models
{
    public class DownTime : ViewModel
    {
        private Types _Type;
        private Relations _Relation;
        private DateTime _StartTime;
        private DateTime _EndTime;
        private string _StartTimeText;
        private string _EndTimeText;

        public enum Types
        {
            Maintenance,
            ToolSearching,
            Mentoring,
            ContactingDepartments,
            FixtureMaking,
            HardwareFailure,
            PartialSetup // не для отметки вручную
        }

        public enum Relations
        {
            Setup, Machining
        }

        public Types Type
        {
            get => _Type;
            set
            {
                if (Set(ref _Type, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public Relations Relation
        {
            get => _Relation;
            set => Set(ref _Relation, value);
        }

        public string Name => Type switch
        {
            Types.Maintenance => Text.DownTimes.Maintenance,
            Types.ToolSearching => Text.DownTimes.ToolSearching,
            Types.Mentoring => Text.DownTimes.Mentoring,
            Types.ContactingDepartments => Text.DownTimes.ContactingDepartments,
            Types.FixtureMaking => Text.DownTimes.FixtureMaking,
            Types.HardwareFailure => Text.DownTimes.HardwareFailure,
            Types.PartialSetup => Text.DownTimes.PartialSetup, // не для отметки вручную
            _ => throw new ArgumentOutOfRangeException()
        };

        public DateTime StartTime
        {
            get => _StartTime;
            private set
            {
                if (!Set(ref _StartTime, value)) return;
                // if (_StartTime > DateTime.MinValue) _StartTimeText = _StartTime.ToString(Text.DateTimeFormat);
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(InProgress));
            }
        }

        [JsonIgnore] 
        public string StartTimeText
        {
            get => _StartTimeText;
            set
            {
                _StartTimeText = value;
                StartTime = DateTime.TryParseExact(_StartTimeText, Text.DateTimeFormat, null, DateTimeStyles.None, out var startTime)
                    ? startTime
                    : DateTime.MinValue;
            }
        }

        public DateTime EndTime
        {
            get => _EndTime;
            private set
            {
                if (!Set(ref _EndTime, value)) return;
                // if (_EndTime > DateTime.MinValue) _EndTimeText = _EndTime.ToString(Text.DateTimeFormat);
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(InProgress));
            }
        }

        [JsonIgnore]
        public string EndTimeText
        {
            get => _EndTimeText;
            set
            {
                _EndTimeText = value;
                EndTime = DateTime.TryParseExact(_EndTimeText, Text.DateTimeFormat, null, DateTimeStyles.None, out var endTime)
                    ? endTime
                    : DateTime.MinValue;
            }
        }

        [JsonIgnore] public TimeSpan Time => EndTime - StartTime;
        [JsonIgnore] public bool InProgress => EndTime < StartTime;

        [JsonConstructor]
        public DownTime(Types type, Relations relation, DateTime startTime, DateTime endTime)
        {
            _Type = type;
            _StartTime = startTime;
            _StartTimeText = _StartTime.ToString(Text.DateTimeFormat);
            _EndTime = endTime;
            _EndTimeText = _EndTime.ToString(Text.DateTimeFormat);
            _Relation = relation;
        }

        public DownTime(Types type, Relations relation)
        {
            _Type = type;
            var now = DateTime.Now;
            _StartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            _StartTimeText = StartTime.ToString(Text.DateTimeFormat);
            _EndTime = DateTime.MinValue;
            _EndTimeText = string.Empty;
            _Relation = relation;
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="downTime">Источник</param>

        public DownTime(DownTime downTime)
        {
            _Type = downTime.Type;
            _StartTime = downTime.StartTime;
            _StartTimeText = downTime.StartTimeText;
            _EndTime = downTime.EndTime;
            _EndTimeText = downTime.EndTimeText;
            _Relation = downTime.Relation;
        }
    }
}
