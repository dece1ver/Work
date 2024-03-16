using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Models;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Extensions;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using remeLog.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Part = remeLog.Models.Part;

namespace remeLog.ViewModels
{

    public class PartsInfoWindowViewModel : ViewModel
    {
        public PartsInfoWindowViewModel(CombinedParts parts)
        {
            ClearContentCommand = new LambdaCommand(OnClearContentCommandExecuted, CanClearContentCommandExecute);
            IncreaseDateCommand = new LambdaCommand(OnIncreaseDateCommandExecuted, CanIncreaseDateCommandExecute);
            DecreaseDateCommand = new LambdaCommand(OnDecreaseDateCommandExecuted, CanDecreaseDateCommandExecute);
            SetYesterdayDateCommand = new LambdaCommand(OnSetYesterdayDateCommandExecuted, CanSetYesterdayDateCommandExecute);
            SetWeekDateCommand = new LambdaCommand(OnSetWeekDateCommandExecuted, CanSetWeekDateCommandExecute);
            SetAllDateCommand = new LambdaCommand(OnSetAllDateCommandExecuted, CanSetAllDateCommandExecute);
            IncreaseSetupCommand = new LambdaCommand(OnIncreaseSetupCommandExecuted, CanIncreaseSetupCommandExecute);
            DecreaseSetupCommand = new LambdaCommand(OnDecreaseSetupCommandExecuted, CanDecreaseSetupCommandExecute);
            UpdatePartsCommand = new LambdaCommand(OnUpdatePartsCommandExecuted, CanUpdatePartsCommandExecute);
            RefreshPartsCommand = new LambdaCommand(OnRefreshPartsCommandExecuted, CanRefreshPartsCommandExecute);
            OpenDailyReportWindowCommand = new LambdaCommand(OnOpenDailyReportWindowCommandExecuted, CanOpenDailyReportWindowCommandExecute);

            PartsInfo = parts;
            ShiftFilterItems = new Shift[3] { new Shift(ShiftType.All), new Shift(ShiftType.Day), new Shift(ShiftType.Night) };
            _ShiftFilter = ShiftFilterItems.FirstOrDefault();
            _OperatorFilter = "";
            _Parts = PartsInfo.Parts;
            _OrderFilter = "";
            _PartNameFilter = "";
            _FromDate = PartsInfo.FromDate;
            _ToDate = PartsInfo.ToDate;
            _FilterMachines = new();
            _FilterMachines.ReadMachines();
            foreach (var machineFilter in FilterMachines)
            {
                if (machineFilter.Machine == parts.Machine) machineFilter.Filter = true;
            }
        }


        public Shift[] ShiftFilterItems { get; set; }

        private ObservableCollection<Part> _Parts;
        /// <summary> Описание </summary>
        public ObservableCollection<Part> Parts
        {
            get => _Parts;
            set
            {
                Set(ref _Parts, value);
                OnPropertyChanged(nameof(SetupTimeRatio));
                OnPropertyChanged(nameof(AverageSetupRatio));
                OnPropertyChanged(nameof(ProductionTimeRatio));
                OnPropertyChanged(nameof(AverageProductionRatio));
                OnPropertyChanged(nameof(SpecifiedDowntimesRatio));
                OnPropertyChanged(nameof(UnspecifiedDowntimesRatio));
            }
        }


        private DateTime _FromDate;
        /// <summary> Описание </summary>
        public DateTime FromDate
        {
            get => _FromDate;
            set
            {
                Set(ref _FromDate, value);
                _ = LoadPartsAsync();
            }
        }

        private DateTime _ToDate;
        /// <summary> Описание </summary>
        public DateTime ToDate
        {
            get => _ToDate;
            set
            {
                Set(ref _ToDate, value);
                _ = LoadPartsAsync();
            }
        }

        private Shift _ShiftFilter;
        /// <summary> Фильтр смены </summary>
        public Shift ShiftFilter
        {
            get => _ShiftFilter;
            set
            {
                Set(ref _ShiftFilter, value);
                _ = LoadPartsAsync();
            }
        }


        private string _OperatorFilter;
        /// <summary> Фильтр оператора </summary>
        public string OperatorFilter
        {
            get => _OperatorFilter;
            set
            {
                Set(ref _OperatorFilter, value);
                _ = LoadPartsAsync();
            }
        }

        private string _PartNameFilter;
        /// <summary> Фильтр названия детали </summary>
        public string PartNameFilter
        {
            get => _PartNameFilter;
            set
            {
                Set(ref _PartNameFilter, value);
                _ = LoadPartsAsync();
            }
        }

        private string _OrderFilter;
        /// <summary> Фильтр названия детали </summary>
        public string OrderFilter
        {
            get => _OrderFilter;
            set
            {
                Set(ref _OrderFilter, value);
                _ = LoadPartsAsync();
            }
        }


