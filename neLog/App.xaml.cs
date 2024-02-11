using neLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace neLog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

            try
            {
                // try to open it - if another instance is running, it will exist , if not it will throw
                _EventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);

                // Notify other instance so it could bring itself to foreground.
                _EventWaitHandle.Set();

                // Terminate this instance.
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                AppSettings.Instance.ReadConfig();
                try
                {
                    if (!Directory.Exists(AppSettings.BasePath)) Directory.CreateDirectory(AppSettings.BasePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка при создании базовой директории", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                _EventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }
            SingleInstanceWatcher();
        }

        private const string UniqueEventName = "{152DBA89-76CB-4AC4-901C-DCF631A4A9E8}";
        private readonly EventWaitHandle _EventWaitHandle;

        private void SingleInstanceWatcher()
        {
            // if this instance gets the signal to show the main window
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
    }
}
