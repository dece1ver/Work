using System;
using System.Globalization;

namespace libeLog.WinApi.Windows;

internal class KeyboardLayoutScope : IDisposable, IKeyboardLayoutScope
{
    private readonly KeyboardLayout currentLayout;

    public KeyboardLayoutScope() { }

    public KeyboardLayoutScope(CultureInfo culture)
    {
        currentLayout = KeyboardLayout.GetCurrent();
        KeyboardLayout.Load(culture).Activate();
    }

    public KeyboardLayoutScope(KeyboardLayout currentLayout) => this.currentLayout = currentLayout;

    public void Dispose() => currentLayout.Activate();
}
internal interface IKeyboardLayoutScope
{
    void Dispose();
}
