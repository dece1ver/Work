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


        private DateTime _CurrentPartSetupStartTime;
        /// <summary>
        /// Начало текущей наладки
        /// </summary>
        public DateTime CurrentPartSetupStartTime
        {
            get => _CurrentPartSetupStartTime;
            set => Set(ref _CurrentPartSetupStartTime, value);
        }

        private DateTime _CurrentPartSetupEndTime;
        /// <summary>
        /// Конец текущей наладки
        /// </summary>
        public DateTime CurrentPartSetupEndTime
        {
            get => _CurrentPartSetupEndTime;
            set => Set(ref _CurrentPartSetupEndTime, value);
        }

        private DateTime _currentPartMachiningStartTime;
        /// <summary>
        /// Начало текущего изготовления
        /// </summary>
        public DateTime CurrentPartMachiningStartTime
        {
            get => _currentPartMachiningStartTime;
            set => Set(ref _currentPartMachiningStartTime, value);
        }

        private DateTime _currentPartMachiningEndTime;
        /// <summary>
        /// Конец текущего изготовления
        /// </summary>
        public DateTime CurrentPartMachiningEndTime
        {
            get => _currentPartMachiningEndTime;
            set => Set(ref _currentPartMachiningEndTime, value);
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

        #region EditOperatorsCommand
        public ICommand EditOperatorsCommand { get; }
        private void OnEditOperatorsCommandExecuted(object p)
        {
            var operators = Operators;
            WindowsUserDialogService windowsUserDialogService = new WindowsUserDialogService();
            if (windowsUserDialogService.EditOperators(ref operators) == true)
            {
                List<Operator> tempOperators = operators.ToList();
                
                // костыль надо нормально сделать
                tempOperators.RemoveAll(x => string.IsNullOrEmpty(x.DisplayName));
                operators = new();
                foreach (var op in tempOperators)
                {
                    operators.Add(op);
                }
                Operators = operators;
                
                OnPropertyChanged(nameof(Operators));
                OnPropertyChanged(nameof(CurrentOperator));
            }
            
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
            if (!windowsUserDialogService.GetBarCode(ref barCode)) return;
            var part = barCode.GetPartFromBarCode();
            if (windowsUserDialogService.Confirm("Наладка?", "Наладка"))
            {
                part.StartSetupTime = DateTime.Now;
                part.EndSetupTime = part.StartSetupTime;
                part.StartMachiningTime = part.EndSetupTime;
            }
            else
            {
                part.StartSetupTime = DateTime.Now;
            }
            
            Parts.Add(part);
            OnPropertyChanged(nameof(Parts));
            OnPropertyChanged(nameof(PartsVisibility));

        }
        private static bool CanStartDetailCommandExecute(object p) => true;
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
            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
            StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
            EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);
        }
    }
}
