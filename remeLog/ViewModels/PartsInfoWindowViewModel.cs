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

            PartsInfo = parts;
            ShiftFilterItems = new string[3] { "Все смены", "День", "Ночь" };
            _ShiftFilter = ShiftFilterItems[0];
            _OperatorFilter = "";
            _Parts = PartsInfo.Parts;
        }


        public string[] ShiftFilterItems { get; set; }

        private ObservableCollection<Part> _Parts;
        /// <summary> Описание </summary>
        public ObservableCollection<Part> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }


        private string _ShiftFilter;
        /// <summary> Фильтр смены </summary>
        public string ShiftFilter
        {
            get => _ShiftFilter;
            set
            {
                Set(ref _ShiftFilter, value);
                UpdateParts();
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
                UpdateParts();
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
                UpdateParts();
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
                UpdateParts();
            }
        }


        public CombinedParts PartsInfo { get; set; }


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
                    Parts = Database.ReadPartsWithConditions($"Machine = '{PartsInfo.Machine}' AND ShiftDate BETWEEN '{PartsInfo.FromDate}' AND '{PartsInfo.ToDate}' " +
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

                }
            });
        }
    }
}
