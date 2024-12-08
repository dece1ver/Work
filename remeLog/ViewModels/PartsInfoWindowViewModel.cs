using DocumentFormat.OpenXml.Bibliography;
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
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Part = remeLog.Models.Part;

namespace remeLog.ViewModels
{

    public class PartsInfoWindowViewModel : ViewModel, IDataErrorInfo
    {
        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private CancellationTokenSource _cancellationTokenSource = new();
        private static bool lockUpdate;
        private static readonly string[] supportedComparisonOperators = { "<=", "<", ">=", "=", ">", "!=" };

        public PartsInfoWindowViewModel(CombinedParts parts)
        {
            lockUpdate = true;
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
            ChangeRoleCommand = new LambdaCommand(OnChangeRoleCommandExecuted, CanChangeRoleCommandExecute);
            ChangeShowUncheckedCommand = new LambdaCommand(OnChangeShowUncheckedCommandExecuted, CanChangeShowUncheckedCommandExecute);
            ChangeCalcFixedCommand = new LambdaCommand(OnChangeCalcFixedCommandExecuted, CanChangeCalcFixedCommandExecute);
            OpenDailyReportWindowCommand = new LambdaCommand(OnOpenDailyReportWindowCommandExecuted, CanOpenDailyReportWindowCommandExecute);
            ShowInfoCommand = new LambdaCommand(OnShowInfoCommandExecuted, CanShowInfoCommandExecute);
            ShowArchiveTableCommand = new LambdaCommand(OnShowArchiveTableCommandExecuted, CanShowArchiveTableCommandExecute);
            ExportShiftsInfoReportCommand = new LambdaCommand(OnExportShiftsInfoReportCommandExecuted, CanExportShiftsInfoReportCommandExecute);
            ExportVerevkinReportCommand = new LambdaCommand(OnExportVerevkinReportCommandExecuted, CanExportVerevkinReportCommandExecute);
            ExportToExcelCommand = new LambdaCommand(OnExportToExcelCommandExecuted, CanExportToExcelCommandExecute);
            ExportToExcelCommand = new LambdaCommand(OnExportToExcelCommandExecuted, CanExportToExcelCommandExecute);
            OperatorReportToExcelCommand = new LambdaCommand(OnOperatorReportToExcelCommandExecuted, CanOperatorReportToExcelCommandExecute);
            OperatorsShiftsReportToExcelCommand = new LambdaCommand(OnOperatorsShiftsReportToExcelCommandExecuted, CanOperatorsShiftsReportToExcelCommandExecute);
            ExportPartsReportToExcelCommand = new LambdaCommand(OnExportPartsReportToExcelCommandExecuted, CanExportPartsReportToExcelCommandExecute);
            ExportLongSetupsCommand = new LambdaCommand(OnExportLongSetupsCommandExecuted, CanExportLongSetupsCommandExecute);
            ExportHistoryToExcelCommand = new LambdaCommand(OnExportHistoryToExcelCommandExecuted, CanExportHistoryToExcelCommandExecute);
            DeleteFilterCommand = new LambdaCommand(OnDeleteFilterCommandExecuted, CanDeleteFilterCommandExecute);
            ShowAllMachinesCommand = new LambdaCommand(OnShowAllMachinesCommandExecuted, CanShowAllMachinesCommandExecute);
            HideAllMachinesCommand = new LambdaCommand(OnHideAllMachinesCommandExecuted, CanHideAllMachinesCommandExecute);
            InvertMachinesCommand = new LambdaCommand(OnInvertMachinesCommandExecuted, CanInvertMachinesCommandExecute);
            SetLatheMachinesCommand = new LambdaCommand(OnSetLatheMachinesCommandExecuted, CanSetLatheMachinesCommandExecute);
            SetMillMachinesCommand = new LambdaCommand(OnSetMillMachinesCommandExecuted, CanSetMillMachinesCommandExecute);
            SetHorMillMachinesCommand = new LambdaCommand(OnSetHorMillMachinesCommandExecuted, CanSetHorMillMachinesCommandExecute);
            SetVerMillMachinesCommand = new LambdaCommand(OnSetVerMillMachinesCommandExecuted, CanSetVerMillMachinesCommandExecute);
            DeletePartCommand = new LambdaCommand(OnDeletePartCommandExecuted, CanDeletePartCommandExecute);

            CalcFixed = Part.CalcFixed;
            PartsInfo = parts;
            ShiftFilterItems = new Shift[3] { new(ShiftType.All), new(ShiftType.Day), new(ShiftType.Night) };
            _ShiftFilter = ShiftFilterItems.FirstOrDefault();
            _OperatorFilter = "";
            _FinishedCountFilter = "";
            _Parts = PartsInfo.Parts;
            _OrderFilter = "";
            _PartNameFilter = "";
            _EngineerCommentFilter = "";
            _TotalCountFilter = "";
            _FromDate = PartsInfo.FromDate;
            _ToDate = PartsInfo.ToDate;
            _MachineFilters = new();
            _MachineFilters.CollectionChanged += MachineFiltersSource_CollectionChanged!;
            _Parts.CollectionChanged += _Parts_CollectionChanged;
            _ViewMode = AppSettings.Instance.User ??= User.Viewer;
            foreach (Part part in _Parts)
            {
                PropertyChanged += Part_PropertyChanged!;
            }
            UpdateHasErrors();
            OnPropertyChanged(nameof(HasErrors));
            _ = Init();
            
        }

