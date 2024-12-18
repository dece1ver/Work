﻿using remeLog.Infrastructure;
using remeLog.Views;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace remeLog
{
    public partial class App : Application
    {
        private const string UniqueEventName = "{55AE85A3-A40C-48E0-8E83-FD55D28FBCF5}";
        private readonly EventWaitHandle _EventWaitHandle;

        public App()
        {
           
            try
            {
                _EventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                _EventWaitHandle.Set();

                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                AppSettings.Instance.ReadConfig();

                Util.TrySetupSyncfusionLicense();

                _EventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            SingleInstanceWatcher();
            ExtractAndLoadDll("fwlib0DN64.dll");
            ExtractAndLoadDll("Fwlib64.dll");
            ExtractAndLoadDll("fwlibe64.dll");
            ExtractAndLoadDll("fwlibNCG64.dll");
        }

        private void ExtractAndLoadDll(string dllName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"remeLog.{dllName}";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new DllNotFoundException($"Embedded resource '{resourceName}' not found.");

                var dllPath = Path.Combine(Path.GetTempPath(), dllName);
                using (var fileStream = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
                var a = File.Exists(dllPath);
                var handle = LoadLibrary(dllPath);
                if (handle == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new DllNotFoundException($"Unable to load DLL '{dllPath}'. Error code: {errorCode}");
                }
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private void SingleInstanceWatcher()
        {
            new Task(() =>
            {
                while (_EventWaitHandle.WaitOne())
                {
                    _ = Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (Current.MainWindow!.Equals(null)) return;
                        var mw = Current.MainWindow;

                        if (mw.WindowState == WindowState.Minimized || mw.Visibility != Visibility.Visible)
                        {
                            mw.Show();
                            mw.WindowState = WindowState.Maximized;
                        }

                        mw.Activate();
                        mw.Topmost = true;
                        mw.Topmost = false;
                        mw.Focus();
                    });
                }
            })
                .Start();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            CultureInfo culture = new CultureInfo("ru-RU");
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NaNSymbol = "-";
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
