using eLog.Infrastructure;
using eLog.Infrastructure.Commands;
using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using eLog.Models;
using eLog.Views.Windows;
using eLog.Services.Interfaces;
using eLog.Services;
using eLog.Infrastructure.Extensions;
using System.Windows.Shapes;

namespace eLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private Machine _Machine = AppSettings.Machine;
        /// <summary>
        /// Станок
        /// </summary>
        public Machine Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        private Operator? _CurrentOperator = AppSettings.CurrentOperator;
        /// <summary>
        /// Текущий оператор
        /// </summary>
        public Operator? CurrentOperator
        {
            get => _CurrentOperator;
            set {
                AppSettings.CurrentOperator = value;
                AppSettings.RewriteConfig();
                Set(ref _CurrentOperator, value);
            }
        }

        public string CurrentOperatorDisplay { get => CurrentOperator!.DisplayName; }

        private ObservableCollection<Operator> _Operators = AppSettings.Operators;
        /// <summary>
        /// Список операторов
        /// </summary>
        public ObservableCollection<Operator> Operators
        {
            get => _Operators;
            set {
                AppSettings.Operators = value;
                AppSettings.RewriteConfig();
                Set(ref _Operators, value); 
            }
        }

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

        private string[] _Shifts = new[] {
            $"{DateTime.Today.AddDays(-1):dd.MM.yyyy 19:00-07:00}", 
            $"{DateTime.Today:dd.MM.yyyy 07:00-19:00}", 
            $"{DateTime.Today:dd.MM.yyyy 19:00-07:00}"};
        public string[] Shifts
        {
            get => _Shifts;
            set => Set(ref _Shifts, value);
        }

        public Visibility StartShiftButtonVisibility => ShiftStarted ? Visibility.Collapsed : Visibility.Visible;
        public Visibility EndShiftButtonVisibility => ShiftStarted ? Visibility.Visible : Visibility.Collapsed;

        public bool EditShiftInfoIsEnabled => !ShiftStarted;

        private string _Status = string.Empty;
        /// <summary>
        /// Статус
        /// </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private double _Progress;
        /// <summary>
        /// Значение прогрессбара
        /// </summary>
        public double Progress
        {
            get => _Progress;
            set => Set(ref _Progress, value);
        }

        private double _ProgressMaxValue;
        /// <summary>
        /// Максимальное значение прогрессбара
        /// </summary>
        public double ProgressMaxValue
        {
            get => _ProgressMaxValue;
            set => Set(ref _ProgressMaxValue, value);
        }

        private Visibility _ProgressBarVisibility = Visibility.Collapsed;

        public Visibility ProgressBarVisibility
        {
            get => _ProgressBarVisibility;
            set => Set(ref _ProgressBarVisibility, value);
        }

        public bool CanAddPart => Parts.Count == 0 || Parts.Count == Parts.Where(x => x.IsFinished).Count();

        /// <summary>
        /// Текущая деталь
        /// </summary>
        //private PartInfoModel _CurrentPart;

        //public PartInfoModel CurrentPart
        //{
        //    get => _CurrentPart;
        //    set => Set(ref _CurrentPart, value);
        //}


        /// <summary>
        /// Детали
        /// </summary>
        private ObservableCollection<PartInfoModel> _Parts = new();

        public ObservableCollection<PartInfoModel> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }
        /// <summary>
        /// Видимость списка
        /// </summary>
        public Visibility PartsVisibility => Parts.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        #region Команды

        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }
        private static void OnCloseApplicationCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }
        private static bool CanCloseApplicationCommandExecute(object p) => true;
        #endregion

        #region EditMachineCommand
        public ICommand EditMachineCommand { get; }
        private void OnEditMachineCommandExecuted(object p)
        {
            var machine = Machine;
            if (!WindowsUserDialogService.EditMachine(ref machine)) return; // (windowsUserDialogService.Edit(machine)) 
            Machine = machine;
            OnPropertyChanged(nameof(Machine));

        }
        private static bool CanEditMachineCommandExecute(object p) => true;
        #endregion

        #region EditOperatorsCommand
        public ICommand EditOperatorsCommand { get; }
        private void OnEditOperatorsCommandExecuted(object p)
        {
            var operators = Operators;
            WindowsUserDialogService windowsUserDialogService = new();
            if (!windowsUserDialogService.Edit(operators)) return;
            var tempOperators = operators.ToList();
                
            // костыль надо нормально сделать
            tempOperators.RemoveAll(x => string.IsNullOrWhiteSpace(x.DisplayName));
            operators = new ObservableCollection<Operator>();
            foreach (var op in tempOperators)
            {
                operators.Add(op);
            }
            Operators = operators;
                
            OnPropertyChanged(nameof(Operators));
            OnPropertyChanged(nameof(CurrentOperator));

        }
        private static bool CanEditOperatorsCommandExecute(object p) => true;
        #endregion

        #region StartShiftCommand
        public ICommand StartShiftCommand { get; }
        private void OnStartShiftCommandExecuted(object p)
        {
            ShiftStarted = true;
            OnPropertyChanged(nameof(EditShiftInfoIsEnabled));
            OnPropertyChanged(nameof(StartShiftButtonVisibility));
            OnPropertyChanged(nameof(EndShiftButtonVisibility));
        }
        private static bool CanStartShiftCommandExecute(object p) => true;
        #endregion

        #region StartDetail
        public ICommand StartDetailCommand { get; }
        private void OnStartDetailCommandExecuted(object p)
        {
            WindowsUserDialogService windowsUserDialogService = new();
            var barCode = string.Empty;
            if (!WindowsUserDialogService.GetBarCode(ref barCode)) return;
            var part = barCode.GetPartFromBarCode();
            if (windowsUserDialogService.Confirm("Наладка?", "Наладка"))
            {
                part.StartSetupTime = DateTime.Now;
                part.StartMachiningTime = DateTime.MinValue;
            }
            else
            {
                part.StartSetupTime = DateTime.Now;
                part.StartMachiningTime = part.StartSetupTime;
            }
            
            Parts.Add(part);
            OnPropertyChanged(nameof(CanAddPart));
            OnPropertyChanged(nameof(PartsVisibility));

        }
        private static bool CanStartDetailCommandExecute(object p) => true;
        #endregion

        #region EndSetup
        public ICommand EndSetupCommand { get; }
        private void OnEndSetupCommandExecuted(object p)
        {
            if (p is null) return;
            switch (WindowsUserDialogService.GetSetupResult())
            {
                case EndSetupResult.Success:
                    Parts[Parts.IndexOf((PartInfoModel)p)].StartMachiningTime = DateTime.Now;
                    break;
                case EndSetupResult.Stop:
                    Parts.Remove((PartInfoModel)p);
                    break;
            }
            OnPropertyChanged(nameof(CanAddPart));
        }
        private static bool CanEndSetupCommandExecute(object p) => true;
        #endregion

        #region EditDetail
        public ICommand EditDetailCommand { get; }
        private void OnEditDetailCommandExecuted(object p)
        {
            MessageBox.Show("Тут будет редактирование");

        }
        private static bool CanEditDetailCommandExecute(object p) => true;
        #endregion

        #region EndDetail
        public ICommand EndDetailCommand { get; }
        private void OnEndDetailCommandExecuted(object p)
        {
            if (p is null) return;
            var (result, partsFinished) = WindowsUserDialogService.GetFinishResult();
            PartInfoModel part;
            switch (result)
            {
                case (EndDetailResult.Finish):
                    part = Parts[Parts.IndexOf((PartInfoModel)p)];
                    part.PartsFinished = partsFinished > part.PartsCount ? part.PartsCount : partsFinished;
                    part.EndMachiningTime = DateTime.Now;
                    break;
                case (EndDetailResult.FinishAndNext):
                    part = Parts[Parts.IndexOf((PartInfoModel)p)];
                    part.PartsFinished = partsFinished > part.PartsCount ? part.PartsCount : partsFinished;
                    part.EndMachiningTime = DateTime.Now;
                    StartDetailCommand.Execute(true);
                    break;
            }
            OnPropertyChanged(nameof(CanAddPart));
        }
        private static bool CanEndDetailCommandExecute(object p) => true;
        #endregion

        #region EndShiftCommand
        public ICommand EndShiftCommand { get; }
        private void OnEndShiftCommandExecuted(object p)
        {
            ShiftStarted = false;
            OnPropertyChanged(nameof(EditShiftInfoIsEnabled));
            OnPropertyChanged(nameof(StartShiftButtonVisibility));
            OnPropertyChanged(nameof(EndShiftButtonVisibility));
        }
        private static bool CanEndShiftCommandExecute(object p) => true;
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            EditMachineCommand = new LambdaCommand(OnEditMachineCommandExecuted, CanEditMachineCommandExecute);
            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
            StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
            EndSetupCommand = new LambdaCommand(OnEndSetupCommandExecuted, CanEndSetupCommandExecute);
            EndDetailCommand = new LambdaCommand(OnEndDetailCommandExecuted, CanEndDetailCommandExecute);
            EditDetailCommand = new LambdaCommand(OnEditDetailCommandExecuted, CanEditDetailCommandExecute);
            EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);
        }
    }
}