        private void _Parts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Part item in e.NewItems)
                    item.PropertyChanged += Part_PropertyChanged!;

            if (e.OldItems != null)
                foreach (Part item in e.OldItems)
                    item.PropertyChanged -= Part_PropertyChanged!;
        }

        async Task Init()
        {
            await _MachineFilters.ReadMachines();

            foreach (var machineFilter in MachineFilters)
            {
                machineFilter.Filter = machineFilter.Machine == PartsInfo.Machine;
            }

            lockUpdate = false;
        }

        private void Part_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateHasErrors();
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

        private async void MachineFilters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MachineVisibility));
            if (e.PropertyName == "Filter")
                await LoadPartsAsync();
        }


        private bool _HasErrors;
        /// <summary> Есть ли ошибки валидации </summary>
        public bool HasErrors
        {
            get => _HasErrors;
            set 
            {
                if (Set(ref _HasErrors, value))
                {
                    OnPropertyChanged(nameof(HasErrors));
                }
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
        /// <summary> Начальная дата </summary>
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
        /// <summary> Конечная дата </summary>
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
        #region Фильтры

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


        private string _FinishedCountFilter;
        /// <summary> фильтр по завершенным деталям </summary>
        public string FinishedCountFilter
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


        private string _TotalCountFilter;
        /// <summary> фильтр по количеству деталей в маршрутном листе </summary>
        public string TotalCountFilter
        {
            get => _TotalCountFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _TotalCountFilter, value))
                {
                    _ = LoadPartsAsync();
                }
            }
        }


        private string _EngineerCommentFilter;
        /// <summary> Фильтр по комментарию техотдела </summary>
        public string EngineerCommentFilter
        {
            get => _EngineerCommentFilter;
            set
            {
                if (!CanBeChanged()) return;
                if (Set(ref _EngineerCommentFilter, value))
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
        #endregion

        private Part? _SelectedPart;
        /// <summary> Выбранная деталь </summary>
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
        /// <summary> Компактный режим </summary>
        public bool CompactView
        {
            get => _CompactView;
            set => Set(ref _CompactView, value);
        }


        private User _ViewMode;
        /// <summary> Режим просмотра </summary>
        public User ViewMode
        {
            get => _ViewMode;
            set => Set(ref _ViewMode, value);
        }


        private bool _ViewUnchecked;
        /// <summary> Описание </summary>
        public bool ViewUnchecked
        {
            get => _ViewUnchecked;
            set => Set(ref _ViewUnchecked, value);
        }

        private Overlay _Overlay = new(false);

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

        public List<string> SetupReasons => AppSettings.Instance.SetupReasons.Select(x => x.Reason).ToList();
        public Dictionary<string, bool> SetupReasonsRequireComment =>
            AppSettings.Instance.SetupReasons.ToDictionary(x => x.Reason, x => x.RequireComment);
        public List<string> MachiningReasons => AppSettings.Instance.MachiningReasons.Select(x => x.Reason).ToList();
        public Dictionary<string, bool> MachiningReasonsRequireComment =>
            AppSettings.Instance.MachiningReasons.ToDictionary(x => x.Reason, x => x.RequireComment);

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
                if (StartDateForCalc == TimeOnly.MinValue || EndDateForCalc == TimeOnly.MaxValue) return TimeSpan.Zero;
                var breakfasts = DescreaseTimes ? TimeOnlys.GetBreaksBetween(StartDateForCalc, EndDateForCalc) : TimeSpan.Zero;
                return EndDateForCalc - StartDateForCalc - breakfasts - new TimeSpan(0, AdditionalDescreaseValue ?? 0, 0);
            }
        }

        public double CalculatedTimeDifferenceMinutes
            => CalculatedTimeDifference.TotalMinutes;


        private bool _CalcFixed;
        /// <summary> Описание </summary>
        public bool CalcFixed
        {
            get => _CalcFixed;
            set => Set(ref _CalcFixed, value);
        }


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


        private bool _UseMockData = false;
        /// <summary>
        /// Фиктивные данные для отладки
        /// </summary>
        public bool UseMockData
        {
            get => _UseMockData;
            set => Set(ref _UseMockData, value);
        }


        public bool MachineVisibility => MachineFilters.Count(m => m.Filter == true) == 1;

        #region IncreaseDateCommand
        public ICommand IncreaseDateCommand { get; }
        private void OnIncreaseDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = FromDate.AddDays(1);
            ToDate = ToDate.AddDays(1);
            UnlockUpdate();
        }
        private bool CanIncreaseDateCommandExecute(object p) => true;
        #endregion

        #region DecreaseDateCommand
        public ICommand DecreaseDateCommand { get; }
        private void OnDecreaseDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = FromDate.AddDays(-1);
            ToDate = ToDate.AddDays(-1);
            UnlockUpdate();
        }
        private bool CanDecreaseDateCommandExecute(object p) => true;
        #endregion

        #region SetYesterdayDateCommand
        public ICommand SetYesterdayDateCommand { get; }
        private void OnSetYesterdayDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = DateTime.Today.AddDays(-1);
            ToDate = FromDate;
            UnlockUpdate();
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
            LockUpdate();
            if (FromDate == ToDate.AddDays(-30))
            {
                FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                ToDate = DateTime.Today.AddDays(-1);
            }
            else
            {
                FromDate = ToDate.AddDays(-30);
            }
            UnlockUpdate();
        }
        private bool CanSetMonthDateCommandExecute(object p) => true;
        #endregion

        #region SetYearDateCommand
        public ICommand SetYearDateCommand { get; }
        private void OnSetYearDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = new DateTime(2024, 01, 01);
            ToDate = DateTime.Today;
            UnlockUpdate();
        }
        private bool CanSetYearDateCommandExecute(object p) => true;
        #endregion

        #region SetAllDateCommand
        public ICommand SetAllDateCommand { get; }
        private void OnSetAllDateCommandExecuted(object p)
        {
            LockUpdate();
            FromDate = new DateTime(2023, 01, 01, 00, 00, 00);
            ToDate = DateTime.Today;
            UnlockUpdate();
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
            MessageBox.Show("Тут будет информация по выборке", "Заглушка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private static bool CanShowInfoCommandExecute(object p) => true;
        #endregion

        #region OperatorReportToExcel
        public ICommand OperatorReportToExcelCommand { get; }
        private async void OnOperatorReportToExcelCommandExecuted(object p)
        {
            try
            {
                using (Overlay = new())
                {
                    var dlg = new ExportOperatorReportDialogWindow();
                    if (p is PartsInfoWindow w) dlg.Owner = w;
                    if (dlg.ShowDialog() == false)
                    {
                        Status = "Отмена";
                        return;
                    }
                    var path = Util.GetXlsxPath();
                    if (string.IsNullOrEmpty(path))
                    {
                        Status = "Выбор файла отменён";
                        return;
                    }
                    if (dlg.DataContext is ExportOperatorDailogWindowViewModel dx)
                    {
                        await Task.Run(() =>
                        {

                            InProgress = true;
                            Status = Xl.ExportOperatorReport(Parts, FromDate, ToDate, path, dx.Type.ToLowerInvariant() == "до" ? 1 : dx.Count, dx.Type.ToLowerInvariant() == "до" ? dx.Count : int.MaxValue);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanOperatorReportToExcelCommandExecute(object p) => true;
        #endregion

        #region OperatorsShiftsReportToExcel
        public ICommand OperatorsShiftsReportToExcelCommand { get; }
        private async void OnOperatorsShiftsReportToExcelCommandExecuted(object p)
        {
            try
            {
                using (Overlay = new())
                {
                    var path = Util.GetXlsxPath();
                    if (string.IsNullOrEmpty(path))
                    {
                        Status = "Выбор файла отменён";
                        return;
                    }
                    await Task.Run(() =>
                    {
                        InProgress = true;
                        Status = Xl.ExportOperatorsShiftsReport(Parts, FromDate, ToDate, path);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanOperatorsShiftsReportToExcelCommandExecute(object p) => true;
        #endregion

        #region ExportPartsReportToExcel
        public ICommand ExportPartsReportToExcelCommand { get; }
        private async void OnExportPartsReportToExcelCommandExecuted(object p)
        {
            try
            {

                var path = Util.GetXlsxPath();
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                await Task.Run(() =>
                {
                    InProgress = true;
                    Status = Xl.ExportPartsInfo(Parts, path, FromDate, ToDate);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportPartsReportToExcelCommandExecute(object p) => true;
        #endregion

        #region ExportReportToExcel
        public ICommand ExportReportToExcelCommand { get; }
        private async void OnExportReportToExcelCommandExecuted(object p)
        {
            try
            {

                var path = Util.GetXlsxPath();
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                await Task.Run(() =>
                {
                    InProgress = true;
                    Status = Xl.ExportReportForPeroid(Parts, FromDate, ToDate, path, AdditionalDescreaseValue);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportReportToExcelCommandExecute(object p) => true;
        #endregion

        #region ExportLongSetups
        public ICommand ExportLongSetupsCommand { get; }
        private async void OnExportLongSetupsCommandExecuted(object p)
        {
            try
            {

                var path = Util.GetXlsxPath();
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                await Task.Run(() =>
                {
                    InProgress = true;
                    Status = Xl.ExportLongSetups(Parts, path);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportLongSetupsCommandExecute(object p) => true;
        #endregion

        #region ExportHistoryToExcel
        public ICommand ExportHistoryToExcelCommand { get; }
        private async void OnExportHistoryToExcelCommandExecuted(object p)
        {
            try
            {
                var dlg = new PartnameDialogWindow(PartNameFilter);
                if (p is PartsInfoWindow w) dlg.Owner = w;
                if (dlg.ShowDialog() != true) return;
                var path = Util.GetXlsxPath();
                var partName = dlg.PartName;
                var oredersCount = dlg.OrderCount;
                await Task.Run(async () =>
                {
                    InProgress = true;
                    var cancellationToken = _cancellationTokenSource.Token;
                    var parts = await Task.Run(() => Database.ReadPartsWithConditions(BuildConditionsForPartForAllTime(partName), cancellationToken));
                    Status = Xl.ExportHistory(parts, path, oredersCount);
                });
                
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportHistoryToExcelCommandExecute(object p) => true;
        #endregion


        /// <summary>
        /// todo
        /// </summary>
        #region ExportShiftsInfoReport
        public ICommand ExportShiftsInfoReportCommand { get; }
        private async void OnExportShiftsInfoReportCommandExecuted(object p)
        {
            try
            {

                var path = Util.GetXlsxPath();
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                await Task.Run(() =>
                {
                    InProgress = true;
                    Status = Xl.ExportShiftsInfo(Parts, path, FromDate, ToDate);
                }
                );

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportShiftsInfoReportCommandExecute(object p) => true;
        #endregion

        /// <summary>
        /// todo
        /// </summary>
        #region ExportVerevkinReportReport
        public ICommand ExportVerevkinReportCommand { get; }
        private async void OnExportVerevkinReportCommandExecuted(object p)
        {
            try
            {
                var path = Util.GetXlsxPath(false);
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                InProgress = true;
                Progress<string> progress = new(p => Status = p);
                await Task.Run(() =>
                {
                    var verevkinParts = Parts.GroupBy(p => new { p.PartName, p.Order })
                    .Select(g =>
                    {
                        var verevkinPart = new VerevkinPart(g.Key.PartName, g.Key.Order, g.Sum(p => p.FinishedCount));

                        foreach (var part in g)
                        {
                            switch (part.Setup)
                            {
                                case 1:
                                    verevkinPart.MachineTime1 += part.MachiningTime;
                                    break;
                                case 2:
                                    verevkinPart.MachineTime2 += part.MachiningTime;
                                    break;
                                case 3:
                                    verevkinPart.MachineTime3 += part.MachiningTime;
                                    break;
                                case 4:
                                    verevkinPart.MachineTime4 += part.MachiningTime;
                                    break;
                                case 5:
                                    verevkinPart.MachineTime5 += part.MachiningTime;
                                    break;
                            }
                        }
                        return verevkinPart;
                    }).ToList();
                    Xl.ExportVerevkinReport(verevkinParts, path, progress);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportVerevkinReportCommandExecute(object p) => true;
        #endregion

        #region ExportToExcel
        public ICommand ExportToExcelCommand { get; }
        private async void OnExportToExcelCommandExecuted(object p)
        {
            try
            {

                var path = Util.GetXlsxPath();
                if (string.IsNullOrEmpty(path))
                {
                    Status = "Выбор файла отменён";
                    return;
                }
                await Task.Run(() =>
                {
                    InProgress = true;
                    Status = Xl.ExportDataset(Parts, path);
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { InProgress = false; }
        }
        private static bool CanExportToExcelCommandExecute(object p) => true;
        #endregion

        #region DeleteFilter
        public ICommand DeleteFilterCommand { get; }
        private void OnDeleteFilterCommandExecuted(object p)
        {
            LockUpdate();
            ShiftFilter = new(ShiftType.All);
            OperatorFilter = string.Empty;
            PartNameFilter = string.Empty;
            OrderFilter = string.Empty;
            SetupFilter = null;
            FinishedCountFilter = "";
            ToDate = PartsInfo.ToDate;
            FromDate = PartsInfo.FromDate;
            _EngineerCommentFilter = "";
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = MachineFilters[i].Machine == PartsInfo.Machine;
            }
            UnlockUpdate();
        }
        private static bool CanDeleteFilterCommandExecute(object p) => true;
        #endregion

        #region ShowAllMachines
        public ICommand ShowAllMachinesCommand { get; }
        private void OnShowAllMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = true;
            }
            UnlockUpdate();
        }
        private static bool CanShowAllMachinesCommandExecute(object p) => true;
        #endregion

        #region HideAllMachines
        public ICommand HideAllMachinesCommand { get; }
        private void OnHideAllMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = false;
            }
            UnlockUpdate();
        }
        private static bool CanHideAllMachinesCommandExecute(object p) => true;
        #endregion

        #region InvertMachines
        public ICommand InvertMachinesCommand { get; }
        private void OnInvertMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = !MachineFilters[i].Filter;
            }
            UnlockUpdate();
        }
        private static bool CanInvertMachinesCommandExecute(object p) => true;
        #endregion

        #region SetLatheMachines
        public ICommand SetLatheMachinesCommand { get; }
        private void OnSetLatheMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = MachineFilters[i].Type.ToLowerInvariant().Contains("токарный");
            }
            UnlockUpdate();
        }
        private static bool CanSetLatheMachinesCommandExecute(object p) => true;
        #endregion

        #region SetMillMachines
        public ICommand SetMillMachinesCommand { get; }
        private void OnSetMillMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = MachineFilters[i].Type.ToLowerInvariant().Contains("фрезерный");
            }
            UnlockUpdate();
        }
        private static bool CanSetMillMachinesCommandExecute(object p) => true;
        #endregion

        #region SetHorMillMachines
        public ICommand SetHorMillMachinesCommand { get; }
        private void OnSetHorMillMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = MachineFilters[i].Type.ToLowerInvariant().Contains("горизонтально");
            }
            UnlockUpdate();
        }
        private static bool CanSetHorMillMachinesCommandExecute(object p) => true;
        #endregion

        #region SetVerMillMachines
        public ICommand SetVerMillMachinesCommand { get; }
        private void OnSetVerMillMachinesCommandExecuted(object p)
        {
            LockUpdate();
            for (int i = 0; i < MachineFilters.Count; i++)
            {
                MachineFilters[i].Filter = MachineFilters[i].Type.ToLowerInvariant().Contains("вертикально");
            }
            UnlockUpdate();
        }
        private static bool CanSetVerMillMachinesCommandExecute(object p) => true;
        #endregion

        #region ShowArchiveTable
        public ICommand ShowArchiveTableCommand { get; }
        private void OnShowArchiveTableCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                //var disqualifiedKeys = new HashSet<(string PartName, int Setup, string Machine)>();
                //var filteredPrograms = new List<Part>();
                //foreach (var part in Parts)
                //{
                //    var key = (part.PartName, part.Setup, part.Machine);

                //}

                var parts = Parts
                    .GroupBy(p => (p.PartName, p.Setup, p.Machine))
                    .Select(g => g.OrderByDescending(p => p.StartSetupTime).First())
                    .OrderBy(p => p.Machine)
                    .ThenBy(p => p.StartSetupTime).ToList();

                ArchiveListWindow archiveListWindow = new(parts) { Owner = p as PartsInfoWindow };
                archiveListWindow.ShowDialog();
            }
        }
        private static bool CanShowArchiveTableCommandExecute(object p) => true;
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

        #region ChangeRole
        public ICommand ChangeRoleCommand { get; }
        private void OnChangeRoleCommandExecuted(object p) 
        {
            if (p is string str && Enum.TryParse(str, out User newRole))
            {
                ViewMode = newRole;
            }
        }
        private static bool CanChangeRoleCommandExecute(object p) => true;
        #endregion

        #region ChangeShowUnchecked
        public ICommand ChangeShowUncheckedCommand { get; }
        private void OnChangeShowUncheckedCommandExecuted(object p)
        {
            ViewUnchecked = !ViewUnchecked;
            EngineerCommentFilter = ViewUnchecked ? "ожидание" : "";
        }
        private static bool CanChangeShowUncheckedCommandExecute(object p) => true;
        #endregion

        #region ChangeCalcFixed
        public ICommand ChangeCalcFixedCommand { get; }
        private void OnChangeCalcFixedCommandExecuted(object p)
        {
            Part.CalcFixed = !Part.CalcFixed;
            CalcFixed = Part.CalcFixed;
        }
        private static bool CanChangeCalcFixedCommandExecute(object p) => true;
        #endregion

        #region OpenDailyReportWindow
        public ICommand OpenDailyReportWindowCommand { get; }

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(FinishedCountFilter) when !TryParseComparison(FinishedCountFilter, out _, out _) && !string.IsNullOrWhiteSpace(FinishedCountFilter):
                        return "Неверно указан фильтр по количеству изготовленных деталей.";

                    default:
                        return null!;
                }
            }
        }

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
            if (Parts.Any(x => x.NeedUpdate))
            {
                MessageBox.Show("Есть несохраненные данные.", "Подтверждение.",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (HasErrors)
            {
                MessageBox.Show("Не всё заполнено корректно.", "Предупреждение.",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (Overlay = new())
            {
                var dailyInfoWindow = new DailyReportWindow((Parts, ToDate, PartsInfo.Machine)) { Owner = p as PartsInfoWindow };
                dailyInfoWindow.ShowDialog();
            }
        }
        private static bool CanOpenDailyReportWindowCommandExecute(object p) => true;
        #endregion

        private async Task<bool> LoadPartsAsync(bool first = false)
        {
            if (lockUpdate) return false;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new();
            var cancellationToken = _cancellationTokenSource.Token;
            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                InProgress = true;
                Status = "Получение информации...";
                if (!first) await Task.Delay(1000, cancellationToken);

                if (UseMockData)
                {
                    Parts = await Util.GenerateMockPartsAsync();
                    InProgress = false;
                    return true;
                }

                var isValidFinishedCountFilter = TryParseComparison(FinishedCountFilter, out string finishedCountOperator, out int finishedCountValue);
                var isValidTotalCountFilter = TryParseComparison(TotalCountFilter, out string totalCountOperator, out int totalCountValue);
                var partNameStartStar = PartNameFilter.StartsWith('*');

                if (!MachineFilters.Any(mf => mf.Filter == true))
                {
                    Application.Current.Dispatcher.Invoke(() => { Parts.Clear(); });
                    InProgress = false;
                    return false;
                }

                var tempParts = await Task.Run(() => Database.ReadPartsWithConditions(BuildConditions(), cancellationToken));

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Parts.Clear();
                    Parts = new(tempParts);
                });
                InProgress = false;
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
                InProgress = false;
                return false;
            }
            finally
            {
                semaphoreSlim.Release();
                Status = "";
            }
        }

        /// <summary>
        /// Собирает строку запроса
        /// </summary>
        /// <returns></returns>
        private string BuildConditions()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("ShiftDate BETWEEN '{0}' AND '{1}' ", FromDate, ToDate);

            if (ShiftFilter is not { Type: ShiftType.All })
            {
                sb.AppendFormat("AND Shift = '{0}' ", ShiftFilter.FilterText);
            }

            AppendCondition(sb, "Operator", OperatorFilter);
            AppendCondition(sb, "PartName", PartNameFilter);
            AppendCondition(sb, "[Order]", OrderFilter);
            AppendCondition(sb, "EngineerComment", EngineerCommentFilter);

            if (TryParseComparison(FinishedCountFilter, out var finishedCountOperator, out var finishedCountValue))
                sb.AppendFormat("AND FinishedCount {0} {1} ", finishedCountOperator, finishedCountValue);

            if (TryParseComparison(TotalCountFilter, out var totalCountOperator, out var totalCountValue))
                sb.AppendFormat("AND totalCount {0} {1} ", totalCountOperator, totalCountValue);

            if (SetupFilter != null)
                sb.AppendFormat("AND Setup = {0}", SetupFilter);

            var machines = string.Join(", ", MachineFilters.Where(mf => mf.Filter).Select(m => $"'{m.Machine}'").Distinct());
            sb.AppendFormat("AND Machine IN ({0}) ", machines);

            return sb.ToString();
        }

        /// <summary>
        /// Собирает строку запроса для получения списка всех изготовлений на всех станках для этой детали
        /// </summary>
        /// <returns></returns>
        private string BuildConditionsForPartForAllTime(string partName)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("ShiftDate BETWEEN '{0}' AND '{1}' ", new DateTime(2023, 1, 1), DateTime.Today);

            AppendCondition(sb, "PartName", FormatSearchInput(partName));

            return sb.ToString();
        }

        /// <summary>
        /// Поиск по строке, либо с полным совпадением, либо "is Like"
        /// </summary>
        /// <param name="sb">StringBuilder в котором формируется строка запроса</param>
        /// <param name="column">Столбец в который будем писать</param>
        /// <param name="value">Значение</param>
        /// <param name="isLike">Примерный поиск, либо точный</param>
        private void AppendCondition(StringBuilder sb, string column, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var pattern = new SearchPattern(value);
                sb.AppendFormat("AND {0} {1} ", column, pattern);
            }
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

        public static bool TryParseComparison(string input, out string comparisonOperator, out int comparisonValue)
        {
            comparisonOperator = null!;
            comparisonValue = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (int.TryParse(input, out int res))
            {
                comparisonOperator = "=";
                comparisonValue = res;
                return true;
            }

            foreach (string op in supportedComparisonOperators)
            {
                if (input.Contains(op))
                {
                    comparisonOperator = op;
                    string[] parts = input.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 1)
                        return false;

                    if (!int.TryParse(parts[0].Trim(), out comparisonValue))
                        return false;
                    return true;
                }
            }

            return false;
        }


        public static string FormatSearchInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.StartsWith('*') && input.EndsWith('*')) return input.Replace('*', '%');
            if (input.StartsWith('=')) return input[1..];
            if (input.StartsWith('*')) return $"%{input[1..]}";
            if (input.EndsWith('*')) return $"{input[..^1]}%";
            return $"%{input}%";
        }

        private void LockUpdate() => lockUpdate = true;

        private void UnlockUpdate(bool first = false)
        {
            lockUpdate = false;
            _ = LoadPartsAsync(first);
        }

        private void UpdateHasErrors()
        {
            HasErrors = Parts.Any(x => !string.IsNullOrEmpty(x.Error));
        }
    }   
}
