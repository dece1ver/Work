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
using eLog.Infrastructure.Interfaces;

namespace eLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
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

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

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
        public ObservableCollection<Operator> Operators
        {
            get => AppSettings.Instance.Operators;
            set => AppSettings.Instance.Operators = value;
        }

        private string _Status = string.Empty;
        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private double _Progress;
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

        private double _ProgressMaxValue;
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

        public Visibility ProgressBarVisibility => Progress is 0 || Math.Abs(Progress - ProgressMaxValue) < 0.001 ? Visibility.Collapsed : Visibility.Visible;
        public bool WorkIsNotInProgress => Parts.Count == 0 || Parts.Count == Parts.Count(x => x.IsFinished is not PartInfoModel.State.InProgress);

        public bool CanAddPart => ShiftStarted && WorkIsNotInProgress && CurrentOperator is {};
        public bool CanStartShift => CurrentOperator is {} && !string.IsNullOrEmpty(CurrentShift);
        public bool CanEditShiftAndParams => !ShiftStarted && WorkIsNotInProgress;
        public bool CanEndShift => ShiftStarted && WorkIsNotInProgress;

        private bool _ShiftStarted = AppSettings.Instance.IsShiftStarted;
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

        public static string[] Shifts => Text.Shifts;

        public string CurrentShift
        {
            get => AppSettings.Instance.CurrentShift;
            set => AppSettings.Instance.CurrentShift = value;
        }
        
        private Overlay _Overlay = new(false);

        /// <summary> Детали </summary>
        public static ObservableCollection<PartInfoModel> Parts
        {
            get => AppSettings.Instance.Parts;
            set => AppSettings.Instance.Parts = value;
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
                if (WindowsUserDialogService.EditSettings())
                    Machine = AppSettings.Instance.Machine;
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
                if (WindowsUserDialogService.EditOperators())
                {
                    CurrentOperator = null;
                }
            }
            OnPropertyChanged(nameof(Operators));
            OnPropertyChanged(nameof(CurrentOperator));
            OnPropertyChanged(nameof(CanAddPart));
            OnPropertyChanged(nameof(CanEndShift));
        }
        private static bool CanEditOperatorsCommandExecute(object p) => true;
        #endregion

        #region ShowAbout
        public ICommand ShowAboutCommand { get; }
        private void OnShowAboutCommandExecuted(object p)
        {
            var exe = Environment.ProcessPath;
            var date = exe is null ? string.Empty : $" от {File.GetLastWriteTime(exe).ToString(Text.DateTimeFormat)}";
            MessageBox.Show(
                $"Версия программы: {Assembly.GetExecutingAssembly().GetName().Version}{date}", 
                "О программе", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private void OnStartDetailCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var part = new PartInfoModel
                {
                    Operator = CurrentOperator!,
                    Shift = AppSettings.Instance.CurrentShift,
                    Setup = 1,
                };
                if (!WindowsUserDialogService.EditDetail(ref part, true)) return;
                switch (part)
                {
                    case { IsFinished: not PartInfoModel.State.InProgress }:
                        part.Id = part.WriteToXl();
                        if (part.Id > 0)
                        {
                            part.IsSynced = true;
                            Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        }
                        break;
                }
                Parts.Insert(0, part);
                RemoveExcessParts();
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
            }
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
                    var part = (PartInfoModel)p;
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
                var part = (PartInfoModel)p;
                var index = Parts.IndexOf((PartInfoModel)p);
            
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
                switch (WindowsUserDialogService.GetSetupResult())
                {
                    case EndSetupResult.Success:
                        Parts[Parts.IndexOf((PartInfoModel)p)].StartMachiningTime = DateTime.Now;
                        break;
                    case EndSetupResult.Stop:
                        Parts.Remove((PartInfoModel)p);
                        break;
                    case EndSetupResult.PartialComplete:
                        var now = DateTime.Now;
                        var index = Parts.IndexOf((PartInfoModel)p);
                        var part = Parts[index];
                        part.StartMachiningTime = now;
                        part.EndMachiningTime = now;
                        part.FinishedCount = 0;
                    

                        if (part.Id != -1)
                        {
                            if (part.RewriteToXl() is Util.WriteResult.Ok)
                            {
                                part.IsSynced = true;
                                Status = $"Информация об изготовлении id{part.Id} обновлена.";
                            }
                        }
                        else
                        {
                            part.Id = part.WriteToXl();
                            if (part.Id > 0)
                            {
                                part.IsSynced = true;
                                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                            }
                        }
                        Parts.RemoveAt(index);
                        Parts.Insert(index, part);
                        break;
                }
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                AppSettings.Instance.Parts = Parts;
            }
        }
        private static bool CanEndSetupCommandExecute(object p) => true;
        #endregion

        #region EditDetail
        public ICommand EditDetailCommand { get; }
        private void OnEditDetailCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var part = new PartInfoModel((PartInfoModel)p);
                var index = Parts.IndexOf((PartInfoModel)p);
            
                if (WindowsUserDialogService.EditDetail(ref part))
                {
                    part.IsSynced = false;
                    OnPropertyChanged(nameof(part.Title));
                    switch (part)
                    {
                        case { Id: -1, IsFinished: not PartInfoModel.State.InProgress }:
                            part.Id = part.WriteToXl();
                            if (part.Id > 0)
                            {
                                part.IsSynced = true;
                                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                            }
                            break;
                        case { Id: > 0, IsFinished: not PartInfoModel.State.InProgress }:
                        {
                            switch (part.RewriteToXl())
                            {
                                case Util.WriteResult.Ok:
                                    part.IsSynced = true;
                                    Status = $"Информация об изготовлении id{part.Id} обновлена.";
                                    break;
                                case Util.WriteResult.IOError:
                                    Status = $"Таблица занята, запись будет произведена позже.";
                                    break;
                                case Util.WriteResult.NotFinded:
                                    part.Id = part.WriteToXl();
                                    if (part.Id > 0)
                                    {
                                        part.IsSynced = true;
                                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                                    }
                                    break;
                                case Util.WriteResult.Error:
                                    break;
                            }
                            break;
                        }
                    }

                    Parts[index] = part;
                    AppSettings.Instance.Parts = Parts;
                }
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
                RemoveExcessParts();
            }
        }
        private static bool CanEditDetailCommandExecute(object p) => true;
        #endregion

        #region EndDetail
        public ICommand EndDetailCommand { get; }
        private void OnEndDetailCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var part = (PartInfoModel)p;
                if (WindowsUserDialogService.FinishDetail(ref part))
                {
                    switch (part)
                    {
                        case { Id: -1, IsFinished: not PartInfoModel.State.InProgress }:
                            part.Id = part.WriteToXl();
                            if (part.Id > 0)
                            {
                                part.IsSynced = true;
                                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                            }
                            Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                            break;
                        case { Id: > 0, IsFinished: not PartInfoModel.State.InProgress }:
                        {
                            switch (part.RewriteToXl())
                            {
                                case Util.WriteResult.Ok:
                                    part.IsSynced = true;
                                    Status = $"Информация об изготовлении id{part.Id} обновлена.";
                                    Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                                    break;
                                case Util.WriteResult.IOError:
                                    Status = $"Таблица занята, запись будет произведена позже.";
                                    Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                                    break;
                                case Util.WriteResult.NotFinded:
                                    part.Id = part.WriteToXl();
                                    if (part.Id > 0)
                                    {
                                        part.IsSynced = true;
                                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                                    }
                                    Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                                    break;
                                case Util.WriteResult.Error:
                                    break;
                            }
                            break;
                        }
                        case { FinishedCount: 0 }:
                        {
                            var now = DateTime.Now;
                            part.StartMachiningTime = now;
                            part.EndMachiningTime = now;
                            part.FinishedCount = 0;
                            part.Id = part.WriteToXl();
                            if (part.Id > 0)
                            {
                                part.IsSynced = true;
                                Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                            }
                            Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                            break;
                        }
                    }
                    AppSettings.Instance.Parts = Parts;
                    RemoveExcessParts();
                };
            
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
                OnPropertyChanged(nameof(CanEndShift));
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
                    for (var i = Parts.Count - 1; i >= 0; i--)
                    {
                        if (Parts[i] is not { IsSynced: false, IsFinished: not PartInfoModel.State.InProgress } part) continue;
                        var index = i;
                        if (part.Id != -1)
                        {
                            switch (part.RewriteToXl())
                            {
                                case Util.WriteResult.Ok:
                                    part.IsSynced = true;
                                    Status = $"Информация об изготовлении id{part.Id} обновлена. (фон)";
                                    break;
                                case Util.WriteResult.IOError or Util.WriteResult.Error:
                                    continue;
                                case Util.WriteResult.NotFinded:
                                    part.Id = part.WriteToXl();
                                    if (part.Id == -1) continue;
                                    part.IsSynced = true;
                                    Status = $"Информация об изготовлении id{part.Id} зафиксирована. (фон)";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            if (part.RewriteToXl() is not Util.WriteResult.Ok) continue;
                           
                        }
                        else
                        {
                            part.Id = part.WriteToXl();
                            if (part.Id == -1) continue;
                            part.IsSynced = true;
                            Status = $"Информация об изготовлении id{part.Id} зафиксирована. (фон)";
                        }
                        OnPropertyChanged(nameof(part.Title));
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            Parts.RemoveAt(index);
                            Parts.Insert(index, part);
                        });
                    }

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        
                        OnPropertyChanged(nameof(Parts));
                        RemoveExcessParts();
                    });
                }
                catch (Exception e)
                {
                    Status = e.Message;
                    Util.WriteLog(e, "Ошибка синхронизации.");
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
            }
            OnPropertyChanged(nameof(Parts));
        }
    }
}
