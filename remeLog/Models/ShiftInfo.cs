using remeLog.Infrastructure.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class ShiftInfo
    {
        public ShiftInfo(int? id, DateTime shiftDate, ShiftType shiftType, string machine, string master, double unspecifiedDowntimes, string downtimesComment, string commonComment, bool isChecked)
        {
            Id = id;
            ShiftDate = shiftDate;
            Shift = new Shift(shiftType).Name;
            Machine = machine;
            Master = master;
            UnspecifiedDowntimes = unspecifiedDowntimes;
            DowntimesComment = downtimesComment;
            CommonComment = commonComment;
            IsChecked = isChecked;
        }
        public ShiftInfo(int? id, DateTime shiftDate, string shiftType, string machine, string master, double unspecifiedDowntimes, string downtimesComment, string commonComment, bool isChecked)
        {
            Id = id;
            ShiftDate = shiftDate;
            Shift = shiftType;
            Machine = machine;
            Master = master;
            UnspecifiedDowntimes = unspecifiedDowntimes;
            DowntimesComment = downtimesComment;
            CommonComment = commonComment;
            IsChecked = isChecked;
        }
        public ShiftInfo(DateTime shiftDate, ShiftType shiftType, string machine)
        {
            ShiftDate = shiftDate;
            Shift = new Shift(shiftType).Name;
            Machine = machine;
            Master = "";
            UnspecifiedDowntimes = 0;
            DowntimesComment = "";
            CommonComment = "";
        }
        public int? Id { get; set; }
        public DateTime ShiftDate { get; set; }
        public string Shift { get; set; }
        public string Machine { get; set; }
        public string Master { get; set; }
        public double UnspecifiedDowntimes { get; set; }
        public string DowntimesComment { get; set; }
        public string CommonComment { get; set; }
        public bool IsChecked { get; set; }
    }
}
