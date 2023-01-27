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
            set
            {
                Set(ref _Progress, value);
                OnPropertyChanged(nameof(ProgressBarVisibility));
            }
        }

        private double _ProgressMaxValue;
        /// <summary>
        /// Максимальное значение прогрессбара
        /// </summary>
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
        public bool CanAddPart => Parts.Count == 0 || Parts.Count == Parts.Count(x => x.IsFinished);

        public bool Overlay { get; set; }

        /// <summary>
        /// Детали
        /// </summary>
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

        #region EditSettingsCommand
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            OverlayOn();
            var settings = new AppSettingsModel(Machine, AppSettings.XlPath, Operators);
            WindowsUserDialogService windowsUserDialogService = new();

            if (windowsUserDialogService.Edit(settings))
            {
                AppSettings.Machine = settings.Machine;
                Machine = AppSettings.Machine;
                AppSettings.XlPath = settings.XlPath;
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
            WindowsUserDialogService windowsUserDialogService = new();
            var barCode = string.Empty;
            if (WindowsUserDialogService.GetBarCode(ref barCode))
            {
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
            }
            OnPropertyChanged(nameof(CanAddPart));
            OverlayOff();
        }
        private static bool CanStartDetailCommandExecute(object p) => true;
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
            OnPropertyChanged(nameof(CanAddPart));
            OverlayOff();
        }
        private static bool CanEndSetupCommandExecute(object p) => true;
        #endregion

        #region EditDetail
        public ICommand EditDetailCommand { get; }
        private void OnEditDetailCommandExecuted(object p)
        {
            OverlayOn();
            MessageBox.Show("Тут будет редактирование");
            OverlayOff();
        }
        private static bool CanEditDetailCommandExecute(object p) => true;
        #endregion

        #region EndDetail
        public ICommand EndDetailCommand { get; }
        private void OnEndDetailCommandExecuted(object p)
        {
            OverlayOn();
            var (result, partsFinished, machineTime) = WindowsUserDialogService.GetFinishResult();
            PartInfoModel part;
            switch (result)
            {
                case (EndDetailResult.Finish or EndDetailResult.FinishAndNext):
                    part = Parts[Parts.IndexOf((PartInfoModel)p)];
                    part.PartsFinished = partsFinished > part.PartsCount ? part.PartsCount : partsFinished;
                    part.MachineTime = machineTime;
                    part.EndMachiningTime = DateTime.Now;
                    part.WriteToXl();
                    if (result is EndDetailResult.FinishAndNext) StartDetailCommand.Execute(true);
                    break;
            }
            OnPropertyChanged(nameof(CanAddPart));
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
            EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
            EditOperatorsCommand = new LambdaCommand(OnEditOperatorsCommandExecuted, CanEditOperatorsCommandExecute);
            StartDetailCommand = new LambdaCommand(OnStartDetailCommandExecuted, CanStartDetailCommandExecute);
            EndSetupCommand = new LambdaCommand(OnEndSetupCommandExecuted, CanEndSetupCommandExecute);
            EndDetailCommand = new LambdaCommand(OnEndDetailCommandExecuted, CanEndDetailCommandExecute);
            EditDetailCommand = new LambdaCommand(OnEditDetailCommandExecuted, CanEditDetailCommandExecute);
        }
    }
}
