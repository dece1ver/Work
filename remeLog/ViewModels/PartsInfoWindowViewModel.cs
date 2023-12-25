using libeLog;
using libeLog.Base;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Extensions;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using System;
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
            UpdatePartsCommand = new LambdaCommand(OnUpdatePartsCommandExecuted, CanUpdatePartsCommandExecute);

            PartsInfo = parts;
            ShiftFilterItems = new Shift[3] { new Shift(ShiftType.All), new Shift(ShiftType.Day), new Shift(ShiftType.Night) };
            _ShiftFilter = ShiftFilterItems.FirstOrDefault();
            _OperatorFilter = "";
            _Parts = PartsInfo.Parts;
            _OrderFilter = "";
            _PartNameFilter = "";
            _FromDate = PartsInfo.FromDate;
            _ToDate = PartsInfo.ToDate;
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

        public double AverageSetupRatio => Parts.AverageSetupRatio();
        public double AverageProductionRatio => Parts.AverageProductionRatio();
        public double SetupTimeRatio => Parts.SetupRatio();
        public double ProductionTimeRatio => Parts.ProductionRatio();
        public double SpecifiedDowntimesRatio => Parts.SpecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter);
        public double UnspecifiedDowntimesRatio => Parts.UnspecifiedDowntimesRatio(FromDate, ToDate, ShiftFilter);

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
            foreach (var part in Parts.Where(p => p.NeedUpdate))
            {
                switch (part.UpdatePart())
                {
                    case libeLog.Models.DbResult.Ok:
                        part.NeedUpdate = false;
                        break;
                    case libeLog.Models.DbResult.AuthError:
                        MessageBox.Show("Ошибка авторизации");
                        break;
                    case libeLog.Models.DbResult.Error:
                        MessageBox.Show("Ошибка");
                        break;
                }
            }
            _ = LoadPartsAsync();
        }
        private static bool CanUpdatePartsCommandExecute(object p) => true;
        #endregion

        private async Task LoadPartsAsync()
        {
            await Task.Run(() => {
                try
                {
                    InProgress = true;
                    Parts = Database.ReadPartsWithConditions($"Machine = '{PartsInfo.Machine}' AND ShiftDate BETWEEN '{FromDate}' AND '{ToDate}' " +
                    $"{(ShiftFilter is { Type: ShiftType.All } ? "" : $"AND Shift = '{ShiftFilter.FilterText}'")}" +
                    $"{(string.IsNullOrEmpty(OperatorFilter) ? "" : $"AND Operator LIKE '%{OperatorFilter}%'")}" +
                    $"{(string.IsNullOrEmpty(PartNameFilter) ? "" : $"AND PartName LIKE '%{PartNameFilter}%'")}" +
                    $"{(string.IsNullOrEmpty(OrderFilter) ? "" : $"AND [Order] LIKE '%{OrderFilter}%'")}" +
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
