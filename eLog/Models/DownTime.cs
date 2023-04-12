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
using System.ComponentModel.DataAnnotations;

namespace eLog.Models
{
    public class DownTime : ViewModel, IDataErrorInfo
    {
        private Types _Type;
        private DateTime _StartTime;
        private DateTime _EndTime;
        private string _StartTimeText;
        private string _EndTimeText;
        private PartInfoModel _ParentPart;

        public PartInfoModel ParentPart
        {
            get => _ParentPart;
            set
            {
                if (Set(ref _ParentPart, value))
                {
                    OnPropertyChanged(nameof(Relation));
                }
            }
        }

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
            Setup, Machining, None
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
            get
            {
                if (StartTime == DateTime.MinValue && StartTime < ParentPart.StartSetupTime) return Relations.None;
                return ParentPart.SetupIsFinished && _StartTime >= ParentPart.StartMachiningTime
                    ? Relations.Machining
                    : Relations.Setup;
            }
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
                if (_StartTime == DateTime.MinValue || _StartTime < ParentPart.StartSetupTime) return;
                OnPropertyChanged(nameof(EndTimeText));
                OnPropertyChanged(nameof(Relation));
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
                OnPropertyChanged(nameof(StartTimeText));
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
        public DownTime(PartInfoModel part, Types type, DateTime startTime, DateTime endTime)
        {
            _ParentPart = part;
            _Type = type;
            _StartTime = startTime;
            _StartTimeText = _StartTime.ToString(Text.DateTimeFormat);
            _EndTime = endTime;
            _EndTimeText = _EndTime.ToString(Text.DateTimeFormat);
        }

        public DownTime(PartInfoModel part, Types type)
        {
            _ParentPart = part;
            _Type = type;
            var now = DateTime.Now;
            _StartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            _StartTimeText = StartTime.ToString(Text.DateTimeFormat);
            _EndTime = DateTime.MinValue;
            _EndTimeText = string.Empty;
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="part">Деталь к которой относится простой</param>
        /// <param name="downTime">Источник</param>

        public DownTime(PartInfoModel part, DownTime downTime)
        {
            _ParentPart = part;
            _Type = downTime.Type;
            _StartTime = downTime.StartTime;
            _StartTimeText = downTime.StartTimeText;
            _EndTime = downTime.EndTime;
            _EndTimeText = downTime.EndTimeText;
        }
        
        public string this[string columnName]
        {
            get
            {
                string error = null!;
                switch (columnName)
                {
                    case nameof(StartTimeText):
                    {
                        if (StartTime == DateTime.MinValue)
                        {
                            error = "Некорректно указано время начала";
                        }
                        else if (StartTime < ParentPart.StartSetupTime)
                        {
                            error = "Время начала простоя не может быть раньше времени запуска детали.";
                        }
                        else if (EndTime != DateTime.MinValue && EndTime <= StartTime)
                        {
                            error = "Время начала простоя раньше времени завершения.";
                        }
                        break;
                    }
                    case nameof(EndTimeText):
                        if (EndTime != DateTime.MinValue && EndTime <= StartTime)
                        {
                            error = "Время завершения простоя не может быть раньше времени начала.";
                        }
                        else if (Relation is Relations.Setup && ParentPart.SetupIsFinished &&
                                 EndTime > ParentPart.StartMachiningTime)
                        {
                            error =
                                "Время завершения простоя не может быть позже времени завершения наладки, т.к. он открыт в наладке";
                        }
                        else if (Relation is Relations.Machining && ParentPart.IsFinished == PartInfoModel.State.Finished &&
                                 EndTime > ParentPart.EndMachiningTime)
                        {
                            error = "Время завершения простоя не может быть позже времени завершения заготовления детали.";
                        }
                        else if (Relation is Relations.Machining && ParentPart.IsFinished == PartInfoModel.State.InProgress &&
                                 EndTime > DateTime.Now)
                        {
                            error = "Время завершения простоя не может быть позже текущего времени, т.к. изготовление детали не завершено.";
                        }
                        break;
                }
                Debug.WriteLine(error);
                return error;
            }
        }

        public string Error => null!;
    }
}
