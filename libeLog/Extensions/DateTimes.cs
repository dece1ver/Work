using libeLog.WinApi.Windows;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace libeLog.Extensions;

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
}
