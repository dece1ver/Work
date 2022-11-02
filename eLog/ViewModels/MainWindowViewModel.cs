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

        private bool _ShiftStarted = false;
        public bool ShiftStarted
        {
            get => _ShiftStarted;
            set => Set(ref _ShiftStarted, value);
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

        public bool EditshiftInfoIsEnabled { get => !ShiftStarted; } 

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


        private string _TargetPath;

        public string TargetPath
        {
            get => _TargetPath;
            set => Set(ref _TargetPath, value);
        }

        /// <summary>
        /// Список файлов
        /// </summary>
        private List<string> _Files;

        public List<string> Files
        {
            get => _Files;
            set => Set(ref _Files, value);
        }

        public int? FilesCount => Files?.Count;

        private double _NcFiles;
        /// <summary>
        /// УП
        /// </summary>
        public double NcFiles
        {
            get => _NcFiles;
            set => Set(ref _NcFiles, value);
        }

        /// <summary>
        /// Отчет
        /// </summary>
        private string _Report;

        public string Report
        {
            get => _Report;
            set => Set(ref _Report, value);
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
            if (windowsUserDialogService.EditOperators(operators) == true)
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
            OnPropertyChanged(nameof(EditshiftInfoIsEnabled));
            OnPropertyChanged(nameof(StartShiftButtonVisibility));
            OnPropertyChanged(nameof(EndShiftButtonVisibility));
        }
        private static bool CanStartShiftCommandExecute(object p) => true;
        #endregion


        #region EndShiftCommand
        public ICommand EndShiftCommand { get; }
        private void OnEndShiftCommandExecuted(object p)
        {
            ShiftStarted = false;
            OnPropertyChanged(nameof(EditshiftInfoIsEnabled));
            OnPropertyChanged(nameof(StartShiftButtonVisibility));
            OnPropertyChanged(nameof(EndShiftButtonVisibility));
        }
        private static bool CanEndShiftCommandExecute(object p) => true;
        #endregion


        #region SaveReportCommand
        public ICommand SaveReportCommand { get; }

        private void OnSaveReportCommandExecuted(object p)
        {
            if (string.IsNullOrEmpty(Report)) return;
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            saveFileDialog.FileName = "Анализы " + Path.GetFileName(TargetPath) + ".txt";
            if (saveFileDialog.ShowDialog() != true) return;
            File.WriteAllText(saveFileDialog.FileName, Report);
            Status = $"Отчет записан в файл \"{saveFileDialog.FileName}\"";
        }
        private static bool CanSaveReportCommandExecute(object p) => true;
        #endregion

        #endregion




        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            StartShiftCommand = new LambdaCommand(OnStartShiftCommandExecuted, CanStartShiftCommandExecute);
            EndShiftCommand = new LambdaCommand(OnEndShiftCommandExecuted, CanEndShiftCommandExecute);
            SaveReportCommand = new LambdaCommand(OnSaveReportCommandExecuted, CanSaveReportCommandExecute);
        }
    }
}
