using System;

namespace eLog.Infrastructure.Extensions;

public static class WorkTime
{
    public static readonly DateTime DayShiftFirstBreak = new(1, 1, 1, 9, 0, 0);
    public static readonly DateTime DayShiftSecondBreak = new(1, 1, 1, 12, 30, 0);
    public static readonly DateTime DayShiftThirdBreak = new(1, 1, 1, 15, 15, 0);
    public static readonly DateTime NightShiftFirstBreak = new(1, 1, 1, 22, 30, 0);
    public static readonly DateTime NightShiftSecondBreak = new(1, 1, 1, 1, 30, 0);
    public static readonly DateTime NightShiftThirdBreak = new(1, 1, 1, 4, 30, 0);
}
