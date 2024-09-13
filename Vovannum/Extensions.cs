using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Vovannum
{
    public static class Extensions
    {
        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public static bool GetBit(this int b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public static void Log(this Exception ex, string message = "")
        {
            if (ex == null) return;

            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLine("=== Ошибка ===");
                writer.WriteLine($"Время: {DateTime.Now}");
                writer.WriteLine($"Сообщение: {ex.Message}");
                writer.WriteLine($"Стек вызовов: {ex.StackTrace}");
                if (!string.IsNullOrEmpty(message)) {writer.WriteLine($"Сообщение: {message}");}
                writer.WriteLine();
            }
        }
    }
}
