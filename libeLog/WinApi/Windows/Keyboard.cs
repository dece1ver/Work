using libeLog.WinApi.pInvoke;
using System;
using System.Diagnostics;

namespace libeLog.WinApi.Windows;

public class Keyboard
{

    private const int KeyEventExtendedKey = 1;
    private const int KeyEventKeyUp = 2;
    private const int ScClose = 0xF060;
    public const uint KlfActivate = 0x00000001;
    public const uint KlfSetForProcess = 0x00000100;

    public static void KeyDown(Keys vKey)
    {
        User32.keybd_event((byte)vKey, 0, KeyEventExtendedKey, 0);
    }

    public static void KeyUp(Keys vKey)
    {
        User32.keybd_event((byte)vKey, 0, KeyEventExtendedKey | KeyEventKeyUp, 0);
    }

    public static void KeyPress(Keys vKey)
    {
        KeyDown(vKey);
        KeyUp(vKey);
    }



    public static void KillTabTip(int killType)
    {
        switch (killType)
        {
            case 0:
                {
                    var tabTip = User32.FindWindow("IPTIP_Main_Window", "");
                    if (tabTip != IntPtr.Zero)
                        _ = User32.SendMessage(tabTip, WM.SYSCOMMAND, (IntPtr)ScClose, (IntPtr)0);

                    break;
                }
            case 1:
                {
                    foreach (var process in Process.GetProcessesByName("tabtip"))
                    {
                        process.Kill();
                    }

                    break;
                }
        }
    }

    public static void RunTabTip()
    {
        if (Process.GetProcessesByName("tabtip").Length == 0)
            Process.Start(new ProcessStartInfo("tabtip") { UseShellExecute = true });
        var trayWnd = User32.FindWindow("Shell_TrayWnd", null);
        IntPtr nullIntPtr = new(0);

        if (trayWnd == nullIntPtr) return;
        var trayNotifyWnd = User32.FindWindowEx(trayWnd, nullIntPtr, "TrayNotifyWnd", null);
        if (trayNotifyWnd == nullIntPtr) return;
        var tIpBandWnd = User32.FindWindowEx(trayNotifyWnd, nullIntPtr, "TIPBand", null);
        if (tIpBandWnd == nullIntPtr) return;
        User32.PostMessage(tIpBandWnd, WM.LBUTTONDOWN, (IntPtr)1, (IntPtr)65537);
        User32.PostMessage(tIpBandWnd, WM.LBUTTONUP, (IntPtr)1, (IntPtr)65537);
    }
}
