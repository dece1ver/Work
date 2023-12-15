using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using remeLog.Infrastructure;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            PartsInfo = parts;
            ShiftFilterItems = new string[3] { "Все смены", "День", "Ночь" };
            _ShiftFilter = ShiftFilterItems[0];
            _OperatorFilter = "";
            _Parts = PartsInfo.Parts;
            _OrderFilter = "";
            _PartNameFilter = "";
            _FromDate = PartsInfo.FromDate;
            _ToDate = PartsInfo.ToDate;
        }


        public string[] ShiftFilterItems { get; set; }

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
                _ = UpdateParts();
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
                _ = UpdateParts();
            }
        }

        private string _ShiftFilter;
        /// <summary> Фильтр смены </summary>
        public string ShiftFilter
        {
            get => _ShiftFilter;
            set
            {
                Set(ref _ShiftFilter, value);
                _ = UpdateParts();
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
                _ = UpdateParts();
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
                _ = UpdateParts();
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
                _ = UpdateParts();
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

        public double AverageSetupRatio
        {
            get
            {
                //double sum = 0;
                //int cnt = 0;
                //foreach (var part in Parts)
                //{
                //    if (part.SetupTimeFact > 0 && part.SetupTimePlan > 0)
                //    {
                //        sum += part.SetupTimePlan / part.SetupTimeFact;
                //        cnt++;
                //    }
                //}
                //return cnt == 0 ? 0 : sum / cnt;
                var validSetupRatios = Parts.Where(p => p.SetupRatio > 0 && !double.IsNaN(p.SetupRatio) && !double.IsPositiveInfinity(p.SetupRatio)).Select(p => p.SetupRatio);
                return validSetupRatios.Any() ? validSetupRatios.Average() : 0.0;
            }
        }

        public double AverageProductionRatio
        {
            get
            {
                var validProductionRatios = Parts.Where(p => p.ProductionRatio > 0 && !double.IsNaN(p.ProductionRatio) && !double.IsPositiveInfinity(p.ProductionRatio)).Select(p => p.ProductionRatio);
                return validProductionRatios.Any() ? validProductionRatios.Average() : 0.0;
            }
        }

        public double SetupTimeRatio
        {
            get
            {
                double planSum = 0;
                double factSum = 0;
                foreach (var part in Parts)
                {
                    factSum += part.SetupTimeFact + part.PartialSetupTime;
                    planSum += part.SetupTimePlanForReport;
                }
                return factSum == 0 ? 0 : planSum / factSum;
                var validSetupTimes = Parts.Where(p => p.SetupTimePlan > 0 && p.SetupTimeFact > 0 && !double.IsPositiveInfinity(p.SetupTimeFact)).Select(p => p.SetupTimePlan / p.SetupTimeFact);
                return validSetupTimes.Any() ? validSetupTimes.Sum() : 0.0;
            }
        }

        public double ProductionTimeRatio
        {
            get
            {
                double planSum = 0;
                double factSum = 0;
                foreach (var part in Parts)
                {
                    factSum += part.ProductionTimeFact;
                    planSum += part.FinishedCountFact * part.SingleProductionTimePlan;
                }
                return factSum == 0 ? 0 : planSum / factSum;
                var validRatios = Parts.Where(p => p.FinishedCount > 0 && p.SingleProductionTimePlan > 0 && p.ProductionTimeFact > 0 && !double.IsPositiveInfinity(p.ProductionTimeFact)).Select(p => (p.FinishedCount * p.SingleProductionTimePlan) / p.ProductionTimeFact);
                return validRatios.Any() ? validRatios.Sum() : 0.0;
            }
        }

        public double SpecifiedDowntimesRatio { get 
            {
                double sum = 0;
                foreach (var part in Parts)
                {
                    sum += part.SetupDowntimes + part.MachiningDowntimes;
                }
                var totalWorkMinutes = (ToDate.AddDays(1) - ToDate).TotalDays * 1290;
                return sum / totalWorkMinutes;

            } }

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

            FromDate = FromDate.AddDays(-7);
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

        private async Task UpdateParts()
        {
            await Task.Run(() => {
                try
                {
                    InProgress = true;
                    Parts = Database.ReadPartsWithConditions($"Machine = '{PartsInfo.Machine}' AND ShiftDate BETWEEN '{FromDate}' AND '{ToDate}' " +
                    $"{(ShiftFilter == ShiftFilterItems[0] ? "" : $"AND Shift = '{ShiftFilter}'")}" +
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
