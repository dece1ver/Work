using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    public static class Util
    {
        public static void WriteLog(string message) 
            => Logs.Write(AppSettings.LogFile, message, AppSettings.Instance.LogsCopyDir);

        public static void WriteLog(Exception exception, string additionalMessage = "") 
            => Logs.Write(AppSettings.LogFile, exception, additionalMessage, AppSettings.Instance.LogsCopyDir);
    }
}
