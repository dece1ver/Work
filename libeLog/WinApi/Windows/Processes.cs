using libeLog.WinApi.pInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.WinApi.Windows
{
    public static class Processes
    {
        /// <summary>
        /// Пытается корректно закрыть процесс, отправляя сообщение WM_CLOSE его главному окну
        /// </summary>
        /// <param name="process">Процесс для закрытия</param>
        /// <returns>True, если процесс был успешно закрыт, иначе False</returns>
        public static bool CloseProcessGracefully(Process process)
        {
            try
            {
                if (process != null && !process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                {
                    // Отправляем сообщение WM_CLOSE главному окну процесса
                    User32.PostMessage(process.MainWindowHandle, WM.CLOSE, IntPtr.Zero, IntPtr.Zero);

                    // Даем время на обработку закрытия (включая диалоги)
                    return process.WaitForExit(10000); // 10 секунд
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
