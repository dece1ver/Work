using libeLog.WinApi.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Extensions;

public static class Chars
{
    public static Keys DKeyFromChar(this char ch)
    {
        return ch switch
        {
            '0' => Keys.D0,
            '1' => Keys.D1,
            '2' => Keys.D2,
            '3' => Keys.D3,
            '4' => Keys.D4,
            '5' => Keys.D5,
            '6' => Keys.D6,
            '7' => Keys.D7,
            '8' => Keys.D8,
            '9' => Keys.D9,
            _ => throw new ArgumentException("Переданный символ не является арабской цифрой."),
        };
    }
}
