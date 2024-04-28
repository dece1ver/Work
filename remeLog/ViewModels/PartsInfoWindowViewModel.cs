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
using System.Collections.Specialized;
using System.ComponentModel;
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
        private readonly object lockObject = new();
        public PartsInfoWindowViewModel(CombinedParts parts)
        {
            ClearContentCommand = new LambdaCommand(OnClearContentCommandExecuted, CanClearContentCommandExecute);
            IncreaseDateCommand = new LambdaCommand(OnIncreaseDateCommandExecuted, CanIncreaseDateCommandExecute);
            DecreaseDateCommand = new LambdaCommand(OnDecreaseDateCommandExecuted, CanDecreaseDateCommandExecute);
            SetYesterdayDateCommand = new LambdaCommand(OnSetYesterdayDateCommandExecuted, CanSetYesterdayDateCommandExecute);
            SetWeekDateCommand = new LambdaCommand(OnSetWeekDateCommandExecuted, CanSetWeekDateCommandExecute);
            SetMonthDateCommand = new LambdaCommand(OnSetMonthDateCommandExecuted, CanSetMonthDateCommandExecute);
            SetYearDateCommand = new LambdaCommand(OnSetYearDateCommandExecuted, CanSetYearDateCommandExecute);
            SetAllDateCommand = new LambdaCommand(OnSetAllDateCommandExecuted, CanSetAllDateCommandExecute);
            IncreaseSetupCommand = new LambdaCommand(OnIncreaseSetupCommandExecuted, CanIncreaseSetupCommandExecute);
            DecreaseSetupCommand = new LambdaCommand(OnDecreaseSetupCommandExecuted, CanDecreaseSetupCommandExecute);
            UpdatePartsCommand = new LambdaCommand(OnUpdatePartsCommandExecuted, CanUpdatePartsCommandExecute);
            RefreshPartsCommand = new LambdaCommand(OnRefreshPartsCommandExecuted, CanRefreshPartsCommandExecute);
            ChangeCompactViewCommand = new LambdaCommand(OnChangeCompactViewCommandExecuted, CanChangeCompactViewCommandExecute);
            ChangeCalcFixedCommand = new LambdaCommand(OnChangeCalcFixedCommandExecuted, CanChangeCalcFixedCommandExecute);
            OpenDailyReportWindowCommand = new LambdaCommand(OnOpenDailyReportWindowCommandExecuted, CanOpenDailyReportWindowCommandExecute);
            ShowInfoCommand = new LambdaCommand(OnShowInfoCommandExecuted, CanShowInfoCommandExecute);
            ExportToExcelCommand = new LambdaCommand(OnExportToExcelCommandExecuted, CanExportToExcelCommandExecute);
            DeleteFilterCommand = new LambdaCommand(OnDeleteFilterCommandExecuted, CanDeleteFilterCommandExecute);
            DeletePartCommand = new LambdaCommand(OnDeletePartCommandExecuted, CanDeletePartCommandExecute);

            PartsInfo = parts;
            ShiftFilterItems = new Shift[3] { new Shift(ShiftType.All), new Shift(ShiftType.Day), new Shift(ShiftType.Night) };
            _ShiftFilter = ShiftFilterItems.FirstOrDefault();
            _OperatorFilter = "";
            _Parts = PartsInfo.Parts;
            _OrderFilter = "";
            _PartNameFilter = "";
            _FromDate = PartsInfo.FromDate;
            _ToDate = PartsInfo.ToDate;
            _MachineFilters = new();
            _MachineFilters.CollectionChanged += MachineFiltersSource_CollectionChanged!;

            _MachineFilters.ReadMachines();
            foreach (var machineFilter in MachineFilters)
            {
                if (machineFilter.Machine == parts.Machine) machineFilter.Filter = true;
            }
        }

        private void MachineFiltersSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (MachineFilter item in e.NewItems)
                    item.PropertyChanged += MachineFilters_PropertyChanged!;

            if (e.OldItems != null)
                foreach (MachineFilter item in e.OldItems)
                    item.PropertyChanged -= MachineFilters_PropertyChanged!;
        }

        private void MachineFilters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Filter")
                _ = LoadPartsAsync();
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
                if (!CanBeChanged()) return;
                if (Set(ref _FromDate, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private DateTime _ToDate;
        /// <summary> Описание </summary>
        public DateTime ToDate
        {
            get => _ToDate;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _ToDate, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private Shift _ShiftFilter;
        /// <summary> Фильтр смены </summary>
        public Shift ShiftFilter
        {
            get => _ShiftFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _ShiftFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }


        private string _OperatorFilter;
        /// <summary> Фильтр оператора </summary>
        public string OperatorFilter
        {
            get => _OperatorFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _OperatorFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private string _PartNameFilter;
        /// <summary> Фильтр названия детали </summary>
        public string PartNameFilter
        {
            get => _PartNameFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _PartNameFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private string _OrderFilter;
        /// <summary> Фильтр названия детали </summary>
        public string OrderFilter
        {
            get => _OrderFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _OrderFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }


        private int? _SetupFilter;
        /// <summary> Описание </summary>
        public int? SetupFilter
        {
            get => _SetupFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _SetupFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }


        private int? _FinishedCountFilter;
        /// <summary> Описание </summary>
        public int? FinishedCountFilter
        {
            get => _FinishedCountFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _FinishedCountFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }


        private ObservableCollection<MachineFilter> _MachineFilters;
        /// <summary> Список станков с необходимостью фильтрации </summary>
        public ObservableCollection<MachineFilter> MachineFilters
        {
            get => _MachineFilters;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _MachineFilters, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private Part? _SelectedPart;
        /// <summary> Описание </summary>
        public Part? SelectedPart
        {
            get => _SelectedPart;
            set => Set(ref _SelectedPart, value);
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


        private bool _CompactView = true;
        /// <summary> Описание </summary>
        public bool CompactView
        {
            get => _CompactView;
            set => Set(ref _CompactView, value);
        }


        private Overlay _Overlay = new(false);

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

        public List<string> SetupReasons => AppSettings.Instance.SetupReasons;
        public List<string> MachiningReasons => AppSettings.Instance.MachiningReasons;

        public double AverageSetupRatio =>
            MachineFilters.Count(f => f.Filter) > 0
            ? Parts.AverageSetupRatio()
            : double.NaN;
        public double AverageProductionRatio =>
            MachineFilters.Count(f => f.Filter) > 0
            ? Parts.AverageProductionRatio()
            : double.NaN;
        public double SetupTimeRatio =>
            MachineFilters.Count(f => f.Filter) > 0
            ? Parts.SetupRatio()
            : double.NaN;
        public double ProductionTimeRatio =>
            MachineFilters.Count(f => f.Filter) > 0 
            ? Parts.ProductionRatio()
            : double.NaN;
        public double SpecifiedDowntimesRatio =>
            MachineFilters.Count(f => f.Filter) > 0
            ? Parts.SpecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter)
            : double.NaN;
        public double UnspecifiedDowntimesRatio =>
            MachineFilters.Count(f => f.Filter) > 0
            ? Parts.UnspecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter)
            : double.NaN;


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

        #region SetMonthDateCommand
        public ICommand SetMonthDateCommand { get; }
        private void OnSetMonthDateCommandExecuted(object p)
        {
            if (FromDate == ToDate.AddDays(-30))
            {
                FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                ToDate = DateTime.Today.AddDays(-1);
            }
            else
            {
                FromDate = ToDate.AddDays(-30);
            }
        }
        private bool CanSetMonthDateCommandExecute(object p) => true;
        #endregion

        #region SetYearDateCommand
        public ICommand SetYearDateCommand { get; }
        private void OnSetYearDateCommandExecuted(object p)
        {
            FromDate = new DateTime(2024, 01, 01);
            ToDate = DateTime.Today;
        }
        private bool CanSetYearDateCommandExecute(object p) => true;
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

        #region ShowInfo
        public ICommand ShowInfoCommand { get; }
        private void OnShowInfoCommandExecuted(object p)
        {
            MessageBox.Show("Тут будет информация по выборке","Заглушка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private static bool CanShowInfoCommandExecute(object p) => true;
        #endregion

        #region ExportToExcel
        public ICommand ExportToExcelCommand { get; }
        private void OnExportToExcelCommandExecuted(object p)
        {
            try
            {
                Status = Xl.ExportDataset(Parts, FromDate, ToDate);;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private static bool CanExportToExcelCommandExecute(object p) => true;
        #endregion

        #region DeleteFilter
        public ICommand DeleteFilterCommand { get; }
        private void OnDeleteFilterCommandExecuted(object p)
        {
            ShiftFilter = new(ShiftType.All);
            OperatorFilter = string.Empty;
            PartNameFilter = string.Empty;
            OrderFilter = string.Empty;
            SetupFilter = null;
            FinishedCountFilter = null;
            ToDate = PartsInfo.ToDate;
            FromDate = PartsInfo.FromDate;
        }
        private static bool CanDeleteFilterCommandExecute(object p) => true;
        #endregion

        #region DeletePart
        public ICommand DeletePartCommand { get; }
        private void OnDeletePartCommandExecuted(object p)
        {
            if (p is Part part && part == SelectedPart 
                && MessageBox.Show($"Удалить деталь: {part.PartName}?\nДанное действие невозможно отменить.", "Удаление информации", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes) 
            {
                switch (Database.DeletePart(part))
                {
                    case DbResult.Ok:
                        _ = LoadPartsAsync();
                        Status = $"Деталь {SelectedPart.PartName} удалена из БД";
                        break;
                    case DbResult.AuthError:
                        Status = "Ошибка авторизации";
                        break;
                    case DbResult.Error:
                        Status = "Ошибка";
                        break;
                    case DbResult.NoConnection:
                        Status = "Нет соединения с БД";
                        break;
                }
            };
        }
        private static bool CanDeletePartCommandExecute(object p) => true;
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

        #region ChangeCompactView
        public ICommand ChangeCompactViewCommand { get; }
        private void OnChangeCompactViewCommandExecuted(object p) => CompactView = !CompactView;
        private static bool CanChangeCompactViewCommandExecute(object p) => true;
        #endregion

        #region ChangeCalcFixed
        public ICommand ChangeCalcFixedCommand { get; }
        private static void OnChangeCalcFixedCommandExecuted(object p)
        {
            Part.CalcFixed = !Part.CalcFixed;
        }
        private static bool CanChangeCalcFixedCommandExecute(object p) => true;
        #endregion

        #region OpenDailyReportWindow
        public ICommand OpenDailyReportWindowCommand { get; }
        private void OnOpenDailyReportWindowCommandExecuted(object p)
        {
            if (FromDate != ToDate)
            {
                MessageBox.Show("Для составления суточного отчета должны быть выбраны одинаковые даты.", "Разные даты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MachineFilters.Count(f => f.Filter) > 1)
            {
                MessageBox.Show("Для составления суточного отчета в фильтре должен быть только один станок.", "Выбрано слишком много станков.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (!MachineFilters.Any(f => f.Filter))
            {
                MessageBox.Show("Для составления суточного отчета в фильтре должен быть как минимум один станок.", "Выбрано слишком мало станков.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (ShiftFilter.Type != ShiftType.All)
            {
                MessageBox.Show("Для составления суточного отчета не должно быть фильтра по смене.", "Лишние фильтры.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(OperatorFilter))
            {
                MessageBox.Show("Для составления суточного отчета не должно быть фильтра по оператору.", "Лишние фильтры.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(PartNameFilter))
            {
                MessageBox.Show("Для составления суточного отчета не должно быть фильтра по детали.", "Лишние фильтры.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(OrderFilter))
            {
                MessageBox.Show("Для составления суточного отчета не должно быть фильтра по маршрутному листу.", "Лишние фильтры.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SetupFilter != null)
            {
                MessageBox.Show("Для составления суточного отчета не должно быть фильтра по установке.", "Лишние фильтры.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Parts.Any(x => !string.IsNullOrEmpty(x.Error))
                && MessageBox.Show("Не всё заполнено корректно, все равно создать отчёт за смену?", "Подтверждение.",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;

            using (Overlay = new())
            {
                var dailyInfoWindow = new DailyReportWindow((Parts, ToDate, PartsInfo.Machine)) { Owner = p as PartsInfoWindow };
                dailyInfoWindow.ShowDialog();
            }
        }
        private static bool CanOpenDailyReportWindowCommandExecute(object p) => true;
        #endregion

        private async Task<bool> LoadPartsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (lockObject)
                    {
                        InProgress = true;
                        var conditions = $"ShiftDate BETWEEN '{FromDate}' AND '{ToDate}' " +
                            $"{(ShiftFilter is { Type: ShiftType.All } ? "" : $"AND Shift = '{ShiftFilter.FilterText}'")}" +
                            $"{(string.IsNullOrEmpty(OperatorFilter) ? "" : $"AND Operator LIKE '%{OperatorFilter}%'")}" +
                            $"{(string.IsNullOrEmpty(PartNameFilter) ? "" : $"AND PartName LIKE '%{PartNameFilter}%'")}" +
                            $"{(string.IsNullOrEmpty(OrderFilter) ? "" : $"AND [Order] LIKE '%{OrderFilter}%'")}" +
                            $"{(FinishedCountFilter == null ? "" : $"AND FinishedCount >= {FinishedCountFilter}")}" +
                            $"{(SetupFilter == null ? "" : $"AND Setup = {SetupFilter}")}";

                        foreach (var machineFilter in MachineFilters)
                        {
                            if (!machineFilter.Filter) conditions += $"AND NOT Machine = '{machineFilter.Machine}'";
                        }

                        Parts = Database.ReadPartsWithConditions(conditions);

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}");
                    return false;
                }
                finally
                {
                    InProgress = false;
                }
            });
        }

        /// <summary>
        /// Подтверждение изменения свойства, которое повлечет изменение списка.
        /// Сразу возвращает true, если нет незаписанных позиций, либо спрашивает подтверждения.
        /// </summary>
        /// <returns></returns>
        private bool CanBeChanged()
        {
            if (Parts.Any(x => x.NeedUpdate)
                && MessageBox.Show("Есть незаписанные изменения. При продолжении они будут утеряны.", "Обновить список деталей?",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return false;
            return true;
        }
    }
}
