using eLog.Infrastructure;
using eLog.Infrastructure.Commands;
using eLog.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using eLog.Models;
using eLog.Services;
using eLog.Infrastructure.Extensions;

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
        public bool WorkIsNotInProgress => Parts.Count == 0 || Parts.Count == Parts.Count(x => x.IsFinished);

        public bool DownTimeInProgress => Parts.Count == Parts.Count(x => x.DownTimes.Count > 0);

        public bool Overlay { get; set; }

        private bool _ShiftStarted = AppSettings.IsShiftStarted;
        public bool ShiftStarted
        {
            get => _ShiftStarted;
            set 
            {
                AppSettings.IsShiftStarted = value;
                AppSettings.RewriteConfig();
                Set(ref _ShiftStarted, value);
            } 
        }

        public static string[] Shifts => new []{ Text.DayShift, Text.NightShift };
        private string _CurrentShift = AppSettings.CurrentShift;

        public string CurrentShift
        {
            get => _CurrentShift;
            set
            {
                AppSettings.CurrentShift = value;
                AppSettings.RewriteConfig();
                Set(ref _CurrentShift, value);
                
            }
        }

        /// <summary> Детали </summary>
        private ObservableCollection<PartInfoModel> _Parts = new();

        public ObservableCollection<PartInfoModel> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
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
        private void OnStartShiftCommandExecuted(object p) => ShiftStarted = true;
        private static bool CanStartShiftCommandExecute(object p) => true;
        #endregion

        #region EndShift
        public ICommand EndShiftCommand { get; }
        private void OnEndShiftCommandExecuted(object p) => ShiftStarted = false;
        private static bool CanEndShiftCommandExecute(object p) => true;
        #endregion

        #region EditSettingsCommand
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            OverlayOn();
            var settings = new AppSettingsModel(Machine, AppSettings.XlPath, AppSettings.OrdersSourcePath, AppSettings.OrderQualifiers, Operators, AppSettings.CurrentShift);
            WindowsUserDialogService windowsUserDialogService = new();

            if (windowsUserDialogService.Edit(settings))
            {
                AppSettings.Machine = settings.Machine;
                Machine = AppSettings.Machine;
                AppSettings.XlPath = settings.XlPath;
                AppSettings.OrdersSourcePath = settings.OrdersSourcePath;
                AppSettings.OrderQualifiers = settings.OrderQualifiers;
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
        
        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private void OnStartDetailCommandExecuted(object p)
        {
            OverlayOn();

            var part = new PartInfoModel();
            if (WindowsUserDialogService.EditDetail(ref part))
            {
                switch (part)
                {
                    case { Id: -1, IsFinished: true }:
                        part.Id = part.WriteToXl();
                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        break;
                    case { Id: > 0, IsFinished: true }:
                    {
                        if (part.RewriteToXl())
                        {
                            Status = $"Информация об изготовлении id{part.Id} обновлена.";
                        }
                        break;
                    }
                }
                part.Shift = AppSettings.CurrentShift;
                Parts.Add(part);
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
            }
            OnPropertyChanged(nameof(WorkIsNotInProgress));
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
                switch (part)
                {
                    case { Id: -1, IsFinished: true }:
                        part.Id = part.WriteToXl();
                        Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                        break;
                    case { Id: > 0, IsFinished: true }:
                    {
                        if (part.RewriteToXl())
                        {
                            Status = $"Информация об изготовлении id{part.Id} обновлена.";
                        }
                        break;
                    }
                }
                Parts[Parts.IndexOf((PartInfoModel)p)] = part;
            }
            OnPropertyChanged(nameof(WorkIsNotInProgress));
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
            var res = WindowsUserDialogService.FinishDetail(ref part);
            switch (res)
            {
                case EndDetailResult.Finish or EndDetailResult.FinishAndNext when part.FinishedCount > 0:
                    part.Id = part.WriteToXl();
                    Status = $"Информация об изготовлении id{part.Id} зафиксирована.";
                    Parts[Parts.IndexOf((PartInfoModel)p)] = part;
                    if (res is EndDetailResult.FinishAndNext) StartDetailCommand.Execute(true);
                    break;
                case EndDetailResult.Finish or EndDetailResult.FinishAndNext when part.FinishedCount == 0:
                    Parts.Remove((PartInfoModel)p);
                    break;
            }
            OnPropertyChanged(nameof(WorkIsNotInProgress));
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
        }
    }
}
