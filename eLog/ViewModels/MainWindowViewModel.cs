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

namespace eLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private Machine _Machine = AppSettings.Machine;
        /// <summary> Станок </summary>
        public Machine Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        private Operator? _CurrentOperator = AppSettings.CurrentOperator;
        /// <summary> Текущий оператор </summary>
        public Operator? CurrentOperator
        {
            get => _CurrentOperator;
            set {
                AppSettings.CurrentOperator = value;
                AppSettings.RewriteConfig();
                Set(ref _CurrentOperator, value);
            }
        }

        private ObservableCollection<Operator> _Operators = AppSettings.Operators;
        /// <summary> Список операторов </summary>
        public ObservableCollection<Operator> Operators
        {
            get => _Operators;
            set {
                AppSettings.Operators = value;
                AppSettings.RewriteConfig();
                Set(ref _Operators, value); 
            }
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

        public bool CanAddPart => ShiftStarted && WorkIsNotInProgress;
        public bool CanEditShiftAndParams => !ShiftStarted && WorkIsNotInProgress;
        public bool CanEndShift => ShiftStarted && WorkIsNotInProgress;

        public bool DownTimeInProgress => Parts.Count == Parts.Count(x => x.DownTimes.Count > 0);

        public bool Overlay { get; set; }

        private bool _ShiftStarted = AppSettings.IsShiftStarted;
        public bool ShiftStarted
        {
            get => _ShiftStarted;
            set 
            {
                if (!Set(ref _ShiftStarted, value)) return;
                AppSettings.IsShiftStarted = value;
                AppSettings.RewriteConfig();
                OnPropertyChanged(nameof(WorkIsNotInProgress));
                OnPropertyChanged(nameof(CanEditShiftAndParams));
                OnPropertyChanged(nameof(CanAddPart));
            } 
        }

        public static string[] Shifts => Text.Shifts;
        private string _CurrentShift = AppSettings.CurrentShift;

        public string CurrentShift
        {
            get => _CurrentShift;
            set
            {
                if (!Set(ref _CurrentShift, value)) return;
                AppSettings.CurrentShift = value;
                AppSettings.RewriteConfig();
            }
        }

        /// <summary> Детали </summary>
        private ObservableCollection<PartInfoModel> _Parts = AppSettings.Parts;

        public ObservableCollection<PartInfoModel> Parts
        {
            get => _Parts;
            set
            {
                Set(ref _Parts, value);
                AppSettings.Parts = _Parts;
                AppSettings.RewriteConfig();
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

        #region EditSettingsCommand
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            OverlayOn();
            var settings = new AppSettingsModel(Machine, AppSettings.XlPath, AppSettings.OrdersSourcePath, AppSettings.OrderQualifiers, Operators, AppSettings.CurrentShift, Parts);
            WindowsUserDialogService windowsUserDialogService = new();

            if (windowsUserDialogService.Edit(settings))
            {
                AppSettings.Machine = settings.Machine;
                Machine = AppSettings.Machine;
                AppSettings.XlPath = settings.XlPath;
                AppSettings.OrdersSourcePath = settings.OrdersSourcePath;
                AppSettings.OrderQualifiers = settings.OrderQualifiers;
                AppSettings.Parts = settings.Parts;
                AppSettings.RewriteConfig();
            }
            OverlayOff();

        }
        private static bool CanEditSettingsCommandExecute(object p) => true;
        #endregion

        #region EditOperatorsCommand
        public ICommand EditOperatorsCommand { get; }
        private void OnEditOperatorsCommandExecuted(object p)
        {
            OverlayOn();
            var operators = Operators;
            WindowsUserDialogService windowsUserDialogService = new();
            
            if (windowsUserDialogService.Edit(operators))
            {
                var tempOperators = operators.ToList();

                // костыль надо нормально сделать
                tempOperators.RemoveAll(x => string.IsNullOrWhiteSpace(x.DisplayName));
                operators = new ObservableCollection<Operator>();
                foreach (var op in tempOperators)
                {
                    operators.Add(op);
                }
                Operators = operators;
            }
            OverlayOff();
            OnPropertyChanged(nameof(Operators));
            OnPropertyChanged(nameof(CurrentOperator));
        }
        private static bool CanEditOperatorsCommandExecute(object p) => true;
        #endregion

        #region EditSettingsCommand
        public ICommand ShowAboutCommand { get; }
        private void OnShowAboutCommandExecuted(object p)
        {
            var exe = Environment.ProcessPath;
            var date = exe is null ? string.Empty : $" от {File.GetCreationTime(exe).ToString(Text.DateTimeFormat)}";
            MessageBox.Show(
                $"Версия программы: {Assembly.GetExecutingAssembly().GetName().Version}{date}", 
                "Версия программы", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private void OnStartDetailCommandExecuted(object p)
        {
            OverlayOn();

            var part = new PartInfoModel
            {
                Operator = CurrentOperator!,
                Shift = AppSettings.CurrentShift,
                Setup = 1,
            };
            if (WindowsUserDialogService.EditDetail(ref part, true))
            {
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
                AppSettings.Parts = Parts;
                AppSettings.RewriteConfig();
            }
            OverlayOff();
        }
        private static bool CanStartDetailCommandExecute(object p) => true;
        #endregion

        #region SetDownTime
        public ICommand SetDownTimeCommand { get; }
        private void OnSetDownTimeCommandExecuted(object p)
        {
            OverlayOn();
            var downTimeType = WindowsUserDialogService.SetDownTimeType();
            if (downTimeType is { } type)
            {
                // Parts[Parts.IndexOf((PartInfoModel)p)].DownTimes.Add(new DownTime(type));
            }
            OverlayOff();
        }
        private static bool CanSetDownTimeCommandExecute(object p) => true;
        #endregion

        #region EndSetup
        public ICommand EndSetupCommand { get; }
        private void OnEndSetupCommandExecuted(object p)
        {
            OverlayOn();
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
                    var part = Parts[Parts.IndexOf((PartInfoModel)p)];
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
                    Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                    break;
            }
            OnPropertyChanged(nameof(WorkIsNotInProgress));
            OnPropertyChanged(nameof(CanEditShiftAndParams));
            OnPropertyChanged(nameof(CanAddPart));
            OnPropertyChanged(nameof(CanEndShift));
            AppSettings.Parts = Parts;
            AppSettings.RewriteConfig();
            OverlayOff();
        }
        private static bool CanEndSetupCommandExecute(object p) => true;
        #endregion

        #region EditDetail
        public ICommand EditDetailCommand { get; }
        private void OnEditDetailCommandExecuted(object p)
        {
            OverlayOn();
            var part = (PartInfoModel)p;
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
                Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                AppSettings.Parts = Parts;
                AppSettings.RewriteConfig();
            }
            OnPropertyChanged(nameof(WorkIsNotInProgress));
            OnPropertyChanged(nameof(CanEditShiftAndParams));
            OnPropertyChanged(nameof(CanAddPart));
            OnPropertyChanged(nameof(CanEndShift));
            RemoveExcessParts();
            OverlayOff();
        }
        private static bool CanEditDetailCommandExecute(object p) => true;
        #endregion

        #region EndDetail
        public ICommand EndDetailCommand { get; }
        private void OnEndDetailCommandExecuted(object p)
        {
            OverlayOn();
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
                AppSettings.Parts = Parts;
                AppSettings.RewriteConfig();
                RemoveExcessParts();
            };
            
            OnPropertyChanged(nameof(WorkIsNotInProgress));
            OnPropertyChanged(nameof(CanEditShiftAndParams));
            OnPropertyChanged(nameof(CanAddPart));
            OnPropertyChanged(nameof(CanEndShift));
            OverlayOff();
        }
        private static bool CanEndDetailCommandExecute(object p) => true;
        #endregion


        #endregion

        private void OverlayOn()
        {
            Overlay = true;
            OnPropertyChanged(nameof(Overlay));
        }

        private void OverlayOff()
        {
            Overlay = false;
            OnPropertyChanged(nameof(Overlay));
        }

        

        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);

            StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
            EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);

            StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
            EndSetupCommand = new LambdaCommand(OnEndSetupCommandExecuted, CanEndSetupCommandExecute);
            SetDownTimeCommand = new LambdaCommand(OnSetDownTimeCommandExecuted, CanSetDownTimeCommandExecute);
            EditDetailCommand = new LambdaCommand(OnEditDetailCommandExecuted, CanEditDetailCommandExecute);
            EndDetailCommand = new LambdaCommand(OnEndDetailCommandExecuted, CanEndDetailCommandExecute);

            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
            ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);

            var syncPartsThread = new Thread(SyncParts) { IsBackground = true };
            syncPartsThread.Start();
        }

        private void SyncParts()
        {
            while (true)
            {
                try
                {
                    for (var i = 0; i < Parts.Count; i++)
                    {
                        if (Parts[i] is not { IsSynced: false, IsFinished: not PartInfoModel.State.InProgress } part) continue;
                        if (part.Id != -1)
                        {
                            if (part.RewriteToXl() is not Util.WriteResult.Ok) continue;
                            part.IsSynced = true;
                            Status = $"Информация об изготовлении id{part.Id} обновлена. (фон)";
                        }
                        else
                        {
                            part.Id = part.WriteToXl();
                            if (part.Id == -1) continue;
                            part.IsSynced = true;
                            Status = $"Информация об изготовлении id{part.Id} зафиксирована. (фон)";
                        }

                        var index = i;
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            Parts[index] = part;
                        });
                        OnPropertyChanged(nameof(part.Title));
                        OnPropertyChanged(nameof(Parts));
                    }

                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        RemoveExcessParts();
                    });
                }
                catch (Exception e)
                {
                    Status = e.Message;
                }
                Thread.Sleep(10000);
            }
        }

        private void RemoveExcessParts(int remains = 20)
        {
            while (Parts.Count(p => p.IsSynced) > remains)
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
