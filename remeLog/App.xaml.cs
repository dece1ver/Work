using libeLog.Extensions;
using remeLog.Infrastructure;
using remeLog.Views;
using System;
using System.Diagnostics;
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
        private readonly string _uniqueEventName;
        private EventWaitHandle? _eventWaitHandle;

        public App()
        {
            _uniqueEventName = CreateUniqueEventName();

            try
            {
                HandleApplicationStart();
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, "Критическая ошибка при запуске приложения");
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void HandleApplicationStart()
        {
            bool isAlreadyRunning = TryConnectToExistingInstance();
            if (isAlreadyRunning)
            {
                Util.WriteLogAsync("Приложение уже запущено, активируем существующее окно");
                _eventWaitHandle?.Set();
                Shutdown();
                return;
            }

            if (CheckForUpdate())
            {
                Util.WriteLogAsync("Запуск обновленной версии");
                Shutdown();
                return;
            }

            InitializeApplication();
        }

        private bool TryConnectToExistingInstance()
        {
            try
            {
                _eventWaitHandle = EventWaitHandle.OpenExisting(_uniqueEventName);
                return true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, _uniqueEventName);
                return false;
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, "Ошибка при проверке запущенных экземпляров");
                throw;
            }
        }

        private bool CheckForUpdate()
        {
            try
            {
                var currentFile = Process.GetCurrentProcess().MainModule?.FileName;
                if (currentFile == null) return false;

                var currentExeName = Path.GetFileName(currentFile);
                var updatePath = Path.Combine("./update", currentExeName);
                if (File.Exists(updatePath) && updatePath.IsFileNewerThan(currentFile))
                {
                    return TryStartUpdate(updatePath);
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, "Ошибка при проверке обновлений");
                return false;
            }
        }

        private bool TryStartUpdate(string updatePath)
        {
            try
            {
                Util.WriteLogAsync($"Запуск обновленной версии из {updatePath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = updatePath,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    Util.WriteLogAsync("Не удалось запустить процесс обновления");
                    return false;
                }

                // Ждем чтобы убедиться что процесс запустился
                if (!process.WaitForInputIdle(5000))
                {
                    Util.WriteLogAsync("Процесс обновления не отвечает");
                    return false;
                }

                Util.WriteLogAsync("Обновленная версия успешно запущена, завершаем текущий процесс");
                return true;
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, "Ошибка при запуске обновления");
                return false;
            }
        }

        private void InitializeApplication()
        {
            Util.WriteLogAsync("Инициализация приложения");

            AppSettings.Instance.ReadConfig();
            Util.TrySetupSyncfusionLicense();
            StartWindowActivationWatcher();
            LoadRequiredDlls();
            ConfigureCulture();
        }

        private void StartWindowActivationWatcher()
        {
            Task.Factory.StartNew(() =>
            {
                while (_eventWaitHandle?.WaitOne() ?? false)
                {
                    try
                    {
                        Dispatcher.BeginInvoke(ActivateMainWindow);
                    }
                    catch (Exception ex)
                    {
                        Util.WriteLogAsync(ex, "Ошибка при активации окна");
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void ActivateMainWindow()
        {
            var mainWindow = Current.MainWindow;
            if (mainWindow == null) return;

            if (mainWindow.WindowState == WindowState.Minimized ||
                mainWindow.Visibility != Visibility.Visible)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Maximized;
            }

            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();
        }

        private void LoadRequiredDlls()
        {
            var requiredDlls = new[]
            {
                "fwlib0DN64.dll",
                "Fwlib64.dll",
                "fwlibe64.dll",
                "fwlibNCG64.dll"
            };

            foreach (var dllName in requiredDlls)
            {
                ExtractAndLoadDll(dllName);
            }
        }

        private void ExtractAndLoadDll(string dllName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"remeLog.{dllName}";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new DllNotFoundException($"Ресурс '{resourceName}' не найден.");
                    }

                    var dllPath = Path.Combine(Path.GetTempPath(), dllName);
                    using (var fileStream = new FileStream(dllPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }

                    IntPtr handle = LoadLibrary(dllPath);
                    if (handle == IntPtr.Zero)  // Правильная проверка IntPtr
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new DllNotFoundException(
                            $"Не удалось загрузить DLL '{dllPath}'. Код ошибки: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, $"Ошибка при загрузке DLL {dllName}");
                throw;
            }
        }

        private void ConfigureCulture()
        {
            var culture = new CultureInfo("ru-RU");
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NaNSymbol = "-";

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        private static string CreateUniqueEventName()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            return $"remeLog-{version.Major}.{version.Minor}.{version.Build}.{timestamp}";
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureCulture();
        }
    }
}