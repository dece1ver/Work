using libeLog.WinApi.Windows;
using System;
using static libeLog.Constants;

namespace libeLog.Extensions
{
    public static class DateTimes
    {
        /// <summary>
        /// Округляет DateTime с точностью до минут
        /// </summary>
        /// <param name="value">Исходное время</param>
        /// <returns></returns>
        public static DateTime Rounded(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
        }

        /// <summary>
        /// Получить массив клавиш для набора времени
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static Keys[] GetKeys(this DateTime dateTime)
        {
            // d  d  .  M  M  .  y  y  y  y     H  H  :  m  m
            // 00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15
            var text = dateTime.ToString(Constants.DateTimeFormat);
            var res = new Keys[16];
            for (var i = 0; i < 16; i++)
            {
                res[i] = i switch
                {
                    2 or 5 => KeyboardLayout.Current is KeyboardLayout.En ? Keys.OemPeriod : Keys.Oem2,
                    10 => Keys.Space,
                    13 => KeyboardLayout.Current is KeyboardLayout.En ? Keys.Oem1 : Keys.D6,
                    _ => text[i].DKeyFromChar()
                };
            }
            return res;
        }

        /// <summary>
        /// Суммарное время перерывов между двумя датами.
        /// </summary>
        /// <param name="startDateTime">Начало</param>
        /// <param name="endDateTime">Завершение</param>
        /// <param name="calcOnEnd">Считать по времени завершения перерывов. Это вариант по-умолчанию для выполняемых действий, по началам перерывов считается при расчете планируемого времени.</param>
        /// <returns>TimeSpan с суммарным временем перерывов</returns>
        public static TimeSpan GetBreaksBetween(DateTime startDateTime, DateTime endDateTime, bool calcOnEnd = true)
        {
            var dayShiftFirstBreak = WorkTime.DayShiftFirstBreak;
            var dayShiftSecondBreak = WorkTime.DayShiftSecondBreak;
            var dayShiftThirdBreak = WorkTime.DayShiftThirdBreak;
            var nightShiftFirstBreak = WorkTime.NightShiftFirstBreak;
            var nightShiftSecondBreak = WorkTime.NightShiftSecondBreak;
            var nightShiftThirdBreak = WorkTime.NightShiftThirdBreak;

            if (!calcOnEnd)
            {
                dayShiftFirstBreak = dayShiftFirstBreak.AddMinutes(-14);
                dayShiftSecondBreak = dayShiftSecondBreak.AddMinutes(-29);
                dayShiftThirdBreak = dayShiftThirdBreak.AddMinutes(-14);
                nightShiftFirstBreak = nightShiftFirstBreak.AddMinutes(-29);
                nightShiftSecondBreak = nightShiftSecondBreak.AddMinutes(-29);
                nightShiftThirdBreak = nightShiftSecondBreak.AddMinutes(-29);
            }

            var breaks = TimeSpan.Zero;
            var startTime = new DateTime(1, 1, 1, startDateTime.Hour, startDateTime.Minute, startDateTime.Second);
            var endTime = new DateTime(1, 1, 1, endDateTime.Hour, endDateTime.Minute, endDateTime.Second);
            if (startTime > endTime)
            {
                nightShiftSecondBreak = nightShiftSecondBreak.AddDays(1);
                nightShiftThirdBreak = nightShiftThirdBreak.AddDays(1);
                endTime = endTime.AddDays(1);
            }

            if (dayShiftFirstBreak > startTime && dayShiftFirstBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(15);
                if (!calcOnEnd) endTime += TimeSpan.FromMinutes(15);
            }
            if (dayShiftSecondBreak > startTime && dayShiftSecondBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(30);
                if (!calcOnEnd) endTime += TimeSpan.FromMinutes(30);
            }
            if (dayShiftThirdBreak > startTime && dayShiftThirdBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(15);
                if (!calcOnEnd) endTime += TimeSpan.FromMinutes(15);
            }
            if (nightShiftFirstBreak > startTime && nightShiftFirstBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(30);
                if (!calcOnEnd) endTime += TimeSpan.FromMinutes(30);
            }
            if (nightShiftSecondBreak > startTime && nightShiftSecondBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(30);
                if (!calcOnEnd) endTime += TimeSpan.FromMinutes(30);
            }
            if (nightShiftThirdBreak > startTime && nightShiftThirdBreak <= endTime)
            {
                breaks += TimeSpan.FromMinutes(30);
            }

            return breaks;
        }

