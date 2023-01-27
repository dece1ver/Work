using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eLog.Infrastructure.Extensions.pInvoke;

namespace eLog.Infrastructure.Extensions.Windows
{
    public class Keyboard
    {

        private const int KeyeventfExtendedkey = 1;
        private const int KeyeventfKeyup = 2;

        public static ushort GetKeyboardLayout()
        {
            return User32.GetKeyboardLayout(User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), IntPtr.Zero));
        }

        public static void KeyDown(Keys vKey)
        {
           User32.keybd_event((byte)vKey, 0, KeyeventfExtendedkey, 0);
        }

        public static void KeyUp(Keys vKey)
        {
            User32.keybd_event((byte)vKey, 0, KeyeventfExtendedkey | KeyeventfKeyup, 0);
        }

        public static void KeyPress(Keys vKey)
        {
            KeyDown(vKey);
            KeyUp(vKey);
        }

    }
}
