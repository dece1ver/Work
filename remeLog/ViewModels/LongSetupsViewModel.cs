using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using remeLog.Infrastructure;
using remeLog.Models;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace remeLog.ViewModels
{
    class LongSetupsViewModel : ViewModel
    {
        DateTime startDate;
        DateTime endDate;
        public LongSetupsViewModel(ObservableCollection<Part> parts)
        {
            _Parts = parts;
            startDate = Parts.Select(p => p.ShiftDate).Min();
            endDate = Parts.Select(p => p.ShiftDate).Max();

            UpdatePartsCommand = new LambdaCommand(OnUpdatePartsCommandExecuted, CanUpdatePartsCommandExecute);
        }

        private ObservableCollection<Part> _Parts;
        /// <summary> Изготовления </summary>
        public ObservableCollection<Part> Parts
        {
            get => _Parts;
            set
            {
                Set(ref _Parts, value);
            }
        }

        private Part? _SelectedPart;
        /// <summary> Выбранная деталь </summary>
        public Part? SelectedPart
        {
            get => _SelectedPart;
            set => Set(ref _SelectedPart, value);
        }

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

        #region UpdateParts
        public ICommand UpdatePartsCommand { get; }
        private async void OnUpdatePartsCommandExecuted(object p)
        {
            await Task.Run(UpdatePartsAsync);
        }
        private static bool CanUpdatePartsCommandExecute(object p) => true;
        #endregion

        async Task UpdatePartsAsync()
        {
            try
            {
                foreach (var part in Parts.Where(p => p.NeedUpdate))
                {
                    await part.UpdatePartAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async Task LoadPartsAsync()
        {
            
        }
    }
}
