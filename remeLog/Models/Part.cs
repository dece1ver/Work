using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class Part
    {
        public Part(Guid guid, string machine, DateTime shiftDate, string shift, string @operator, string partName, string order, int setup, int finishedCount, int totalCount, DateTime startSetupTime, DateTime startMachiningTime, DateTime endMachiningTime, double setupTimePlan, double setupTimePlanForReport, double singleProductionTimePlan, TimeSpan machiningTime, double setupDowntimes, double machiningDowntimes, double partialSetupTime, double maintenanceTime, double toolSearchingTime, double mentoringTime, double contactingDepartmentsTime, double fixtureMakingTime, double hardwareFailureTime)
        {
            Guid = guid;
            Machine = machine;
            ShiftDate = shiftDate;
            Shift = shift;
            Operator = @operator;
            PartName = partName;
            Order = order;
            Setup = setup;
            FinishedCount = finishedCount;
            TotalCount = totalCount;
            StartSetupTime = startSetupTime;
            StartMachiningTime = startMachiningTime;
            EndMachiningTime = endMachiningTime;
            SetupTimePlan = setupTimePlan;
            SetupTimePlanForReport = setupTimePlanForReport;
            SingleProductionTimePlan = singleProductionTimePlan;
            MachiningTime = machiningTime;
            SetupDowntimes = setupDowntimes;
            MachiningDowntimes = machiningDowntimes;
            PartialSetupTime = partialSetupTime;
            MaintenanceTime = maintenanceTime;
            ToolSearchingTime = toolSearchingTime;
            MentoringTime = mentoringTime;
            ContactingDepartmentsTime = contactingDepartmentsTime;
            FixtureMakingTime = fixtureMakingTime;
            HardwareFailureTime = hardwareFailureTime;
        }

        public Guid Guid { get; set; }
        public string Machine { get; set; }
        public DateTime ShiftDate { get; set; }
        public string Shift { get; set; }
        public string Operator { get; set; }
        public string PartName { get; set; }
        public string Order { get; set; }
        public int Setup { get; set; }
        public int FinishedCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime StartSetupTime { get; set; }
        public DateTime StartMachiningTime { get; set; }
        public DateTime EndMachiningTime { get; set; }
        public double SetupTimePlan { get; set; }
        public double SetupTimePlanForReport { get; set; }
        public double SingleProductionTimePlan { get; set; }
        public TimeSpan MachiningTime { get; set; }
        public double SetupDowntimes { get; set; }
        public double MachiningDowntimes { get; set; }
        public double PartialSetupTime { get; set; }
        public double MaintenanceTime { get; set; }
        public double ToolSearchingTime { get; set; }
        public double MentoringTime { get; set; }
        public double ContactingDepartmentsTime { get; set; }
        public double FixtureMakingTime { get; set; }
        public double HardwareFailureTime { get; set; }
    }
    
    
}
