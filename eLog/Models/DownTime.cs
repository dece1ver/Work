using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure.Extensions;

namespace eLog.Models
{
    public class DownTime
    {
        public enum Types
        {
            Maintenance,
            ToolSearching,
            Mentoring,
            ContactingDepartments,
            FixtureMaking,
            HardwareFailure,
            PartialSetup // используется для игнорирования времени наладки при частичном выполнении наладки
        }

        public enum Relations
        {
            Setup, Machining
        }

        public Types Type { get; set; }
        public Relations Relation { get; set; }

        public string Name => Type switch
        {
            Types.Maintenance => Text.Maintenance,
            Types.ToolSearching => Text.ToolSearching,
            Types.Mentoring => Text.Mentoring,
            Types.ContactingDepartments => Text.ContactingDepartments,
            Types.FixtureMaking => Text.FixtureMaking,
            Types.HardwareFailure => Text.HardwareFailure,
            Types.PartialSetup => Text.PartialSetup,
            _ => throw new ArgumentOutOfRangeException()
        };

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
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
