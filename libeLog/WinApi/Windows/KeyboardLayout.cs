using System;
using System.Globalization;
using System.Text;
using libeLog.WinApi.pInvoke;
using static System.Threading.Thread;

namespace libeLog.WinApi.Windows;

public sealed class KeyboardLayout
{
    public const int Ru = 1049;
    public const int En = 1033;

    public static int Current => User32.GetKeyboardLayout(User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), IntPtr.Zero));

    private readonly uint hkl;

    private KeyboardLayout(CultureInfo cultureInfo) => hkl = User32.LoadKeyboardLayout(new StringBuilder(cultureInfo.LCID.ToString("x8")), Keyboard.KlfActivate);

    private KeyboardLayout(uint hkl) => this.hkl = hkl;

    public static KeyboardLayout GetCurrent() => new(User32.GetKeyboardLayout(CurrentThread.ManagedThreadId));

    public static KeyboardLayout Load(CultureInfo culture) => new(culture);

    public void Activate() => User32.ActivateKeyboardLayout(hkl, Keyboard.KlfSetForProcess);
}
