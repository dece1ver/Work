using eLog.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
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
                _EventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }
            SingleInstanceWatcher();
        }

        private const string UniqueEventName = "{0E54B49C-DADB-4A87-8DA3-47133B69D56E}";
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