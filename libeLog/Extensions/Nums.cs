using System;

namespace libeLog.Extensions
{
    public static class Nums
    {
        public static string FormattedHours(this int hours)
        {
            if (hours < 0) throw new ArgumentException("Значение должно быть положительным");
            if (hours % 100 >= 11 && hours % 100 <= 19)
            {
                return "часов";
            }

            int lastDigit = hours % 10;

            var word = lastDigit switch
            {
                1 => "час",
                2 => "часа",
                3 => "часа",
                4 => "часа",
                _ => "часов"
            };
            return $"{hours} {word}";
        }

        public static string FormattedMinutes(this int minutes)
        {
            if (minutes < 0) throw new ArgumentException("Значение должно быть положительным");
            if (minutes % 100 >= 11 && minutes % 100 <= 19)
            {
                return $"{minutes} минут";
            }
            int lastDigit = minutes % 10;
            var word = lastDigit switch
            {
                1 => "минута",
                2 => "минуты",
                3 => "минуты",
                4 => "минуты",
                _ => "минут"
            };
            return $"{minutes} {word}";
        }
    }
}