        public static double GetPartialBreakBetween(DateTime startDateTime, DateTime endDateTime)
        {
            if (endDateTime == DateTime.MinValue) return 0;
            var dayShiftFirstBreakDuration = 15;
            var dayShiftFirstBreak = WorkTime.DayShiftFirstBreak.AddMinutes(-dayShiftFirstBreakDuration);

            var dayShiftSecondBreakDuration = 30;
            var dayShiftSecondBreak = WorkTime.DayShiftSecondBreak.AddMinutes(-dayShiftSecondBreakDuration);

            var dayShiftThirdBreakDuration = 15;
            var dayShiftThirdBreak = WorkTime.DayShiftThirdBreak.AddMinutes(-dayShiftThirdBreakDuration);

            var nightShiftFirstBreakDuration = 30;
            var nightShiftFirstBreak = WorkTime.NightShiftFirstBreak.AddMinutes(-nightShiftFirstBreakDuration);

            var nightShiftSecondBreakDuration = 30;
            var nightShiftSecondBreak = WorkTime.NightShiftSecondBreak.AddMinutes(-nightShiftSecondBreakDuration);

            var nightShiftThirdDuration = 30;
            var nightShiftThirdBreak = WorkTime.NightShiftThirdBreak.AddMinutes(-nightShiftThirdDuration);

            var startTime = new DateTime(1, 1, 1, startDateTime.Hour, startDateTime.Minute, startDateTime.Second);
            var endTime = new DateTime(1, 1, 1, endDateTime.Hour, endDateTime.Minute, endDateTime.Second);
            if (startTime > endTime)
            {
                nightShiftSecondBreak = nightShiftSecondBreak.AddDays(1);
                nightShiftThirdBreak = nightShiftThirdBreak.AddDays(1);
                endTime = endTime.AddDays(1);
            }

            var breaks = 0.0;

            breaks += GetPartial(startTime, endTime, dayShiftFirstBreak, dayShiftFirstBreakDuration);
            breaks += GetPartial(startTime, endTime, dayShiftSecondBreak, dayShiftSecondBreakDuration);
            breaks += GetPartial(startTime, endTime, dayShiftThirdBreak, dayShiftThirdBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftFirstBreak, nightShiftFirstBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftSecondBreak, nightShiftSecondBreakDuration);
            breaks += GetPartial(startTime, endTime, nightShiftThirdBreak, nightShiftThirdDuration);

            return breaks;
        }

        private static double GetPartial(DateTime startDateTime, DateTime endDateTime, DateTime breakTime, double duration)
        {
            var result = 0.0;
            var endBreakTime = breakTime.AddMinutes(duration);
            if (startDateTime < breakTime && endDateTime > breakTime && endDateTime <= endBreakTime)
            {
                result = (endDateTime - breakTime).TotalMinutes;
            }
            else if (startDateTime > breakTime && startDateTime < endBreakTime && endDateTime >= endBreakTime)
            {
                result = (endBreakTime - startDateTime).TotalMinutes;
            }
            else if (startDateTime > breakTime && endDateTime < endBreakTime)
            {
                result = (endDateTime - startDateTime).TotalMinutes;
            }
            else if (startDateTime <= breakTime && endDateTime >= endBreakTime)
            {
                result = (endBreakTime - breakTime).TotalMinutes;
            }
            return result;
        }
    }
}