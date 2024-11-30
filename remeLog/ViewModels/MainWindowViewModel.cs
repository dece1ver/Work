using libeLog;
using libeLog.Base;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using remeLog.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static libeLog.Constants;
using static remeLog.Models.CombinedParts;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace remeLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
        private readonly object lockObject = new();
        private bool lockUpdate = false;
        private CancellationTokenSource _cancellationTokenSource = new();
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
            LoadPartsInfoCommand = new LambdaCommand(OnLoadPartsInfoCommandExecuted, CanLoadPartsInfoCommandExecute);
            ShowMonitorCommand = new LambdaCommand(OnShowMonitorCommandExecuted, CanShowMonitorCommandExecute);
            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
            ShowPartsInfoCommand = new LambdaCommand(OnShowPartsInfoCommandExecuted, CanShowPartsInfoCommandExecute);
            IncreaseDateCommand = new LambdaCommand(OnIncreaseDateCommandExecuted, CanIncreaseDateCommandExecute);
            DecreaseDateCommand = new LambdaCommand(OnDecreaseDateCommandExecuted, CanDecreaseDateCommandExecute);
            SetYesterdayDateCommand = new LambdaCommand(OnSetYesterdayDateCommandExecuted, CanSetYesterdayDateCommandExecute);
            SetWeekDateCommand = new LambdaCommand(OnSetWeekDateCommandExecuted, CanSetWeekDateCommandExecute);
            _Machines = new();
            if (AppSettings.Instance.InstantUpdateOnMainWindow) { _ = LoadPartsAsync(true); }
            // var backgroundWorker = new Thread(BackgroundWorker) { IsBackground = true };
            // backgroundWorker.Start();
        }


        private Overlay _Overlay = new(false);

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

        private string _Status = string.Empty;
        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private double _Progress = 1;
        /// <summary> Значение прогрессбара </summary>
        public double Progress
        {
            get => _Progress;
            set
            {
                Set(ref _Progress, value);
                OnPropertyChanged(nameof(ProgressBarVisibility));
            }
        }

        private double _ProgressMaxValue = 1;
        /// <summary> Максимальное значение прогрессбара </summary>
        public double ProgressMaxValue
        {
            get => _ProgressMaxValue;
            set
            {
                Set(ref _ProgressMaxValue, value);
                OnPropertyChanged(nameof(ProgressBarVisibility));
            }
        }

        public Visibility ProgressBarVisibility
        {
            get => _ProgressBarVisibility;
            set => Set(ref _ProgressBarVisibility, value);
        }

        private Visibility _ProgressBarVisibility = Visibility.Hidden;



        private DateTime _FromDate = DateTime.Today.AddDays(-1);
        public DateTime FromDate
        {
            get => _FromDate;
            set
            {
                if (Set(ref _FromDate, value) && AppSettings.Instance.InstantUpdateOnMainWindow)
                {
                    OnPropertyChanged(nameof(IsSingleShift));
                    _ = LoadPartsAsync();
                }
            }
        }

        private DateTime _ToDate = DateTime.Today.AddDays(-1);
        public DateTime ToDate
        {
            get => _ToDate;
            set
            {
                if (Set(ref _ToDate, value) && AppSettings.Instance.InstantUpdateOnMainWindow)
                {
                    OnPropertyChanged(nameof(IsSingleShift));
                    _ = LoadPartsAsync();
                }
            }
        }

        private ObservableCollection<CombinedParts> _Parts = new();
        /// <summary> Объединенный список объединенных списков </summary>
        public ObservableCollection<CombinedParts> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }


        private List<string> _Machines;
        /// <summary> Описание </summary>
        public List<string> Machines
        {
            get => _Machines;
            set => Set(ref _Machines, value);
        }


        private bool _Debug = false;
        /// <summary> отладка </summary>
        public bool Debug
        {
            get => _Debug;
            set => Set(ref _Debug, value);
        }


        private bool IsSingleShift => FromDate == ToDate;


        #region Команды

        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }
        private static void OnCloseApplicationCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }
        private static bool CanCloseApplicationCommandExecute(object p) => true;
        #endregion

        #region EditSettings
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                SettingsWindow settingsWindow = new SettingsWindow() { Owner = Application.Current.MainWindow };
                if (settingsWindow.ShowDialog() == true && settingsWindow.DataContext is SettingsWindowViewModel settings)
                {
                    AppSettings.Instance.DataSource = settings.DataSource;
                    AppSettings.Instance.QualificationSourcePath = settings.QualificationSourcePath.Value;
                    AppSettings.Instance.ReportsPath = settings.ReportsPath.Value;
                    AppSettings.Instance.DailyReportsDir = settings.DailyReportsDir.Value;
                    AppSettings.Instance.ConnectionString = settings.ConnectionString.Value;
                    AppSettings.Instance.InstantUpdateOnMainWindow = settings.InstantUpdateOnMainWindow;
                    AppSettings.Instance.User = settings.Role;
                    AppSettings.Save();
                    Status = "Параметры сохранены";
                }
            }
        }
        private static bool CanEditSettingsCommandExecute(object p) => true;
        #endregion

        #region EditOperators
        public ICommand EditOperatorsCommand { get; }
        private void OnEditOperatorsCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                EditOperatorsWindow editOperatorsWindow = new EditOperatorsWindow() { Owner = Application.Current.MainWindow };
                editOperatorsWindow.ShowDialog();
            }
        }
        private static bool CanEditOperatorsCommandExecute(object p) => true;
        #endregion

        #region LoadPartsInfo
        public ICommand LoadPartsInfoCommand { get; }
        private async void OnLoadPartsInfoCommandExecuted(object p)
        {
            if (p is true)
            {
                Parts.Clear(); 
                var cp = await GenerateMockDataAsync("Hyundai WIA SKT21 №104", DateTime.Today, DateTime.Today);
                var partsInfoWindow = new PartsInfoWindow(cp) 
                { 
                    Owner = Application.Current.MainWindow, DataContext = new PartsInfoWindowViewModel(cp) 
                    {
                        UseMockData = true
                    }
                };
                partsInfoWindow.ShowDialog();
                return;
            }
            await LoadPartsAsync(true);
        }
        private static bool CanLoadPartsInfoCommandExecute(object p) => true;
        #endregion

        #region ShowMonitor
        public ICommand ShowMonitorCommand { get; }
        private void OnShowMonitorCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                FanucMonitor fanucMonitor = new FanucMonitor() { Owner = App.Current.MainWindow };
                fanucMonitor.ShowDialog();
            }
        }
        private static bool CanShowMonitorCommandExecute(object p) => true;
        #endregion

        #region ShowAbout
        public ICommand ShowAboutCommand { get; }
        private void OnShowAboutCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                MessageBox.Show("О программе.");
            }
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region ShowPartsInfo
        public ICommand ShowPartsInfoCommand { get; }
        private void OnShowPartsInfoCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var partsInfo = (CombinedParts)p;
                partsInfo.FromDate = FromDate;
                partsInfo.ToDate = ToDate;
                var partsInfoWindow = new PartsInfoWindow(partsInfo) { Owner = Application.Current.MainWindow };
                partsInfoWindow.Closed += (_, _) => _ = LoadPartsAsync();
                partsInfoWindow.Show();
            }
        }

        private static bool CanShowPartsInfoCommandExecute(object p) => true;
        #endregion

        #region IncreaseDateCommand
        public ICommand IncreaseDateCommand { get; }
        private void OnIncreaseDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = FromDate.AddDays(1);
            ToDate = ToDate.AddDays(1);
            UnlockUpdate();
        }
        private bool CanIncreaseDateCommandExecute(object p) => true;
        #endregion

        #region DecreaseDateCommand
        public ICommand DecreaseDateCommand { get; }
        private void OnDecreaseDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = FromDate.AddDays(-1);
            ToDate = ToDate.AddDays(-1);
            UnlockUpdate();
        }
        private bool CanDecreaseDateCommandExecute(object p) => true;
        #endregion

        #region SetYesterdayDateCommand
        public ICommand SetYesterdayDateCommand { get; }
        private void OnSetYesterdayDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = DateTime.Today.AddDays(-1);
            ToDate = FromDate;
            UnlockUpdate();
        }
        private bool CanSetYesterdayDateCommandExecute(object p) => true;
        #endregion

        #region SetWeekDateCommand
        public ICommand SetWeekDateCommand { get; }
        private void OnSetWeekDateCommandExecuted(object p)
        {
            FromDate = ToDate.AddDays(-7);
        }
        private bool CanSetWeekDateCommandExecute(object p) => true;
        #endregion

        #endregion

        private async Task LoadPartsAsync(bool first = false)
        {
            if (lockUpdate) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
            var cancellationToken = _cancellationTokenSource.Token;

            await semaphoreSlim.WaitAsync(cancellationToken);
            ProgressBarVisibility = Visibility.Visible;
            Status = "Получение списка станков...";
            if (!first)
            {
                try
                {
                    await Task.Delay(700, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    semaphoreSlim?.Release();
                    return;
                }
            }
            switch (Machines.ReadMachines())
            {
                case DbResult.AuthError:
                    MessageBox.Show("Не удалось получить список станков из-за неудачной авторизации в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.Error:
                    MessageBox.Show("Не удалось получить список станков из-за ошибки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.NoConnection:
                    MessageBox.Show("Нет соединения с базой данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
            Status = "Получение списка причин неотмеченных простоев...";
            switch (AppSettings.Instance.UnspecifiedDowntimesReasons.ReadDowntimeReasons())
            {
                case DbResult.AuthError:
                    MessageBox.Show("Не удалось получить список причин простоев из-за неудачной авторизации в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.Error:
                    MessageBox.Show("Не удалось получить список причин простоев из-за ошибки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.NoConnection:
                    MessageBox.Show("Нет соединения с базой данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            Status = "Получение списка причин отклонений в наладке...";
            switch (AppSettings.Instance.SetupReasons.ReadDeviationReasons(DeviationReasonType.Setup))
            {
                case DbResult.AuthError:
                    MessageBox.Show("Не удалось получить список причин отклонений для наладок из-за неудачной авторизации в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.Error:
                    MessageBox.Show("Не удалось получить список причин отклонений для наладок из-за ошибки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.NoConnection:
                    MessageBox.Show("Нет соединения с базой данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            Status = "Получение списка причин отклонений в изготовлении...";
            switch (AppSettings.Instance.MachiningReasons.ReadDeviationReasons(DeviationReasonType.Machining))
            {
                case DbResult.AuthError:
                    MessageBox.Show("Не удалось получить список причин отклонений для изготовления из-за неудачной авторизации в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.Error:
                    MessageBox.Show("Не удалось получить список причин отклонений для изготовления из-за ошибки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case DbResult.NoConnection:
                    MessageBox.Show("Нет соединения с базой данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Status = "Очистка списка деталей...";
                Parts.Clear();
                foreach (var machine in Machines)
                {
                    ReportState state = ReportState.NotExist;
                    bool dayExist = false;
                    bool nightExist = false;
                    bool dayChecked = false;
                    bool nightChecked = false;
                    Status = $"Получение информации за сутки на стнке {machine}...";
                    if (FromDate == ToDate)
                    {
                        if (Database.ReadShiftInfo(new ShiftInfo(ToDate, ShiftType.Day, machine), out var dbDayShifts) is DbResult.Ok && dbDayShifts.Count > 0 && dbDayShifts[0].Master != "")
                        {
                            dayExist = true;
                            if (dbDayShifts.Any(s => s.IsChecked)) dayChecked = true;
                        }

                        if (Database.ReadShiftInfo(new ShiftInfo(ToDate, ShiftType.Night, machine), out var dbNightShifts) is DbResult.Ok && dbNightShifts.Count > 0 && dbNightShifts[0].Master != "")
                        {
                            nightExist = true;
                            if (dbNightShifts.Any(s => s.IsChecked)) nightChecked = true;
                        }

                        if (dayExist && nightExist)
                        {
                            state = ReportState.Exist;
                        }
                        else if (dayExist || nightExist)
                        {
                            state = ReportState.Partial;
                        }
                    }
                    Parts.Add(new CombinedParts(machine, FromDate, ToDate) { IsReportExist = state, IsReportChecked = dayChecked && nightChecked });
                }
                return true;
            });
            Status = "Загрузка информации...";
            foreach (var part in Parts)
            {
                try
                {
                    part.Parts = await Database.ReadPartsByShiftDateAndMachine(FromDate, ToDate, part.Machine, cancellationToken);
                }
                catch (OperationCanceledException)
                {

                }
                catch (SqlException sqlEx)
                {
                    var message = sqlEx.Number switch
                    {
                        SqlErrorCode.NoConnection => StatusTips.NoConnectionToDb,
                        SqlErrorCode.AuthError => StatusTips.AuthFailedToDb,
                        _ => $"Ошибка БД №{sqlEx.Number}\n{sqlEx.Message}",
                    };
                    MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            Status = "";
            ProgressBarVisibility = Visibility.Collapsed;
            semaphoreSlim.Release();
        }

        private void LockUpdate() => lockUpdate = true;

        private void UnlockUpdate()
        {
            lockUpdate = false;
            _ = LoadPartsAsync();
        }

        public static async Task<CombinedParts> GenerateMockDataAsync(string machine, DateTime fromDate, DateTime toDate)
        {
            Random random = new();
            var combinedParts = new CombinedParts(machine, fromDate, toDate)
            {
                IsReportExist = (CombinedParts.ReportState)random.Next(0, 3),
                IsReportChecked = random.NextDouble() > 0.5,
                Parts = await Util.GenerateMockPartsAsync()
            };
            return combinedParts;
        }

        private void BackgroundWorker()
        {
            while (true)
            {

            }
        }
    }
}