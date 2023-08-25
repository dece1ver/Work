using eLog.Infrastructure;
using eLog.Infrastructure.Commands;
using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using eLog.Models;
using eLog.Services;
using eLog.Infrastructure.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using eLog.Infrastructure.Interfaces;
using eLog.Views.Windows.Settings;
using Microsoft.Win32;
using eLog.Views.Windows.Dialogs;

namespace eLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
        bool _startNewPart = false;
        public MainWindowViewModel()
        {
            AppSettings.DebugMode = true;
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

        public bool CanAddPart => ShiftStarted && WorkIsNotInProgress && CurrentOperator is {};
        public bool CanStartShift => CurrentOperator is {} && !string.IsNullOrEmpty(CurrentShift);
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
                var aboutWindow = new AboutWindow() { Owner = Application.Current.MainWindow };
                aboutWindow.ShowDialog();
            }
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private async void OnStartDetailCommandExecuted(object p)
        {
            _startNewPart = true;
            if (AppSettings.DebugMode) Util.WriteLog($"Старт новой детали.\n\tОператор {AppSettings.Instance.CurrentOperator?.DisplayName}\n\tВсего деталей: {Parts.Count}");
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
                    if (AppSettings.DebugMode) Util.WriteLog($"Отмена старта.\n\tВсего деталей: {Parts.Count}");
                    _startNewPart = false;
                    return; 
                }
                if (AppSettings.DebugMode) Util.WriteLog($"Подтверждение старта.\n\tВсего деталей: {Parts.Count}");
                //ProgressBarVisibility = Visibility.Visible;
                //Status = "Запись в процессе";
                //await Task.Run(() =>
                //{
                //    switch (part)
                //    {
                //        case { IsFinished: not Part.State.InProgress }:
                //            Progress = 0;
                //            part.Id = part.WriteToXl();
                //            if (part.Id > 0)
                //            {
                //                part.IsSynced = true;
                //                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                //            }

                //            break;
                //    }
                //});
                //ProgressBarVisibility = Visibility.Collapsed;
                Status = string.Empty;
                if (AppSettings.DebugMode) Util.WriteLog($"Добавление в список.\n\tДеталь: {part.Name}");
                Parts.Insert(0, part);
                if (AppSettings.DebugMode) Util.WriteLog($"Добавлено.\n\tВсего деталей: {Parts.Count}");
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                OnPropertyChanged(nameof(Parts));
                AppSettings.Save();
            }
            _startNewPart = false;
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
                    downTimes.Add(new DownTime(part, type));
                    part.DownTimes = downTimes;
                    OnPropertyChanged(nameof(part.DownTimes));
                    OnPropertyChanged(nameof(part.DownTimesIsClosed));
                    Parts.Insert(index, part);
                    OnPropertyChanged(nameof(Parts));
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
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
            
                if (part.LastDownTime is {InProgress: true} 
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
                    
                        part.LastDownTime.EndTimeText = DateTime.Now.ToString(Text.DateTimeFormat);
                    
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
                var index = Parts.IndexOf((Part)p);
                var part = Parts[index];
                switch (WindowsUserDialogService.GetSetupResult())
                {
                    case EndSetupResult.Success:
                        part.StartMachiningTime = DateTime.Now.Rounded();
                        _ = Util.SetPartialState(ref part);
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
            using (Overlay = new())
            {
                var part = new Part((Part)p);
                var index = Parts.IndexOf((Part)p);
            
                if (WindowsUserDialogService.EditDetail(ref part))
                {
                    part.IsSynced = false;
                    OnPropertyChanged(nameof(part.Title));

                    {
                        //ProgressBarVisibility = Visibility.Visible;
                        //Status = "Запись в процессе";
                        //await Task.Run(() =>
                        //{
                        //    switch (part)
                        //    {
                        //        case { Id: -1, IsFinished: not Part.State.InProgress }:
                        //            part.Id = part.WriteToXl();
                        //            if (part.Id > 0)
                        //            {
                        //                part.IsSynced = true;
                        //                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        //            }
                        //            break;
                        //        case { Id: > 0, IsFinished: not Part.State.InProgress }:
                        //        {
                        //            switch (part.RewriteToXl())
                        //            {
                        //                case Util.WriteResult.Ok:
                        //                    part.IsSynced = true;
                        //                    Status = $"Информация об изготовлении id{part.Id} обновлена.";
                        //                    break;
                        //                case Util.WriteResult.IOError:
                        //                    Status = $"Таблица занята, запись будет произведена позже.";
                        //                    break;
                        //                case Util.WriteResult.NotFinded:
                        //                    part.Id = part.WriteToXl();
                        //                    if (part.Id > 0)
                        //                    {
                        //                        part.IsSynced = true;
                        //                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        //                    }
                        //                    break;
                        //                case Util.WriteResult.Error:
                        //                    break;
                        //            }
                        //            break;
                        //        }
                        //    }
                        //});
                        //ProgressBarVisibility = Visibility.Collapsed;
                    } // попытка записи сразу

                    Status = string.Empty;
                    Parts[index] = part;
                    //Parts.RemoveAt(index);
                    //Parts.Insert(index, part);
                    _ = Util.SetPartialState(ref part);
                    if (part.StartMachiningTime == DateTime.MinValue) part.DownTimes = new DeepObservableCollection<DownTime>(part.DownTimes.Where(dt => dt.Type != DownTime.Types.PartialSetup));
                    OnPropertyChanged(nameof(Parts));
                    AppSettings.Instance.Parts = Parts;
                    AppSettings.Save();
                }
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                OnPropertyChanged(nameof(Parts));
                RemoveExcessParts();
            }
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
                    //ProgressBarVisibility = Visibility.Visible;
                    //Status = "Запись в процессе";
                    //await Task.Run(() =>
                    //{
                    //    switch (part)
                    //    {
                    //        case { Id: -1, IsFinished: not Part.State.InProgress }:
                    //            if (part is { FinishedCount: 1, SetupTimeFact.Ticks: > 0 }) part.StartMachiningTime = part.EndMachiningTime;
                    //            part.Id = part.WriteToXl();
                    //            if (part.Id > 0)
                    //            {
                    //                part.IsSynced = true;
                    //                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                    //            }
                    //            Application.Current.Dispatcher.Invoke(delegate { Parts[Parts.IndexOf((Part)p)] = part; });
                    //            break;
                    //        case { Id: > 0, IsFinished: not Part.State.InProgress }:
                    //            if (part is { FinishedCount: 1, SetupTimeFact.Ticks: > 0 }) part.StartMachiningTime = part.EndMachiningTime;
                    //            switch (part.RewriteToXl())
                    //            {
                    //                case Util.WriteResult.Ok:
                    //                    part.IsSynced = true;
                    //                    Status = $"Информация об изготовлении id{part.Id} обновлена.";
                    //                    Application.Current.Dispatcher.Invoke(delegate { Parts[Parts.IndexOf((Part)p)] = part; });
                    //                    break;
                    //                case Util.WriteResult.IOError:
                    //                    Status = $"Таблица занята, запись будет произведена позже.";
                    //                    Application.Current.Dispatcher.Invoke(delegate { Parts[Parts.IndexOf((Part)p)] = part; });
                    //                    break;
                    //                case Util.WriteResult.NotFinded:
                    //                    part.Id = part.WriteToXl();
                    //                    if (part.Id > 0)
                    //                    {
                    //                        part.IsSynced = true;
                    //                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                    //                    }
                    //                    Application.Current.Dispatcher.Invoke(delegate { Parts[Parts.IndexOf((Part)p)] = part; });
                    //                    break;
                    //                case Util.WriteResult.Error:
                    //                    break;
                    //            }
                    //            break;
                    //        case { FinishedCount: 0 }:
                    //            var now = DateTime.Now;
                    //            part.StartMachiningTime = now;
                    //            part.EndMachiningTime = now;
                    //            part.FinishedCount = 0;
                    //            part.Id = part.WriteToXl();
                    //            if (part.Id > 0)
                    //            {
                    //                part.IsSynced = true;
                    //                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                    //            }
                    //            Application.Current.Dispatcher.Invoke(delegate { Parts[Parts.IndexOf((Part)p)] = part; });
                    //            break;
                    //    }
                    //});
                    //ProgressBarVisibility = Visibility.Collapsed;
                    Status = string.Empty;
                    //Parts[index] = part;
                    Parts.RemoveAt(index);
                    Parts.Insert(index, part);
                    OnPropertyChanged(nameof(Parts));
                    RemoveExcessParts();
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
                    if (_startNewPart) throw new InvalidOperationException();
                    Status = "";
                    bool needSave = false;
                    for (var i = Parts.Count - 1; i >= 0; i--)
                    {
                        if (ProgressBarVisibility == Visibility.Visible) break;
                        if (Parts[i] is not { IsSynced: false, IsFinished: not Part.State.InProgress } part) continue;
                        var partName = part.Name.Length >= 83 ? part.Name[..80] + "..." : part.Name;
                        Status = $"Синхронизация: [{partName}]";
                        ProgressBarVisibility = Visibility.Visible;
                        needSave = true;
                        //Thread.Sleep(1000);
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
                                case Util.WriteResult.IOError or Util.WriteResult.Error or Util.WriteResult.FileNotExist or Util.WriteResult.DontNeed:
                                    continue;
                                case Util.WriteResult.NotFinded:
                                    part.Id = part.WriteToXl();
                                    if (part.Id == -1) continue;
                                    part.IsSynced = true;
                                    Status = $"Информация записана: [{partName}]";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            if (part.RewriteToXl() is not Util.WriteResult.Ok) continue;
                        }
                        else
                        {
                            var res = part.WriteToXl();
                            switch (res)
                            {
                                case -2:
                                    break;
                                case -1:
                                    continue;
                                default:
                                    part.IsSynced = true;
                                    Status = $"Информация записана: [{partName}]";
                                    break;
                            }
                            if (part.Id == -1) continue;

                            
                        }
                        ProgressBarVisibility = Visibility.Hidden;
                        OnPropertyChanged(nameof(part.Title));
                        //Application.Current.Dispatcher.Invoke(delegate
                        //{
                        //    Parts[index] = part;
                        //    Parts.RemoveAt(index);
                        //    Parts.Insert(index, part);
                        //});

                    }
                    Thread.Sleep(1000);
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        RemoveExcessParts();
                    });
                    Thread.Sleep(1000);
                    if (needSave)
                    {
                        AppSettings.Instance.Parts = Parts;
                        AppSettings.Save();
                    }
                    Thread.Sleep(2000);
                }
                catch (InvalidOperationException)
                {
                    Thread.Sleep(5000);
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
                Thread.Sleep(20000);
            }
        }

        private void RemoveExcessParts(int remains = 20)
        {
            var syncedPartsCount = Parts.Count(p => p.IsSynced);

            if (syncedPartsCount > remains)
            {
                var partsToRemove = Parts
                    .Where(p => p.IsSynced)
                    .Reverse()
                    .Take(syncedPartsCount - remains);

                foreach (var part in partsToRemove.ToList())
                {
                    Parts.Remove(part);
                }
                Parts = new DeepObservableCollection<Part>(Parts.Distinct());
                OnPropertyChanged(nameof(Parts));
            }
        }
    }
}
