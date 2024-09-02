using System;
using System.Reflection;
using System.Threading;

namespace Vovannum
{
    class Program
    {
        static ushort _handle = 0;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Utils.LogError(e.ExceptionObject as Exception);
        }
    }
}
