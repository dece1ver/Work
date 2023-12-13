using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class Part : ViewModel
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

        private Guid _Guid;
        /// <summary> GUID </summary>
        public Guid Guid
        {
            get => _Guid;
            set => Set(ref _Guid, value);
        }

        private string _Machine;
        /// <summary> Станок </summary>
        public string Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        private DateTime _ShiftDate;
        /// <summary> Дата смены </summary>
        public DateTime ShiftDate
        {
            get => _ShiftDate;
            set => Set(ref _ShiftDate, value);
        }


        private string _Shift;
        /// <summary> Смена </summary>
        public string Shift
        {
            get => _Shift;
            set => Set(ref _Shift, value);
        }

        private string _Operator;
        /// <summary> Оператор </summary>
        public string Operator
        {
            get => _Operator;
            set => Set(ref _Operator, value);
        }


        private string _PartName;
        /// <summary> Название детали </summary>
        public string PartName
        {
            get => _PartName;
            set => Set(ref _PartName, value);
        }


        private string _Order;
        /// <summary> Номер маршрутного листа </summary>
        public string Order
        {
            get => _Order;
            set => Set(ref _Order, value);
        }


        private int _Setup;
        /// <summary> Номер установки </summary>
        public int Setup
        {
            get => _Setup;
            set => Set(ref _Setup, value);
        }


        private int _FinishedCount;
        /// <summary> Изготовлено </summary>
        public int FinishedCount
        {
            get => _FinishedCount;
            set => Set(ref _FinishedCount, value);
        }

        /// <summary> Изготовлено по факту с учетом наладок </summary>
        public int FinishedCountFact
        {
            get
            {
                return StartSetupTime != StartMachiningTime && FinishedCount != 0 ? FinishedCount - 1 : FinishedCount;
            }
        }

        private int _TotalCount;
        /// <summary> Всего партия </summary>
        public int TotalCount
        {
            get => _TotalCount;
            set => Set(ref _TotalCount, value);
        }


        private DateTime _StartSetupTime;
        /// <summary> Начало наладки </summary>
        public DateTime StartSetupTime
        {
            get => _StartSetupTime;
            set => Set(ref _StartSetupTime, value);
        }

        private DateTime _StartMachiningTime;
        /// <summary> Завершение наладки / начало изготовления </summary>
        public DateTime StartMachiningTime
        {
            get => _StartMachiningTime;
            set => Set(ref _StartMachiningTime, value);
        }

        private double _SetupTimeFact;
        /// <summary> Фактическое время наладки </summary>
        public double SetupTimeFact
        {
            get => _SetupTimeFact;
            set
            {
                Set(ref _SetupTimeFact, value);
                OnPropertyChanged(nameof(SetupRatio));
                OnPropertyChanged(nameof(SetupRatioTitle));
            }
        }

        private DateTime _EndMachiningTime;
        /// <summary> Завершение изготовления </summary>
        public DateTime EndMachiningTime
        {
            get => _EndMachiningTime;
            set => Set(ref _EndMachiningTime, value);
        }

        private double _SetupTimePlan;
        /// <summary> Норматив наладки </summary>
        public double SetupTimePlan
        {
            get => _SetupTimePlan;
            set
            {
                Set(ref _SetupTimePlan, value);
                OnPropertyChanged(nameof(SetupRatio));
                OnPropertyChanged(nameof(SetupRatioTitle));
            }
        }


        private double _SetupTimePlanForReport;
        /// <summary> Норматив наладки для отчета </summary>
        public double SetupTimePlanForReport
        {
            get => _SetupTimePlanForReport;
            set => Set(ref _SetupTimePlanForReport, value);
        }


        private double _SingleProductionTimePlan;
        /// <summary> Штучный норматив </summary>
        public double SingleProductionTimePlan
        {
            get => _SingleProductionTimePlan;
            set => Set(ref _SingleProductionTimePlan, value);
        }


        private double _ProductionTimeFact;
        /// <summary> Фасктическое время изготовления </summary>
        public double ProductionTimeFact
        {
            get => _ProductionTimeFact;
            set => Set(ref _ProductionTimeFact, value);
        }


        private TimeSpan _MachiningTime;
        /// <summary> Машинное время </summary>
        public TimeSpan MachiningTime
        {
            get => _MachiningTime;
            set => Set(ref _MachiningTime, value);
        }


        private double _SetupDowntimes;
        /// <summary> Время простоев в наладке </summary>
        public double SetupDowntimes
        {
            get => _SetupDowntimes;
            set => Set(ref _SetupDowntimes, value);
        }


        private double _MachiningDowntimes;
        /// <summary> Время простоев в изготовлении </summary>
        public double MachiningDowntimes
        {
            get => _MachiningDowntimes;
            set => Set(ref _MachiningDowntimes, value);
        }


        private double _PartialSetupTime;
        /// <summary> Время простоя "Частичная наладка" </summary>
        public double PartialSetupTime
        {
            get => _PartialSetupTime;
            set => Set(ref _PartialSetupTime, value);
        }


        private double _MaintenanceTime;
        /// <summary> Время простоя "Обслуживание" </summary>
        public double MaintenanceTime
        {
            get => _MaintenanceTime;
            set => Set(ref _MaintenanceTime, value);
        }


        private double _ToolSearchingTime;
        /// <summary> Время простоя "Поиск и получение инструмента" </summary>
        public double ToolSearchingTime
        {
            get => _ToolSearchingTime;
            set => Set(ref _ToolSearchingTime, value);
        }


        private double _MentoringTime;
        /// <summary> Время простоя "Помощь / обучение" </summary>
        public double MentoringTime
        {
            get => _MentoringTime;
            set => Set(ref _MentoringTime, value);
        }


        private double _ContactingDepartmentsTime;
        /// <summary> Время простоя "Обращение в другие службы" </summary>
        public double ContactingDepartmentsTime
        {
            get => _ContactingDepartmentsTime;
            set => Set(ref _ContactingDepartmentsTime, value);
        }


        private double _FixtureMakingTime;
        /// <summary> Время простоя "Изготовление оснастки и калибров" </summary>
        public double FixtureMakingTime
        {
            get => _FixtureMakingTime;
            set => Set(ref _FixtureMakingTime, value);
        }


        private double _HardwareFailureTime;
        /// <summary> Время простоя "Отказ оборудования" </summary>
        public double HardwareFailureTime
        {
            get => _HardwareFailureTime;
            set => Set(ref _HardwareFailureTime, value);
        }


        private string _OperatorComment;
        /// <summary> Комментарий оператора </summary>
        public string OperatorComment
        {
            get => _OperatorComment;
            set => Set(ref _OperatorComment, value);
        }


        private string _MasterSetupComment;
        /// <summary> Комментарий к отклонениям от нормативов в наладке </summary>
        public string MasterSetupComment
        {
            get => _MasterSetupComment;
            set => Set(ref _MasterSetupComment, value);
        }


        private string _MasterMachiningComment;
        /// <summary> Комментарий к отклонениям от нормативов в изготовлении </summary>
        public string MasterMachiningComment
        {
            get => _MasterMachiningComment;
            set => Set(ref _MasterMachiningComment, value);
        }


        private string _SpecifiedDowntimesComment;
        /// <summary> Комментарий к зарегистрированным простоям </summary>
        public string SpecifiedDowntimesComment
        {
            get => _SpecifiedDowntimesComment;
            set => Set(ref _SpecifiedDowntimesComment, value);
        }


        private string _UnspecifiedDowntimesComment;
        /// <summary> Комментарий к незарегистрированным простоям </summary>
        public string UnspecifiedDowntimesComment
        {
            get => _UnspecifiedDowntimesComment;
            set => Set(ref _UnspecifiedDowntimesComment, value);
        }


        private string _MasterComment;
        /// <summary> Комментарий мастера </summary>
        public string MasterComment
        {
            get => _MasterComment;
            set => Set(ref _MasterComment, value);
        }


        private string _EngineerComment;
        /// <summary> Комментарий техотдела </summary>
        public string EngineerComment
        {
            get => _EngineerComment;
            set => Set(ref _EngineerComment, value);
        }

        public double SetupRatio => SetupTimePlan / SetupTimeFact;
        public string SetupRatioTitle => SetupRatio is double.NaN or double.PositiveInfinity ? "б/н" : $"{SetupRatio:0%}";
        public double ProductionRatio => FinishedCountFact * SingleProductionTimePlan / ProductionTimeFact;
        public string ProductionRatioTitle => ProductionRatio is double.NaN or double.PositiveInfinity ? "б/и" : $"{ProductionRatio:0%}";
    }
}
