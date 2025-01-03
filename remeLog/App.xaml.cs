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

            Util.WriteLogAsync("Создан экземпляр приложения. Уникальное имя события: " + _uniqueEventName);

            try
            {
                Util.WriteLogAsync("Начало обработки запуска приложения");
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
            Util.WriteLogAsync("Проверка на наличие уже запущенных экземпляров");
            bool isAlreadyRunning = TryConnectToExistingInstance();
            if (isAlreadyRunning)
            {
                Util.WriteLogAsync("Обнаружен запущенный экземпляр приложения. Активируем окно и завершаем текущий процесс.");
                _eventWaitHandle?.Set();
                Shutdown();
                return;
            }

            Util.WriteLogAsync("Проверка обновлений приложения");
            if (CheckForUpdate())
            {
                Util.WriteLogAsync("Обновление найдено и успешно запущено. Завершаем текущий процесс.");
                Shutdown();
                return;
            }

            Util.WriteLogAsync("Начало инициализации приложения");
            InitializeApplication();
        }

        private bool TryConnectToExistingInstance()
        {
            try
            {
                Util.WriteLogAsync($"Попытка открыть событие с именем: {_uniqueEventName}");
                _eventWaitHandle = EventWaitHandle.OpenExisting(_uniqueEventName);
                Util.WriteLogAsync("Событие найдено. Другой экземпляр уже запущен.");
                return true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Util.WriteLogAsync("Событие не найдено. Создаем новое.");
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
                Util.WriteLogAsync("Получение текущего пути исполняемого файла");
                var currentFile = Process.GetCurrentProcess().MainModule?.FileName;
                if (currentFile == null)
                {
                    Util.WriteLogAsync("Не удалось получить путь текущего файла. Обновление не требуется.");
                    return false;
                }

                var currentExeName = Path.GetFileName(currentFile);
                var updatePath = Path.Combine("./update", currentExeName);
                Util.WriteLogAsync($"Проверка файла обновления по пути: {updatePath}");
                if (File.Exists(updatePath) && updatePath.IsFileNewerThan(currentFile))
                {
                    Util.WriteLogAsync("Обновленный файл найден. Запуск обновления...");
                    return TryStartUpdate(updatePath);
                }

                Util.WriteLogAsync("Обновление не требуется.");
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
            Util.WriteLogAsync("Инициализация настроек приложения");
            AppSettings.Instance.ReadConfig();

            Util.WriteLogAsync("Настройка лицензии Syncfusion");
            Util.TrySetupSyncfusionLicense();

            Util.WriteLogAsync("Запуск наблюдателя активации окна");
            StartWindowActivationWatcher();

            Util.WriteLogAsync("Загрузка необходимых DLL");
            LoadRequiredDlls();

            Util.WriteLogAsync("Настройка культуры приложения");
            ConfigureCulture();

            Util.WriteLogAsync("Инициализация завершена");
        }

        private void StartWindowActivationWatcher()
        {
            Util.WriteLogAsync("Запуск наблюдателя активации окна");

            Task.Factory.StartNew(() =>
            {
                Util.WriteLogAsync("Наблюдатель активации запущен");
                
                while (_eventWaitHandle?.WaitOne() ?? false)
                {
                    Util.WriteLogAsync("Получен сигнал активации");

                    try
                    {
                        if (!Dispatcher.CheckAccess())
                        {
                            throw new InvalidOperationException("Попытка использовать Dispatcher из другого потока.");
                        }
                        Dispatcher.BeginInvoke(() =>
                        {
                            Util.WriteLogAsync("Вызов метода ActivateMainWindow через Dispatcher");
                            ActivateMainWindow();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.WriteLogAsync(ex, "Ошибка при активации окна");
                    }
                }

                Util.WriteLogAsync("Наблюдатель активации завершил работу");
            }, TaskCreationOptions.LongRunning);
        }

        private static void ActivateMainWindow()
        {
            Util.WriteLogAsync("Попытка активации главного окна");

            var mainWindow = Current.MainWindow;
            if (mainWindow == null)
            {
                Util.WriteLogAsync("Главное окно не найдено. Завершаем активацию.");
                return;
            }

            if (mainWindow.WindowState == WindowState.Minimized ||
                mainWindow.Visibility != Visibility.Visible)
            {
                Util.WriteLogAsync("Окно свернуто или скрыто. Приводим его в нормальное состояние.");
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Maximized;
            }

            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();

            Util.WriteLogAsync("Главное окно активировано.");
        }

        private static void LoadRequiredDlls()
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

        private static void ExtractAndLoadDll(string dllName)
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

        public static string CreateUniqueEventName()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
            string timestamp;
            try
            {
                timestamp = File.GetLastWriteTime(Process.GetCurrentProcess().MainModule?.FileName!).ToString("yyMMddHHmm");
            }
            catch (Exception ex)
            {
                Util.WriteLogAsync(ex, "Исключение при получении даты изменения");
                timestamp = "";
            }
            return $"remeLog-{version.Major}.{version.Minor}.{version.Build}.{timestamp}";
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureCulture();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _eventWaitHandle?.Close();
            base.OnExit(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            string message = exception != null
                ? $"Необработанное исключение: {exception.Message}\n{exception.StackTrace}"
                : $"Необработанное исключение. Тип: {e.ExceptionObject?.GetType()}";

            Util.WriteLogAsync(message);

            MessageBox.Show("Произошла критическая ошибка. Приложение будет закрыто.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            Shutdown(1);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = $"Необработанное исключение в UI потоке: {e.Exception.Message}\n{e.Exception.StackTrace}";
            Util.WriteLogAsync(message);

            MessageBox.Show("Произошла ошибка в пользовательском интерфейсе. Приложение будет закрыто.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true; 
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            string message = $"Необработанное исключение в задаче: {e.Exception?.Message}\n{e.Exception?.StackTrace}";
            Util.WriteLogAsync(message);

            e.SetObserved();
        }
    }
}