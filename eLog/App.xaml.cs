using eLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace eLog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SingleInstanceWatcher();
            AppSettings.ReadConfig();
        }

        private const string UniqueEventName = "{0E54B49C-DADB-4A87-8DA3-47133B69D56E}";
        private EventWaitHandle _EventWaitHandle;

        private void SingleInstanceWatcher()
        {
            // check if it is already open.
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
                _EventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            // if this instance gets the signal to show the main window
            new Task(() =>
                {
                    while (_EventWaitHandle.WaitOne())
                    {
                        _ = Current.Dispatcher.BeginInvoke((Action)(() =>
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
                        }));
                    }
                })
                .Start();
        }
    }
}