        private int? _SetupFilter;
        /// <summary> Описание </summary>
        public int? SetupFilter
        {
            get => _SetupFilter;
            set 
            {
                Set(ref _SetupFilter, value);
                _ = LoadPartsAsync();
            }
        }

        private DeepObservableCollection<MachineFilter> _FilterMachines;
        /// <summary> Список станков с необходимостью фильтрации </summary>
        public DeepObservableCollection<MachineFilter> FilterMachines
        {
            get => _FilterMachines;
            set
            {
                Set(ref _FilterMachines, value);
                _ = LoadPartsAsync();
            }
        }


        public CombinedParts PartsInfo { get; set; }


        private bool _InProgress;
        /// <summary> Загрузка информации </summary>
        public bool InProgress
        {
            get => _InProgress;
            set => Set(ref _InProgress, value);
        }

        private string _Status = string.Empty;
        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        private Overlay _Overlay = new(false);

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

        public double AverageSetupRatio => Parts.AverageSetupRatio();
        public double AverageProductionRatio => Parts.AverageProductionRatio();
        public double SetupTimeRatio => Parts.SetupRatio();
        public double ProductionTimeRatio => Parts.ProductionRatio();
        public double SpecifiedDowntimesRatio => Parts.SpecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter);
        public double UnspecifiedDowntimesRatio => Parts.UnspecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter);


        private TimeOnly _EndDateForCalc;
        /// <summary> Конечная дата для расчета </summary>
        public TimeOnly EndDateForCalc
        {
            get => _EndDateForCalc;
            set
            {
                if (Set(ref _EndDateForCalc, value))
                {
                    OnPropertyChanged(nameof(CalculatedTimeDifference));
                    OnPropertyChanged(nameof(CalculatedTimeDifferenceMinutes));
                }
            }
        }

        private TimeOnly _StartDateForCalc;
        /// <summary> Начальная дата для расчета </summary>
        public TimeOnly StartDateForCalc
        {
            get => _StartDateForCalc;
            set
            {
                if (Set(ref _StartDateForCalc, value))
                {
                    OnPropertyChanged(nameof(CalculatedTimeDifference));
                    OnPropertyChanged(nameof(CalculatedTimeDifferenceMinutes));
                }
            }
        }

        public TimeSpan CalculatedTimeDifference
        {
            get
            {
                var breakfasts = DescreaseTimes ? TimeOnlys.GetBreaksBetween(StartDateForCalc, EndDateForCalc) : TimeSpan.Zero;
                return EndDateForCalc - StartDateForCalc - breakfasts - new TimeSpan(0, AdditionalDescreaseValue ?? 0, 0);
            }
        }

        public double CalculatedTimeDifferenceMinutes
            => CalculatedTimeDifference.TotalMinutes;


        private bool _DescreaseTimes;
        /// <summary> Описание </summary>
        public bool DescreaseTimes
        {
            get => _DescreaseTimes;
            set
            {
                if (Set(ref _DescreaseTimes, value))
                {
                    OnPropertyChanged(nameof(CalculatedTimeDifference));
                    OnPropertyChanged(nameof(CalculatedTimeDifferenceMinutes));
                }
            }
        }


        private int? _AdditionalDescreaseValue = null;
        /// <summary> Описание </summary>
        public int? AdditionalDescreaseValue
        {
            get => _AdditionalDescreaseValue;
            set
            {
                if (Set(ref _AdditionalDescreaseValue, value))
                {
                    OnPropertyChanged(nameof(CalculatedTimeDifference));
                    OnPropertyChanged(nameof(CalculatedTimeDifferenceMinutes));
                }
            }
        }

        #region IncreaseDateCommand
        public ICommand IncreaseDateCommand { get; }
        private void OnIncreaseDateCommandExecuted(object p)
        {

            FromDate = FromDate.AddDays(1);
            ToDate = ToDate.AddDays(1);
        }
        private bool CanIncreaseDateCommandExecute(object p) => true;
        #endregion

        #region DecreaseDateCommand
        public ICommand DecreaseDateCommand { get; }
        private void OnDecreaseDateCommandExecuted(object p)
        {

            FromDate = FromDate.AddDays(-1);
            ToDate = ToDate.AddDays(-1);
        }
        private bool CanDecreaseDateCommandExecute(object p) => true;
        #endregion

        #region SetYesterdayDateCommand
        public ICommand SetYesterdayDateCommand { get; }
        private void OnSetYesterdayDateCommandExecuted(object p)
        {

            FromDate = DateTime.Today.AddDays(-1);
            ToDate = FromDate;
        }
        private bool CanSetYesterdayDateCommandExecute(object p) => true;
        #endregion

        #region SetWeekDateCommand
        public ICommand SetWeekDateCommand { get; }
        private void OnSetWeekDateCommandExecuted(object p)
        {
            FromDate = ToDate.AddDays(-7);
        }
        private bool CanSetWeekDateCommandExecute(object p) => true;
        #endregion

        #region SetAllDateCommand
        public ICommand SetAllDateCommand { get; }
        private void OnSetAllDateCommandExecuted(object p)
        {
            FromDate = new DateTime(2023, 01, 01, 00, 00, 00);
            ToDate = DateTime.Today;
        }
        private bool CanSetAllDateCommandExecute(object p) => true;
        #endregion

        #region IncreaseSetupCommand
        public ICommand IncreaseSetupCommand { get; }
        private void OnIncreaseSetupCommandExecuted(object p)
        {
            if (SetupFilter is null)
            {
                SetupFilter = 1;
            }
            else if (SetupFilter is int i && i < int.MaxValue)
            {
                SetupFilter++;
            }
        }
        private bool CanIncreaseSetupCommandExecute(object p) => true;
        #endregion

        #region DecreaseSetupCommand
        public ICommand DecreaseSetupCommand { get; }
        private void OnDecreaseSetupCommandExecuted(object p)
        {
            if (SetupFilter is int i && i > 0)
            {
                if (SetupFilter == 1)
                {
                    SetupFilter = null;
                }
                else 
                {
                    SetupFilter--;
                }
            }
        }
        private bool CanDecreaseSetupCommandExecute(object p) => true;
        #endregion

        #region ClearContent
        public ICommand ClearContentCommand { get; }
        private void OnClearContentCommandExecuted(object p)
        {
            if (p is TextBox textBox)
            {
                textBox.Text = "";
            }
        }
        private static bool CanClearContentCommandExecute(object p) => true;
        #endregion

        #region UpdateParts
        public ICommand UpdatePartsCommand { get; }
        private void OnUpdatePartsCommandExecuted(object p)
        {
            if (MessageBox.Show("Обновить информацию?", "Вы точно уверены?", MessageBoxButton.YesNo, MessageBoxImage.Question) is MessageBoxResult.No) return;
            foreach (var part in Parts.Where(p => p.NeedUpdate))
            {
                switch (part.UpdatePart())
                {
                    case DbResult.Ok:
                        part.NeedUpdate = false;
                        break;
                    case DbResult.AuthError:
                        MessageBox.Show("Ошибка авторизации");
                        break;
                    case DbResult.Error:
                        MessageBox.Show("Ошибка");
                        break;
                }
            }
            _ = LoadPartsAsync();
        }
        private static bool CanUpdatePartsCommandExecute(object p) => true;
        #endregion

        #region RefreshParts
        public ICommand RefreshPartsCommand { get; }
        private void OnRefreshPartsCommandExecuted(object p) => _ = LoadPartsAsync();
        private static bool CanRefreshPartsCommandExecute(object p) => true;
        #endregion

        #region OpenDailyReportWindow
        public ICommand OpenDailyReportWindowCommand { get; }
        private void OnOpenDailyReportWindowCommandExecuted(object p)
        {
            if (FromDate != ToDate)
            {
                MessageBox.Show("Для составления суточного отчета должны быть выбраны одинаковые даты!", "Разные даты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // TODO: сделать овнером текущее окно, которому принадлежит этот датаконтект 
            using (Overlay = new())
            {
                var dailyInfoWindow = new DailyReportWindow((Parts, ToDate, PartsInfo.Machine)) { Owner = Application.Current.MainWindow };
                dailyInfoWindow.ShowDialog();
            }
        }
        private static bool CanOpenDailyReportWindowCommandExecute(object p) => true;
        #endregion

        private async Task LoadPartsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    InProgress = true;
                    Parts = Database.ReadPartsWithConditions($"Machine = '{PartsInfo.Machine}' AND ShiftDate BETWEEN '{FromDate}' AND '{ToDate}' " +
                    $"{(ShiftFilter is { Type: ShiftType.All } ? "" : $"AND Shift = '{ShiftFilter.FilterText}'")}" +
                    $"{(string.IsNullOrEmpty(OperatorFilter) ? "" : $"AND Operator LIKE '%{OperatorFilter}%'")}" +
                    $"{(string.IsNullOrEmpty(PartNameFilter) ? "" : $"AND PartName LIKE '%{PartNameFilter}%'")}" +
                    $"{(string.IsNullOrEmpty(OrderFilter) ? "" : $"AND [Order] LIKE '%{OrderFilter}%'")}" +
                    $"{(SetupFilter == null ? "" : $"AND Setup = {SetupFilter}")}" +
                    $"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}");
                }
                finally
                {
                    InProgress = false;
                }
            });
        }
    }
}
