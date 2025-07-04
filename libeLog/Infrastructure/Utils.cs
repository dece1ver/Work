using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Infrastructure
{
    public static class Utils
    {
        public static string ConvertColumnIndexToLetters(int columnIndex)
        {
            if (columnIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index cannot be negative.");

            string letters = "";
            while (columnIndex >= 0)
            {
                int remainder = columnIndex % 26;
                letters = (char)('A' + remainder) + letters;
                columnIndex = (columnIndex / 26) - 1;
            }
            return letters;
        }

        
    }
}
