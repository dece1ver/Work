using DocumentFormat.OpenXml.Spreadsheet;
using libeLog.Base;
using remeLog.Infrastructure.Extensions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class CombinedParts : ViewModel
    {
        public enum ReportState
        {
            Exist, NotExist, Partial
        }

        public CombinedParts(string machine)
        {
            _Machine = machine;
        }

        public CombinedParts(string machine, DateTime fromDate, DateTime toDate)
        {
            _Machine = machine;
            _FromDate = fromDate;
            _ToDate = toDate;
        }

        public CombinedParts(CombinedParts cp)
        {
            _Machine = cp.Machine;
            _FromDate = cp.FromDate;
            _ToDate = cp.ToDate;
            _ToDate = cp.ToDate;
            _Parts = new();
            foreach (var part in cp.Parts)
            {
                _Parts.Add(new Part(part));
            }
        }


        private string _Machine;
        public string Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }



        private DateTime _FromDate;
        public DateTime FromDate
        {
            get => _FromDate;
            set => Set(ref _FromDate, value);
        }

        private DateTime _ToDate;
        public DateTime ToDate
        {
            get => _ToDate;
            set => Set(ref _ToDate, value);
        }

        private ReportState _IsReportExist = ReportState.NotExist;
        /// <summary> Существует ли отчет за смену </summary>
        public ReportState IsReportExist
        {
            get => _IsReportExist;
            set => Set(ref _IsReportExist, value);
        }

        private ObservableCollection<Part> _Parts = new();
        public ObservableCollection<Part> Parts
        {
            get => _Parts;
            set 
            {
                if (Set(ref _Parts, value))
                {
                    OnPropertyChanged(nameof(TotalShifts));
                    OnPropertyChanged(nameof(WorkedShifts));
                    OnPropertyChanged(nameof(AverageSetupRatio));
                    OnPropertyChanged(nameof(AverageProductionRatio));
                    OnPropertyChanged(nameof(SetupTimeRatio));
                    OnPropertyChanged(nameof(ProductionTimeRatio));
                    OnPropertyChanged(nameof(SpecifiedDowntimesRatio));
                    OnPropertyChanged(nameof(UnspecifiedDowntimesRatio));
                }
            }
        }

        public int TotalShifts => (int)(ToDate.AddDays(1) - FromDate).TotalDays * 2;
        public int WorkedShifts
        {
            get
            {
                var partsByDates = Parts.Where(part => part.ShiftDate >= FromDate && part.ShiftDate <= ToDate)
                                        .Select(part => new { part.ShiftDate.Date, part.Shift })
                                        .Distinct();
                return partsByDates.Count();
            }
        }

        public bool IsSingleShift => FromDate == ToDate;
        public double AverageSetupRatio => Parts.AverageSetupRatio();
        public double AverageProductionRatio => Parts.AverageProductionRatio();
        public double SetupTimeRatio => Parts.SetupRatio();
        public double ProductionTimeRatio => Parts.ProductionRatio();
        public double SpecifiedDowntimesRatio => Parts.SpecifiedDowntimesRatio(FromDate, ToDate, new Shift(Infrastructure.Types.ShiftType.All));
        public double UnspecifiedDowntimesRatio => Parts.UnspecifiedDowntimesRatio(FromDate, ToDate, new Shift(Infrastructure.Types.ShiftType.All));
    }
}
