using System;
using System.IO;

namespace Vovannum
{
    public static class Utils
    {
        public static void LogError(Exception ex)
        {
            ex.Log();
        }
    }
}
