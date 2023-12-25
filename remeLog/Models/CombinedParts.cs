using DocumentFormat.OpenXml.Spreadsheet;
using remeLog.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class CombinedParts
    {
        public CombinedParts(string machine)
        {
            Machine = machine;
        }

        public string Machine { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public ObservableCollection<Part> Parts { get; set; } = new();
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
        public double AverageSetupRatio => Parts.AverageSetupRatio();
        public double AverageProductionRatio => Parts.AverageProductionRatio();
        public double SetupTimeRatio => Parts.SetupRatio();
        public double ProductionTimeRatio => Parts.ProductionRatio();
        public double SpecifiedDowntimesRatio => Parts.SpecifiedDowntimesRatio(FromDate, ToDate, new Shift(Infrastructure.Types.ShiftType.All));
        public double UnspecifiedDowntimesRatio => Parts.UnspecifiedDowntimesRatio(FromDate, ToDate, new Shift(Infrastructure.Types.ShiftType.All));
    }
}
