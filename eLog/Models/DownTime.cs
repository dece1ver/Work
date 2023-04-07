using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure.Extensions;
using eLog.ViewModels.Base;

namespace eLog.Models
{
    public class DownTime : ViewModel
    {
        private Types _Type;
        private Relations _Relation;
        private DateTime _StartTime;
        private DateTime _EndTime;

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
            set
            {
                if (!Set(ref _StartTime, value)) return;
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(InProgress));
            }
        }

        public DateTime EndTime
        {
            get => _EndTime;
            set
            {
                if (!Set(ref _EndTime, value)) return;
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(InProgress));
            }
        }

        public TimeSpan Time => EndTime - StartTime;
        public bool InProgress => EndTime < StartTime;

        public DownTime(Types type, Relations relation)
        {
            Type = type;
            StartTime = DateTime.Now;
            EndTime = DateTime.MinValue;
            Relation = relation;
        }

    }
}
