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
    }
}