using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Extensions
{
    public static class TimeOnlys
    {
        public static TimeSpan GetBreaksBetween(TimeOnly startTime, TimeOnly endTime, bool calcOnEnd = true) 
            => DateTimes.GetBreaksBetween(
                new DateTime(1, 1, 1, startTime.Hour, startTime.Minute, startTime.Second),
                new DateTime(1, 1, 1, endTime.Hour, endTime.Minute, endTime.Second), calcOnEnd);
    }
}
