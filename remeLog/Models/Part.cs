using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class Part
    {
        public Part(
            Guid guid,
            string machine,
            DateTime shiftDate,
            string shift,
            string @operator,
            string partName,
            string order,
            int setup,
            int finishedCount,
            int totalCount,
            DateTime startSetupTime,
            DateTime startMachiningTime,
            double setupTimeFact,
            DateTime endMachiningTime,
            double setupTimePlan,
            double setupTimePlanForReport,
            double singleProductionTimePlan,
            double productionTimeFact, TimeSpan machiningTime,
            double setupDowntimes,
            double machiningDowntimes,
            double partialSetupTime,
            double maintenanceTime,
            double toolSearchingTime,
            double mentoringTime,
            double contactingDepartmentsTime,
            double fixtureMakingTime,
            double hardwareFailureTime,
            string operatorComment, 
            string masterSetupComment = "", 
            string masterMachiningComment = "",
            string specifiedDowntimesComment = "", 
            string unspecifiedDowntimesComment = "", 
            string masterComment = "", 
            string engineerComment = "")
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
            SetupTimeFact = setupTimeFact;
            EndMachiningTime = endMachiningTime;
            SetupTimePlan = setupTimePlan;
            SetupTimePlanForReport = setupTimePlanForReport;
            SingleProductionTimePlan = singleProductionTimePlan;
            ProductionTimeFact = productionTimeFact;
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
            OperatorComment = operatorComment;
            MasterSetupComment = masterSetupComment;
            MasterMachiningComment = masterMachiningComment;
            SpecifiedDowntimesComment = specifiedDowntimesComment;
            UnspecifiedDowntimesComment = unspecifiedDowntimesComment;
            MasterComment = masterComment;
            EngineerComment = engineerComment;
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
        public double SetupTimeFact { get; set; }
        public DateTime EndMachiningTime { get; set; }
        public double SetupTimePlan { get; set; }
        public double SetupTimePlanForReport { get; set; }
        public double SingleProductionTimePlan { get; set; }
        public double ProductionTimeFact { get; set; }
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
        public string OperatorComment { get; set; }
        public string MasterSetupComment { get; set; }
        public string MasterMachiningComment { get; set; }
        public string SpecifiedDowntimesComment { get; set; }
        public string UnspecifiedDowntimesComment { get; set; }
        public string MasterComment { get; set; }
        public string EngineerComment { get; set; }
        public double SetupRatio => SetupTimePlan / SetupTimeFact;
        public string SetupRatioTitle => SetupRatio is double.NaN or double.PositiveInfinity ? "б/н" : $"{SetupRatio:0%}";
        public double ProductionRatio => FinishedCount * SingleProductionTimePlan / ProductionTimeFact;
        public string ProductionRatioTitle => ProductionRatio is double.NaN or double.PositiveInfinity ? "б/и" : $"{ProductionRatio:0%}";
    }
}
