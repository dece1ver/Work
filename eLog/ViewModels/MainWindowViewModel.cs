using DocumentFormat.OpenXml.Wordprocessing;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Models;
using eLog.Services;
using eLog.Views.Windows.Dialogs;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static eLog.Infrastructure.Extensions.Util;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Text = eLog.Infrastructure.Extensions.Text;

namespace eLog.ViewModels;

internal class MainWindowViewModel : ViewModel, IOverlay
{
    bool _editPart = false;
    bool _needSave = false;
    public MainWindowViewModel()
    {
        CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);

        StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
        EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);

        StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
        EndSetupCommand = new LambdaCommand(OnEndSetupCommandExecuted, CanEndSetupCommandExecute);
        SetDownTimeCommand = new LambdaCommand(OnSetDownTimeCommandExecuted, CanSetDownTimeCommandExecute);
        EndDownTimeCommand = new LambdaCommand(OnEndDownTimeCommandExecuted, CanEndDownTimeCommandExecute);
        EditDetailCommand = new LambdaCommand(OnEditDetailCommandExecuted, CanEditDetailCommandExecute);
        EndDetailCommand = new LambdaCommand(OnEndDetailCommandExecuted, CanEndDetailCommandExecute);

        EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
        EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
        ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);

        Parts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Parts));

        var syncPartsThread = new Thread(SyncParts) { IsBackground = true };
        syncPartsThread.Start();
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

    public bool WorkIsNotInProgress => Parts.Count == 0 || Parts.Count == Parts.Count(x => x.IsFinished is not Part.State.InProgress);

    public bool CanAddPart => ShiftStarted && WorkIsNotInProgress && CurrentOperator is { };
    public bool CanStartShift => CurrentOperator is { } && !string.IsNullOrEmpty(CurrentShift);
    public bool CanEditShiftAndParams => !ShiftStarted && WorkIsNotInProgress;
    public bool CanEndShift => ShiftStarted && WorkIsNotInProgress;

    private bool _ShiftStarted = AppSettings.Instance.IsShiftStarted;
    private Visibility _ProgressBarVisibility = Visibility.Collapsed;

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
            }
        }
    }
    private static bool CanShowAboutCommandExecute(object p) => true;
    #endregion

    #region StartDetail
    public ICommand StartDetailCommand { get; }
    private async void OnStartDetailCommandExecuted(object p)
    {
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
            if (!WindowsUserDialogService.EditDetail(ref part, true))
            {
                if (AppSettings.Instance.DebugMode) WriteLog($"Отмена старта.\n\tВсего деталей: {Parts.Count}");
                _editPart = false;
                return;
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
            var downTimeType = WindowsUserDialogService.SetDownTimeType();
            if (downTimeType is { } type)
            {
                var part = (Part)p;
                var index = Parts.IndexOf(part);
                Parts.RemoveAt(index);
                var downTimes = part.DownTimes;
                var downTime = new DownTime(part, type);
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
                if (AppSettings.Instance.DebugMode) { WriteLog(part, $"Завершен простой [{part.LastDownTime.Name}]"); }
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
    private async void OnEditDetailCommandExecuted(object p)
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
    private async void OnEndDetailCommandExecuted(object p)
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

    #endregion


    private void SyncParts()
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
                    // if (AppSettings.Instance.DebugMode) { WriteLog(part, "Нужна синхронизация"); }
                    var partName = part.Name.Length >= 83 ? part.Name[..80] + "..." : part.Name;



#pragma warning disable CA1416 // Проверка совместимости платформы
                    switch (AppSettings.Instance.StorageType.Type)
                    {
                        case StorageType.Types.Database:

                            ProgressBarVisibility = Visibility.Visible;
                            Status = $"Запись в БД: [{partName}]";
                            var writeResult = part.Id == -1 ? Database.WritePart(part) : Database.UpdatePart(part);
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
                            
                            break;
                        case StorageType.Types.Excel:
                            WriteToXl(part, i, partName);
                            break;
                        case StorageType.Types.All:
                            ProgressBarVisibility = Visibility.Visible;
                            Status = $"Запись в БД: {partName}";
                            _ = part.Id == -1 ? Database.WritePart(part, true) : Database.UpdatePart(part, true);
                            Status = $"Готово";
                            WriteToXl(part, i, partName);
                            break;
                        default:
                            break;
                    }
#pragma warning restore CA1416 // Проверка совместимости платформы
                    ProgressBarVisibility = Visibility.Hidden;
                    OnPropertyChanged(nameof(part.Title));
                    //Application.Current.Dispatcher.Invoke(delegate
                    //{
                    //    Parts[index] = part;
                    //    Parts.RemoveAt(index);
                    //    Parts.Insert(index, part);
                    //});

                }
                Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    RemoveExcessParts();
                });
                Thread.Sleep(2000);
                if (_needSave)
                {
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
                }
                Thread.Sleep(2000);
            }
            catch (InvalidOperationException)
            {
                Status = "";
                ProgressBarVisibility = Visibility.Hidden;
                Thread.Sleep(30000);
            }
            catch (Exception e)
            {
                Status = e.Message;
                Util.WriteLog(e, "Ошибка синхронизации.");
            }
            finally
            {
                ProgressBarVisibility = Visibility.Hidden;
            }
            Thread.Sleep(10000);
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

    private bool WriteToXl(Part part, int i, string partName)
    {
        Status = $"Синхронизация: [{partName}]";
        ProgressBarVisibility = Visibility.Visible;
        _needSave = true;
        var index = i;
        if (part.Id != -1)
        {
            var rewriteResult = part.RewriteToXl();
            switch (rewriteResult)
            {
                case Util.WriteResult.Ok:
                    part.IsSynced = true;
                    Status = $"Информация обновлена: [{partName}]";
                    break;
                case Util.WriteResult.FileNotExist:
                    Status = "Таблица не найдена.";
                    ProgressBarVisibility = Visibility.Hidden;
                    Thread.Sleep(300000);
                    break;
                case Util.WriteResult.IOError:
                    Status = "Таблица занята.";
                    ProgressBarVisibility = Visibility.Hidden;
                    Thread.Sleep(new Random().Next(5000, 60000));
                    break;
                case Util.WriteResult.Error:
                    Status = "Ошибка записи.";
                    ProgressBarVisibility = Visibility.Hidden;
                    Thread.Sleep(10000);
                    break;
                case Util.WriteResult.DontNeed:
                    ProgressBarVisibility = Visibility.Hidden;
                    Thread.Sleep(10000);
                    break;
                case Util.WriteResult.NotFinded:
                    part.Id = part.WriteToXl();
                    if (part.Id == -1) return false;
                    part.IsSynced = true;
                    Status = $"Информация записана: [{partName}]";
                    ProgressBarVisibility = Visibility.Hidden;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (rewriteResult is not Util.WriteResult.Ok) return false;
        }
        else
        {
            var res = part.WriteToXl();
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
                    Thread.Sleep(300000);
                    break;
                // Part exists
                case -2:
                    Status = "Отмена записи, деталь была изменена.";
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
                    Status = $"Информация записана: [{partName}]";
                    break;
            }
            if (part.Id == -1) return false;
        }
        return true;
    }
}
