using System;

namespace libeLog.Extensions
{
    public static class Nums
    {
        /// <summary>
        /// Возвращает строку, представляющую количество часов в правильной числовой форме.
        /// </summary>
        /// <param name="hours">Количество часов.</param>
        /// <returns>Строка с количеством часов и правильным склонением слова "час".</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если значение отрицательное.</exception>
        public static string FormattedHours(this int hours)
        {
            if (hours < 0)
                throw new ArgumentException("Значение должно быть положительным");

            if (hours % 100 >= 11 && hours % 100 <= 19)
            {
                return $"{hours} часов";
            }

            int lastDigit = hours % 10;
            var word = lastDigit switch
            {
                1 => "час",
                2 or 3 or 4 => "часа",
                _ => "часов"
            };
            return $"{hours} {word}";
        }

        /// <summary>
        /// Возвращает строку, представляющую количество минут в правильной числовой форме.
        /// </summary>
        /// <param name="minutes">Количество минут.</param>
        /// <returns>Строка с количеством минут и правильным склонением слова "минута".</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если значение отрицательное.</exception>
        public static string FormattedMinutes(this int minutes)
        {
            if (minutes < 0)
                throw new ArgumentException("Значение должно быть положительным");

            if (minutes % 100 >= 11 && minutes % 100 <= 19)
            {
                return $"{minutes} минут";
            }

            int lastDigit = minutes % 10;
            var word = lastDigit switch
            {
                1 => "минута",
                2 or 3 or 4 => "минуты",
                _ => "минут"
            };
            return $"{minutes} {word}";
        }


        /// <summary>
        /// Возвращает строку, представляющую количество запусков в правильной числовой форме.
        /// </summary>
        /// <param name="launches">Количество запусков.</param>
        /// <param name="useGenitive">Использовать родительный падеж (по умолчанию false - именительный падеж).</param>
        /// <returns>Строка с количеством запусков и правильным склонением слова "запуск".</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если значение отрицательное.</exception>
        public static string FormattedLaunches(this int launches, bool useGenitive = false)
        {
            if (launches < 0)
                throw new ArgumentException("Значение должно быть положительным");

            if (launches % 100 >= 11 && launches % 100 <= 19)
            {
                return $"{launches} запусков";
            }

            int lastDigit = launches % 10;
            var word = lastDigit switch
            {
                1 => useGenitive ? "запуска" : "запуск",
                2 or 3 or 4 => useGenitive ? "запусков" : "запуска",
                _ => "запусков"
            };

            return $"{launches} {word}";
        }
    }
}