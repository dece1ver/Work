﻿using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Models;
using eLog.Services;
using eLog.Views.Windows.Dialogs;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Infrastructure;
using libeLog.Interfaces;
using libeLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static eLog.Infrastructure.Extensions.Util;
using Application = System.Windows.Application;
using Database = eLog.Infrastructure.Extensions.Database;
using Machine = eLog.Models.Machine;
using MessageBox = System.Windows.MessageBox;
using Text = eLog.Infrastructure.Extensions.Text;

namespace eLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
        bool _editPart = false;
        bool _needSave = false;
        SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cts = new();
        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);

            StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
            EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);

            EndSetupCommand = new LambdaCommand(OnEndSetupCommandExecuted, CanEndSetupCommandExecute);

            SetDownTimeCommand = new LambdaCommand(OnSetDownTimeCommandExecuted, CanSetDownTimeCommandExecute);
            EndDownTimeCommand = new LambdaCommand(OnEndDownTimeCommandExecuted, CanEndDownTimeCommandExecute);

            StartHelpCaseCommand = new LambdaCommand(OnStartHelpCaseCommandExecuted, CanStartHelpCaseCommandExecute);
            EndHelpCaseCommand = new LambdaCommand(OnEndHelpCaseCommandExecuted, CanEndHelpCaseCommandExecute);

            StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
            EditDetailCommand = new LambdaCommand(OnEditDetailCommandExecuted, CanEditDetailCommandExecute);
            EndDetailCommand = new LambdaCommand(OnEndDetailCommandExecuted, CanEndDetailCommandExecute);

            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
            LoadProductionTasksCommand = new LambdaCommand(OnLoadProductionTasksCommandExecuted, CanLoadProductionTasksCommandExecute);
            SendMessageCommand = new LambdaCommand(OnSendMessageCommandExecuted, CanSendMessageCommandExecute);
            ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
            TestCommand = new LambdaCommand(OnTestCommandExecuted, CanTestCommandExecute);

            Parts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Parts));

            var syncPartsThread = new Thread(() => _ = SyncParts()) { IsBackground = true };
            syncPartsThread.Start();

            var notifyThread = new Thread(() => NotifyWatcher()) { IsBackground = true };
            notifyThread.Start();

            var shiftHandoverWatcher = new Thread(() => ShiftHandoverWatcher()) { IsBackground = true };
            shiftHandoverWatcher.Start();

            Task.Run(UpdateToolTypes);

            WriteLog("Старт");
        }

        private async static Task UpdateToolTypes()
        {
            var (result, types, error) = await Database.GetSearchToolTypes();
            switch (result)
            {
                case DbResult.Ok:
                    AppSettings.Instance.SearchToolTypes = types;
                    break;
                case DbResult.AuthError:
                    Util.WriteLog("Не удалось авторизоваться при обновлении списка типов инструмента.");
                    break;
                case DbResult.Error:
                    Util.WriteLog($"Ошибка при обновлении списка типов инструмента:\n{error}");
                    break;
                case DbResult.NoConnection:
                    Util.WriteLog("Не удалось установить соединение с БД при обновлении списка типов инструмента.");
                    break;
                case DbResult.NotFound:
                    Util.WriteLog($"Список типов инструмента пуст.");
                    break;
            }
        }

        #region Свойства-обертки настроек


        /// <summary> Станок </summary>
        public Machine Machine
        {
            get => AppSettings.Instance.Machine;
            set => AppSettings.Instance.Machine = value;
        }

        /// <summary> Текущий оператор </summary>
        public Operator? CurrentOperator
        {
            get => AppSettings.Instance.CurrentOperator;
            set
            {
                AppSettings.Instance.CurrentOperator = value;
                OnPropertyChanged(nameof(CanStartShift));
                OnPropertyChanged(nameof(CanAddPart));
            }
        }

        /// <summary> Список операторов </summary>
        public DeepObservableCollection<Operator> Operators
        {
            get => AppSettings.Instance.Operators;
            set => AppSettings.Instance.Operators = value;
        }

        public static string[] Shifts => Text.Shifts;

        public string CurrentShift
        {
            get => AppSettings.Instance.CurrentShift;
            set => AppSettings.Instance.CurrentShift = value;
        }

        /// <summary> Детали </summary>
        public static DeepObservableCollection<Part> Parts
        {
            get => AppSettings.Instance.Parts;
            set
            {
                AppSettings.Instance.Parts = value;
                AppSettings.Save();
            }
        }

        #endregion

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


        private bool _ProductionTasksIsLoading;
        /// <summary> В процессе ли загрузка заданий на производство </summary>
        public bool ProductionTasksIsLoading
        {
            get => _ProductionTasksIsLoading;
            set => Set(ref _ProductionTasksIsLoading, value);
        }

        public bool WorkIsNotInProgress => Parts.Count == 0 || Parts.Count == Parts.Count(x => x.IsFinished is not Part.State.InProgress);
        public bool CanAddPart => ShiftStarted && WorkIsNotInProgress && CurrentOperator is { };
        public bool CanStartShift => CurrentOperator is { } && !string.IsNullOrEmpty(CurrentShift);
        public bool CanEditShiftAndParams => !ShiftStarted && WorkIsNotInProgress;
        public bool CanEndShift => ShiftStarted && WorkIsNotInProgress;

        private bool _ShiftStarted = AppSettings.Instance.IsShiftStarted;
        private Visibility _ProgressBarVisibility = Visibility.Collapsed;
        private object lockObject = new();
        private bool askedForUpdate;

        public bool ShiftStarted
        {
            get => _ShiftStarted;
            set
            {
                if (!Set(ref _ShiftStarted, value)) return;
                AppSettings.Instance.IsShiftStarted = value;
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
            }
        }

        #region Команды


        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }
        private static void OnCloseApplicationCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }
        private static bool CanCloseApplicationCommandExecute(object p) => true;
        #endregion

        #region StartShift
        public ICommand StartShiftCommand { get; }
        private void OnStartShiftCommandExecuted(object p)
        {
            if (AppSettings.Instance.EnableWriteShiftHandover)
            {
                DateTime dateTime = DateTime.Today;
                if (DateTime.Now.Hour < 8 && AppSettings.Instance.CurrentShift == "Ночь") dateTime.AddDays(-1);
                ShiftHandoverWindow shiftHandoverWindow = new($"Приём смены {dateTime:dd.MM.yy}") { Owner = App.Current.MainWindow };
                if (shiftHandoverWindow.ShowDialog() == false) return;
                Task.Run(async () =>
                {
                    var shiftInfo = new ShiftHandOverInfo(dateTime,
                        AppSettings.Instance.CurrentShift,
                        AppSettings.Instance.Machine.Name,
                        false,
                        shiftHandoverWindow.WorkplaceCleaned,
                        shiftHandoverWindow.Failures,
                        shiftHandoverWindow.ExtraneousNoises,
                        shiftHandoverWindow.ExtraneousNoises,
                        shiftHandoverWindow.ToolBreakage,
                        shiftHandoverWindow.СoolantСoncentration);
                    var dbResult = await Database.WriteShiftHandover(shiftInfo);
                    if (dbResult != DbResult.Ok)
                    {
                        AppSettings.Instance.NotWritedShiftHandovers.Add(shiftInfo);
                    }
                    WriteLog($"Приём смены: {dbResult}");
                });
            }
            if (AppSettings.Instance.DebugMode) WriteLog($"Старт смены.\n\tОператор {AppSettings.Instance.CurrentOperator?.DisplayName}\n\tСмена: {AppSettings.Instance.CurrentShift}.");
            ShiftStarted = true;
            OnPropertyChanged(nameof(CanEndShift));
            OnPropertyChanged(nameof(Parts));
        }

        private static bool CanStartShiftCommandExecute(object p) => true;
        #endregion

        #region EndShift
        public ICommand EndShiftCommand { get; }

        private void OnEndShiftCommandExecuted(object p)
        {
            if (AppSettings.Instance.EnableWriteShiftHandover)
            {
                DateTime dateTime = DateTime.Today;
                if (DateTime.Now.Hour < 8 && AppSettings.Instance.CurrentShift == "Ночь") dateTime.AddDays(-1);
                ShiftHandoverWindow shiftHandoverWindow = new($"Сдача смены {dateTime:dd.MM.yy}") { Owner = App.Current.MainWindow };
                if (shiftHandoverWindow.ShowDialog() == false) return;
                Task.Run(async () =>
                {
                    var shiftInfo = new ShiftHandOverInfo(dateTime,
                        AppSettings.Instance.CurrentShift,
                        AppSettings.Instance.Machine.Name,
                        true,
                        shiftHandoverWindow.WorkplaceCleaned,
                        shiftHandoverWindow.Failures,
                        shiftHandoverWindow.ExtraneousNoises,
                        shiftHandoverWindow.ExtraneousNoises,
                        shiftHandoverWindow.ToolBreakage,
                        shiftHandoverWindow.СoolantСoncentration);
                    var dbResult = await Database.WriteShiftHandover(shiftInfo);
                    if (dbResult != DbResult.Ok)
                    {
                        AppSettings.Instance.NotWritedShiftHandovers.Add(shiftInfo);
                    }
                    WriteLog($"Сдача смены: {dbResult}");
                });
            }
            if (AppSettings.Instance.DebugMode) WriteLog($"Завершение смены.\n\tОператор {AppSettings.Instance.CurrentOperator?.DisplayName}\n\tСмена: {AppSettings.Instance.CurrentShift}.");
            ShiftStarted = false;
            OnPropertyChanged(nameof(CanEndShift));
            OnPropertyChanged(nameof(Parts));
        }
        private static bool CanEndShiftCommandExecute(object p) => true;
        #endregion

        #region EditSettings
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                if (!WindowsUserDialogService.EditSettings()) return;
                OnPropertyChanged(nameof(Machine));
            }
        }
        private static bool CanEditSettingsCommandExecute(object p) => true;
        #endregion

        #region LoadProductionTasks
        public ICommand LoadProductionTasksCommand { get; }
        private async void OnLoadProductionTasksCommandExecuted(object p)
        {
            if (ProductionTasksIsLoading) _cts.Cancel();
            if (string.IsNullOrEmpty(AppSettings.Instance.GsId) || !File.Exists(AppSettings.Instance.GoogleCredentialsPath))
                _cts = new CancellationTokenSource();
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                ProductionTasksIsLoading = true;
                var progress = new Progress<string>(m => Status = m);
                var gs = new GoogleSheet(AppSettings.Instance.GoogleCredentialsPath, AppSettings.Instance.GsId);
                var tasks = await gs.GetProductionTasksData(AppSettings.Instance.Machine.Name, AppSettings.Machines.Select(m => m.Name), progress, _cts.Token);
                ProductionTasksIsLoading = false;
                ProgressBarVisibility = Visibility.Collapsed;
                using (Overlay = new())
                {
                    if (tasks.Count > 0)
                    {
                        TasksForProductionWindow tasksWindow = new(tasks) { Owner = Application.Current.MainWindow };
                        tasksWindow.ShowDialog();
                        if (tasksWindow.NeedStart) StartDetailCommand.Execute(tasksWindow.SelectedTask);
                    }
                    else
                    {
                        MessageBox.Show("В списке нет заданий под данный станок.", "Задания на производство");
                    }
                }

            }
            catch (OperationCanceledException)
            {
                Status = "Загрузка списка отменена.";
            }
            catch
            {
                Status = "Список работы недоступен.";
            }
            finally
            {
                ProductionTasksIsLoading = false;
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }
        private static bool CanLoadProductionTasksCommandExecute(object p) => true;
        #endregion

        #region SendMessage
        public ICommand SendMessageCommand { get; }
        private async void OnSendMessageCommandExecuted(object p)
        {
            try
            {
                if (!Parts.Any() && Parts[0].IsFinished != Part.State.InProgress)
                {
                    MessageBox.Show("Без детали не положено", "Нет", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string message;
                string to;
                using (Overlay = new())
                {
                    var dlg = new UserInputDialogWindow("Что вас беспокоит?", "Кому:", Email.RecieversGroups.Keys.ToList())
                    {
                        Owner = App.Current.MainWindow
                    };

                    if (dlg.ShowDialog() != true) return;

                    message = dlg.UserInput ?? "-";
                    to = dlg.SelectedOption;
                }
                IProgress<string> progress = new Progress<string>(status => Status = status);
                progress.Report("Составление списка получателей...");

                if (!Email.RecieversGroups.TryGetValue(to, out var receiverTypes))
                {
                    progress.Report("Выбранная группа не найдена.");
                    await Task.Delay(3000);
                    return;
                }

                var recipients = await Task.Run(() =>
                    receiverTypes.SelectMany(GetMailReceivers).Distinct().ToList());

                if (!recipients.Any())
                {
                    progress.Report("Получателей не обнаружено.");
                    await Task.Delay(3000);
                    return;
                }

                progress.Report($"Отправка сообщения группе: [{to}]...");
                await Task.Run(() => Email.SendMessage(Parts[0], message, recipients));

                progress.Report("Сообщение отправлено");
            }
            catch (Exception ex)
            {
                Status = $"Ошибка при отправке: {ex.Message}";
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
                await Task.Delay(3000);
                Status = "";
            }
        }

        private static bool CanSendMessageCommandExecute(object p) => true;
        #endregion

        #region EditOperators
        public ICommand EditOperatorsCommand { get; }
        private void OnEditOperatorsCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                if (!WindowsUserDialogService.EditOperators()) return;
                OnPropertyChanged(nameof(CurrentOperator));
                OnPropertyChanged(nameof(Operators));
                OnPropertyChanged(nameof(CanStartShift));
                OnPropertyChanged(nameof(CanAddPart));
            }
        }
        private static bool CanEditOperatorsCommandExecute(object p) => true;
        #endregion

        #region ShowAbout
        public ICommand ShowAboutCommand { get; }
        private void OnShowAboutCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                if (AppSettings.Instance.DebugMode) WriteLog("Вход в справку.");
                var aboutWindow = new AboutWindow() { Owner = Application.Current.MainWindow };
                bool aboutResult = (bool)aboutWindow.ShowDialog()!;
                if (aboutResult)
                {
                    Debug.Print($"aboutResult = {aboutResult}");
                    var serviceResult = aboutWindow.ServiceResult;
                    if (serviceResult.ClearLogs)
                    {
                        try
                        {
                            File.Delete(AppSettings.LogFile);
                            Status = "Логи очищены.";
                            WriteLog("Лог файл очищен.");
                            if (serviceResult.UnSyncAll) Status += " | ";
                        }
                        catch
                        {
                            Status = "Очистка логов не удалась.";
                        }
                    }
                    if (serviceResult.UnSyncAll)
                    {
                        try
                        {
                            foreach (var part in Parts)
                            {
                                part.IsSynced = false;
                            }
                            Status += "Синхронизация сброшена.";
                        }
                        catch
                        {
                            Status = "Не удалось сбросить синхронизацию.";
                        }

                    }

                    if (serviceResult.ResetTasksInfo)
                    {
                        try
                        {
                            foreach (var part in Parts)
                            {
                                part.TaskInfo = Part.PartTaskInfo.NoData;
                                part.IsTaskStatusWritten = false;
                            }
                            Status += "Информация о списках сброшена.";
                        }
                        catch
                        {
                            Status = "Не удалось сбросить синхронизацию.";
                        }
                    }
                }
            }
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region TestCommand
        public ICommand TestCommand { get; }
        private void OnTestCommandExecuted(object p)
        {
            var dlg = new DifficultiesReportWindow() {Owner = App.Current.MainWindow};
            dlg.ShowDialog();
        }
        private static bool CanTestCommandExecute(object p) => true;
        #endregion

        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private void OnStartDetailCommandExecuted(object p)
        {
            ProductionTaskData? productionTaskData = null;
            if (p is ProductionTaskData ptd) productionTaskData = ptd;
            _editPart = true;
            if (AppSettings.Instance.DebugMode) WriteLog($"Старт новой детали.\n\tОператор {AppSettings.Instance.CurrentOperator?.DisplayName}\n\tВсего деталей: {Parts.Count}");
            using (Overlay = new())
            {
                var part = new Part
                {
                    Operator = CurrentOperator!,
                    Shift = AppSettings.Instance.CurrentShift,
                    Setup = 1,
                };
                if (productionTaskData == null)
                {
                    if (!WindowsUserDialogService.EditDetail(ref part, true))
                    {
                        if (AppSettings.Instance.DebugMode) WriteLog($"Отмена старта.\n\tВсего деталей: {Parts.Count}");
                        _editPart = false;
                        return;
                    }

                }
                else
                {
                    part.Order = productionTaskData.Order;
                    if (!WindowsUserDialogService.EditDetail(ref part, true, true))
                    {
                        if (AppSettings.Instance.DebugMode) WriteLog($"Отмена старта.\n\tВсего деталей: {Parts.Count}");
                        _editPart = false;
                        return;
                    }
                }
                if (AppSettings.Instance.DebugMode) WriteLog($"Подтверждение старта.\n\tВсего деталей: {Parts.Count}");
                Status = string.Empty;
                if (AppSettings.Instance.DebugMode) WriteLog($"Добавление в список.\n\tДеталь: {part.Name}");
                Parts.Insert(0, part);
                _ = SetPartialState(ref part);

                if (AppSettings.Instance.DebugMode) WriteLog($"Добавлено.\n\tВсего деталей: {Parts.Count}");
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                OnPropertyChanged(nameof(Parts));
                AppSettings.Save();
            }
            _editPart = false;
        }
        private static bool CanStartDetailCommandExecute(object p) => true;
        #endregion

        #region SetDownTime
        public ICommand SetDownTimeCommand { get; }
        private void OnSetDownTimeCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var (downTimeType, toolType, comment) = WindowsUserDialogService.SetDownTimeType();
                if (downTimeType is { } type)
                {
                    if (type is DownTime.Types.CreateNcProgram && (Part)p is { SetupIsFinished: true })
                    {
                        MessageBox.Show($"{Text.DownTimes.CreateNcProgram} может быть только в наладке.", "Молодой человек, это не для вас простой.", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var part = (Part)p;
                    var index = Parts.IndexOf(part);
                    Parts.RemoveAt(index);
                    var downTimes = part.DownTimes;
                    var downTime = new DownTime(part, type) {ToolType = toolType, Comment = comment };
                    downTimes.Add(downTime);
                    part.DownTimes = downTimes;
                    OnPropertyChanged(nameof(part.DownTimes));
                    OnPropertyChanged(nameof(part.DownTimesIsClosed));
                    Parts.Insert(index, part);
                    OnPropertyChanged(nameof(Parts));
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
                    if (AppSettings.Instance.DebugMode) { WriteLog(part, $"Запущен простой [{downTime.Name}]"); }

                }
            }
        }
        private static bool CanSetDownTimeCommandExecute(object p) => true;
        #endregion

        #region EndDownTime
        public ICommand EndDownTimeCommand { get; }
        private void OnEndDownTimeCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var part = (Part)p;
                var index = Parts.IndexOf((Part)p);

                if (part.LastDownTime is { InProgress: true }
                    && MessageBox.Show($"Завершить простой {part.LastDownTimeName}?",
                        "Подтверждение",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question)
                    == MessageBoxResult.OK)
                {
                    var now = DateTime.Now;
                    var endTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                    if (AppSettings.Instance.DebugMode) { WriteLog(part, $"Завершен простой [{part.LastDownTime.Name}]"); }
                    Parts.RemoveAt(index);
                    if (part.LastDownTime.StartTime == endTime)
                    {
                        part.DownTimes.Remove(part.LastDownTime);
                    }
                    else
                    {

                        part.LastDownTime.EndTimeText = DateTime.Now.ToString(Constants.DateTimeFormat);

                    }
                    OnPropertyChanged(nameof(part.DownTimes));
                    OnPropertyChanged(nameof(part.DownTimesIsClosed));
                    Parts.Insert(index, part);
                    OnPropertyChanged(nameof(Parts));
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();

                }
            }
        }
        private static bool CanEndDownTimeCommandExecute(object p) => true;
        #endregion

        #region EndSetup
        public ICommand EndSetupCommand { get; }
        private void OnEndSetupCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                if (AppSettings.Instance.DebugMode) WriteLog("Вход в справку.");
                var index = Parts.IndexOf((Part)p);
                var part = Parts[index];
                switch (WindowsUserDialogService.GetSetupResult())
                {
                    case EndSetupResult.Success:
                        part.StartMachiningTime = DateTime.Now.Rounded();
                        _ = SetPartialState(ref part);
                        Parts.RemoveAt(index);
                        Parts.Insert(index, part);
                        break;
                    case EndSetupResult.Stop:
                        Parts.Remove((Part)p);
                        break;
                    case EndSetupResult.PartialComplete:
                        var now = DateTime.Now.Rounded();
                        part.StartMachiningTime = now;
                        part.EndMachiningTime = now;
                        part.FinishedCount = 0;
                        _ = SetPartialState(ref part);


                        //if (part.Id != -1)
                        //{
                        //    if (part.RewriteToXl() is Util.WriteResult.Ok)
                        //    {
                        //        part.IsSynced = true;
                        //        Status = $"Информация об изготовлении id{part.Id} обновлена.";
                        //    }
                        //}
                        //else
                        //{
                        //    part.Id = part.WriteToXl();
                        //    if (part.Id > 0)
                        //    {
                        //        part.IsSynced = true;
                        //        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        //    }
                        //}

                        Parts[index] = part;
                        //Parts.RemoveAt(index);
                        //Parts.Insert(index, part);
                        break;
                }
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                AppSettings.Instance.Parts = Parts;
                OnPropertyChanged(nameof(Parts));
                AppSettings.Save();
            }
        }
        private static bool CanEndSetupCommandExecute(object p) => true;
        #endregion

        #region EditDetail
        public ICommand EditDetailCommand { get; }
        private void OnEditDetailCommandExecuted(object p)
        {
            _editPart = true;
            using (Overlay = new())
            {
                var part = new Part((Part)p);
                var index = Parts.IndexOf((Part)p);

                if (WindowsUserDialogService.EditDetail(ref part))
                {
                    OnPropertyChanged(nameof(part.Title));
                    Status = string.Empty;
                    //Parts.RemoveAt(index);
                    //Parts.Insert(index, part);
                    Parts[index] = part;
                    var resPartial = SetPartialState(ref part);
                    Debug.Print($"Set partial for [{part.Name}]: {resPartial}");
                    if (part.StartMachiningTime == DateTime.MinValue) part.DownTimes = new DeepObservableCollection<DownTime>(part.DownTimes.Where(dt => dt.Type != DownTime.Types.PartialSetup));
                    OnPropertyChanged(nameof(Parts));
                    part.IsSynced = false;
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
                }
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                OnPropertyChanged(nameof(Parts));
            }
            _editPart = false;
        }
        private static bool CanEditDetailCommandExecute(object p) => true;
        #endregion

        #region EndDetail
        public ICommand EndDetailCommand { get; }
        private void OnEndDetailCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var part = new Part((Part)p);
                var index = Parts.IndexOf((Part)p);
                if (WindowsUserDialogService.FinishDetail(ref part))
                {
                    Status = string.Empty;
                    Parts.RemoveAt(index);
                    Parts.Insert(index, part);
                    var resPartial = SetPartialState(ref part);
                    Debug.Print($"Set partial for [{part.Name}]: {resPartial}");
                    if (part.StartMachiningTime == DateTime.MinValue) part.DownTimes = new DeepObservableCollection<DownTime>(part.DownTimes.Where(dt => dt.Type != DownTime.Types.PartialSetup));
                    OnPropertyChanged(nameof(Parts));
                    part.IsSynced = false;
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
                };
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                OnPropertyChanged(nameof(Parts));
            }
        }
        private static bool CanEndDetailCommandExecute(object p) => true;
        #endregion

        #region StartHelpCase
        public ICommand StartHelpCaseCommand { get; }
        private void OnStartHelpCaseCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var dlg = new GetPasswordDialogWindow();
            }
        }
        private static bool CanStartHelpCaseCommandExecute(object p) => true;
        #endregion

        #region EndHelpCase
        public ICommand EndHelpCaseCommand { get; }
        private void OnEndHelpCaseCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                
            }
        }
        private static bool CanEndHelpCaseCommandExecute(object p) => true;
        #endregion

        #endregion

        private void NotifyWatcher()
        {
            while (true)
            {
                try
                {
                    // комментарии по инструменту
                    AppSettings.Instance.NotSendedToolComments ??= new();
                    if (AppSettings.Instance.NotSendedToolComments.Any()) 
                    {
                        AppSettings.ToolSearchMailRecievers = GetMailReceivers(ReceiversType.ToolSearch);
                        if (Parts.Any())
                        {
                            var tempList = AppSettings.Instance.NotSendedToolComments.ToList();
                            foreach (var item in tempList)
                            {
                                if (Email.SendToolSearchComment(Parts[0], item))
                                {
                                    AppSettings.Instance.NotSendedToolComments.Remove(item);
                                }
                            }
                        }
                    }

                    if (Parts.Count > 0 
                        && Parts[0].IsFinished == Part.State.InProgress
                        && Parts[0].DownTimes.Any() 
                        && Parts[0].DownTimes.First() is { Type: DownTime.Types.ToolSearching, NeedToSend: true} dts)
                    {
                        if (dts.EndTime < dts.StartTime && DateTime.Now - dts.StartTime > TimeSpan.FromMinutes(5))
                        {
                            AppSettings.Instance.NotSendedToolComments ??= new();
                            AppSettings.Instance.NotSendedToolComments?.Add($"[{dts.ToolType}] {dts.Comment}");
                            dts.NeedToSend = false;
                        }
                    }

                    // Длинные наладки
                    if (Parts.Count > 0
                    && Parts[0].IsFinished == Part.State.InProgress
                    && !Parts[0].LongSetupNotifySended
                    && Parts[0].StartMachiningTime < Parts[0].StartSetupTime
                    && AppSettings.Instance.TimerForNotify > 0)
                    {
                        int limit;
                        if (Parts[0].SetupTimePlan > 0)
                        {
                            var (result, setupCoefficient, error) = AppSettings.Instance.Machine.Name.GetMachineSetupCoefficient();
                            if (result == DbResult.Ok && setupCoefficient.HasValue)
                            {
                                limit = (int)(setupCoefficient.Value * Parts[0].SetupTimePlan);
                            }
                            else
                            {
                                limit = AppSettings.Instance.TimerForNotify * 60;
                                WriteLog(error);
                            }
                        } else
                        {
                            var (result, setupLimit, error) = AppSettings.Instance.Machine.Name.GetMachineSetupLimit();
                            if (result == DbResult.Ok && setupLimit.HasValue)
                            {
                                limit = setupLimit.Value;
                            }
                            else
                            {
                                limit = AppSettings.Instance.TimerForNotify * 60;
                                WriteLog(error);
                            }
                        }
                        var tempDowntimes = new List<DownTime>();
                        foreach (var dt in Parts[0].DownTimes)
                        {
                            var edt = dt.EndTime == DateTime.MinValue ? DateTime.Now : dt.EndTime;
                            tempDowntimes.Add(new DownTime(Parts[0], dt) { EndTimeText = edt.ToString(Constants.DateTimeFormat) });
                        }
                        var totalDowntime = tempDowntimes
                        .Where(dt => dt.Type is not DownTime.Types.PartialSetup)
                        .Aggregate(TimeSpan.Zero, (sum, dt) => sum.Add(dt.Time));

                        var setupTimeWithoutDowntime = DateTime.Now - Parts[0].StartSetupTime - totalDowntime;
                        var breaks = TimeSpan.FromMinutes(DateTimes.GetPartialBreakBetween(Parts[0].StartSetupTime, DateTime.Now));
                        var factSetupTime = setupTimeWithoutDowntime - breaks;
                        if (factSetupTime > TimeSpan.FromMinutes(limit))
                        {
                            AppSettings.LongSetupsMailRecievers = GetMailReceivers(ReceiversType.LongSetup);
                            if (Email.SendLongSetupNotify(Parts[0], limit))
                            {
                                Parts[0].LongSetupNotifySended = true;
                                Parts[0].NeedMasterAttention = true;
                            }
                        }
                    }
                }

                catch (Exception ex) { WriteLog(ex); }
                finally
                {
                    Thread.Sleep(30000);
                    if (!string.IsNullOrWhiteSpace(AppSettings.Instance.UpdatePath) && !askedForUpdate && App.CheckForUpdate(AppSettings.Instance.UpdatePath, false))
                    {
                        if (MessageBox.Show("Доступно обновление, перезапустите программу.\nА в идеале перезагрузить компьютер и подождать 15 минут.\n\n" +
                            "Можно нажать \"Да\", тогда всё закроется и попытается перезагрузиться само.", "Обновление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "shutdown",
                                Arguments = $"/r /t 10",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            });
                            App.Current.Shutdown(0);
                        }
                        askedForUpdate = true;
                    }
                }
            }
        }

        private async void ShiftHandoverWatcher()
        {
            while (true)
            {
                try
                {
                    AppSettings.Instance.NotWritedShiftHandovers ??= new();
                    foreach (var shiftHandover in AppSettings.Instance.NotWritedShiftHandovers)
                    {
                        if (await Database.WriteShiftHandover(shiftHandover) == DbResult.Ok) AppSettings.Instance.NotWritedShiftHandovers.Remove(shiftHandover);
                    }
                    AppSettings.Save();
                }
                catch (Exception ex) { WriteLog(ex); }
                finally
                {
                    Thread.Sleep(30000);
                }
            }
        }

        private async Task SyncParts()
        {
            while (true)
            {
                try
                {
                    if (_editPart) throw new InvalidOperationException();
                    Status = "";
                    _needSave = false;
                    for (var i = Parts.Count - 1; i >= 0; i--)
                    {
                        if (ProgressBarVisibility == Visibility.Visible) break;
                        if (Parts[i] is not { IsSynced: false, IsFinished: not Part.State.InProgress } part) continue;
                        var partName = part.Name.TrimLen(86);

                        switch (AppSettings.Instance.StorageType.Type)
                        {
                            case StorageType.Types.Database:
                                await SyncWithDatabase(part, partName);
                                break;
                            case StorageType.Types.Excel:
                                await WriteToXlAsync(part, i, partName);
                                break;
                            case StorageType.Types.All:
                                await SyncWithDatabase(part, partName);
                                part.IsSynced = false;
                                await WriteToXlAsync(part, i, partName);
                                break;
                        }

                        ProgressBarVisibility = Visibility.Hidden;
                        OnPropertyChanged(nameof(part.Title));
                    }


                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SetPartsTaskInfo();
                        }
                        catch (Exception ex)
                        {
                            await WriteLogAsync(ex);
                        }
                    });


                    //await WriteTasksStatuses();
                    foreach (var part in Parts)
                    {
                        part.NotifyTaskStatus();
                        OnPropertyChanged(nameof(part.TaskInfo));
                        OnPropertyChanged(nameof(part.IsTaskStatusWritten));
                        OnPropertyChanged(nameof(part.NeedToSyncTask));
                    }
                    await Task.Delay(2000);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        RemoveExcessParts();
                    });
                    await Task.Delay(1000);
                    if (_needSave)
                    {
                        AppSettings.Instance.Parts = Parts;
                        AppSettings.Save();
                    }
                    await Task.Delay(1000);
                }
                catch (InvalidOperationException)
                {
                    Status = "";
                    ProgressBarVisibility = Visibility.Hidden;
                    await Task.Delay(30000);
                }
                catch (HttpRequestException)
                {
                    Status = "Ошибка соединения";
                    WriteLog("Ошибка соединения.");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    Status = e.Message;
                    WriteLog(e, "Ошибка синхронизации");
                }
                finally
                {
                    ProgressBarVisibility = Visibility.Hidden;
                }
                await Task.Delay(10000);
            }
        }
        private async Task SyncWithDatabase(Part part, string partName)
        {
            ProgressBarVisibility = Visibility.Visible;
            Status = $"Запись в БД: [{partName}]";
            var writeResult = part.Id == -1 ? await Database.WritePartAsync(part) : await Database.UpdatePartAsync(part);
            switch (writeResult)
            {
                case DbResult.Ok:
                    part.IsSynced = true;
                    Status = $"Готово";
                    break;
                case DbResult.AuthError:
                    Status = $"Ошибка авторизации в БД";
                    break;
                case DbResult.NoConnection:
                    Status = $"БД недоступна";
                    break;
                case DbResult.Error:
                    Status = $"Ошибка записи в БД";
                    break;
            }
            ProgressBarVisibility = Visibility.Hidden;
        }

        async Task SetPartsTaskInfo()
        {
            lock (lockObject)
            {
                int gStatus = 0;
                Progress<(int, string)> gProgress = new(p =>
                {
                    gStatus = p.Item1;
                    Status = p.Item2;
                });
                if (string.IsNullOrEmpty(AppSettings.Instance.GsId) || !File.Exists(AppSettings.Instance.GoogleCredentialsPath)) return;
                foreach (var part in Parts.Where(p => p.TaskInfo == Part.PartTaskInfo.NoData || (p.TaskInfo == Part.PartTaskInfo.InList && !p.IsTaskStatusWritten)))
                {
                    if (Parts.Count > 0 && Parts[0].FullName == part.FullName && Parts[0].Setup == part.Setup && Parts.IndexOf(part) > 0) return;
                    if (_editPart) return;
                    var partPosition = part.GetPositionInTasksList(gProgress).Result;
                    if (_editPart) return;
                    part.TaskInfo = gStatus switch
                    {
                        2 => Part.PartTaskInfo.NotInList,
                        3 => Part.PartTaskInfo.InList,
                        _ => Part.PartTaskInfo.NoData,
                    };
                    part.NotifyTaskStatus();
                    gStatus = 0;
                    if (string.IsNullOrEmpty(partPosition)) continue;
                    if (_editPart) return;
                    if (AppSettings.Instance.WiteToGs && part.TaskInfo is Part.PartTaskInfo.InList && !part.IsTaskStatusWritten)
                    {
                        var inProgress = part.IsFinished == Part.State.InProgress || (part.IsFinished == Part.State.PartialSetup && part.FinishedCount == 0);
                        var gs = new GoogleSheet(AppSettings.Instance.GoogleCredentialsPath, AppSettings.Instance.GsId);
                        gs.UpdateCellValue(partPosition, inProgress ? $"(уст {part.Setup}) в работе" : $"(уст {part.Setup}) готово", gProgress).GetAwaiter().GetResult();
                    }
                    if (gStatus == 1) part.IsTaskStatusWritten = true;
                    part.NotifyTaskStatus();
                }
            }

        }

        private void RemoveExcessParts(int remains = 20)
        {
            if (Parts.Count(p => p.IsSynced) > remains)
            {

                foreach (var part in Parts.Skip(remains))
                {
                    var i = Parts.IndexOf(part);
                    if (part.IsSynced) Parts.RemoveAt(i);
                    break;
                }
                OnPropertyChanged(nameof(Parts));
            }
        }

        private async Task<bool> WriteToXlAsync(Part part, int i, string partName)
        {

            Status = $"Синхронизация: [{partName}]";
            ProgressBarVisibility = Visibility.Visible;
            _needSave = true;
            var index = i;

            var progress = new Progress<string>(m => Status = m);

            if (part.Id != -1)
            {
                var rewriteResult = await part.RewriteToXlAsync(progress);
                switch (rewriteResult)
                {
                    case WriteResult.Ok:
                        part.IsSynced = true;
                        Status = $"Информация обновлена: [{part.Order} - {partName}, Уст №{part.Setup}]";
                        break;
                    case WriteResult.FileNotExist:
                        Status = "Таблица не найдена.";
                        ProgressBarVisibility = Visibility.Hidden;
                        await Task.Delay(300000);
                        break;
                    case WriteResult.IOError:
                        Status = "Таблица занята.";
                        ProgressBarVisibility = Visibility.Hidden;
                        Thread.Sleep(new Random().Next(5000, 60000));
                        break;
                    case WriteResult.Error:
                        Status = "Ошибка записи.";
                        ProgressBarVisibility = Visibility.Hidden;
                        await Task.Delay(10000);
                        break;
                    case WriteResult.DontNeed:
                        ProgressBarVisibility = Visibility.Hidden;
                        await Task.Delay(10000);
                        break;
                    case WriteResult.NotFinded:
                        part.Id = await part.WriteToXlAsync(progress);
                        if (part.Id == -1) return false;
                        part.IsSynced = true;
                        Status = $"Информация записана: [{part.Order} - {partName}, Уст №{part.Setup}]";
                        ProgressBarVisibility = Visibility.Hidden;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (rewriteResult is not WriteResult.Ok) return false;
            }
            else
            {
                var res = await part.WriteToXlAsync(progress);
                switch (res)
                {
                    // IO Ecxeption
                    case -4:
                        Status = "Таблица занята.";
                        ProgressBarVisibility = Visibility.Hidden;
                        Thread.Sleep(new Random().Next(5000, 60000));
                        break;
                    // Not Exists
                    case -3:
                        Status = "Таблица не найдена.";
                        ProgressBarVisibility = Visibility.Hidden;
                        await Task.Delay(300000);
                        break;
                    // Part exists
                    case -2:
                        Status = "Обнаружен совпадающий GUID, отмена записи.";
                        ProgressBarVisibility = Visibility.Hidden;
                        break;
                    // Прочие ошибки
                    case -1:
                        ProgressBarVisibility = Visibility.Hidden;
                        break;
                    // Ок
                    default:
                        part.IsSynced = true;
                        ProgressBarVisibility = Visibility.Hidden;
                        Status = $"Информация записана: [{part.Order} - {partName}, Уст №{part.Setup}]";
                        break;
                }
                if (part.Id == -1) return false;
            }
            return true;
        }
    }
}