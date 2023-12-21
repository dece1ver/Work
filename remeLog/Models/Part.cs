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
            _Guid = guid;
            _Machine = machine;
            _ShiftDate = shiftDate;
            _Shift = shift;
            _Operator = @operator;
            _PartName = partName;
            _Order = order;
            _Setup = setup;
            _FinishedCount = finishedCount;
            _TotalCount = totalCount;
            _StartSetupTime = startSetupTime;
            _StartMachiningTime = startMachiningTime;
            _SetupTimeFact = setupTimeFact;
            _EndMachiningTime = endMachiningTime;
            _SetupTimePlan = setupTimePlan;
            _SetupTimePlanForReport = setupTimePlanForReport;
            _SingleProductionTimePlan = singleProductionTimePlan;
            _ProductionTimeFact = productionTimeFact;
            _MachiningTime = machiningTime;
            _SetupDowntimes = setupDowntimes;
            _MachiningDowntimes = machiningDowntimes;
            _PartialSetupTime = partialSetupTime;
            _MaintenanceTime = maintenanceTime;
            _ToolSearchingTime = toolSearchingTime;
            _MentoringTime = mentoringTime;
            _ContactingDepartmentsTime = contactingDepartmentsTime;
            _FixtureMakingTime = fixtureMakingTime;
            _HardwareFailureTime = hardwareFailureTime;
            _OperatorComment = operatorComment;
            _MasterSetupComment = masterSetupComment;
            _MasterMachiningComment = masterMachiningComment;
            _SpecifiedDowntimesComment = specifiedDowntimesComment;
            _UnspecifiedDowntimesComment = unspecifiedDowntimesComment;
            _MasterComment = masterComment;
            _EngineerComment = engineerComment;
            NeedUpdate = false;
        }

        private Guid _Guid;
        /// <summary> GUID </summary>
        public Guid Guid
        {
            get => _Guid;
            set {
                if (Set(ref _Guid, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private string _Machine;
        /// <summary> Станок </summary>
        public string Machine
        {
            get => _Machine;
            set
            {
                if (Set(ref _Machine, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private DateTime _ShiftDate;
        /// <summary> Дата смены </summary>
        public DateTime ShiftDate
        {
            get => _ShiftDate;
            set
            {
                if (Set(ref _ShiftDate, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _Shift;
        /// <summary> Смена </summary>
        public string Shift
        {
            get => _Shift;
            set
            {
                if (Set(ref _Shift, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private string _Operator;
        /// <summary> Оператор </summary>
        public string Operator
        {
            get => _Operator;
            set
            {
                if (Set(ref _Operator, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _PartName;
        /// <summary> Название детали </summary>
        public string PartName
        {
            get => _PartName;
            set
            {
                if (Set(ref _PartName, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _Order;
        /// <summary> Номер маршрутного листа </summary>
        public string Order
        {
            get => _Order;
            set {
                if (Set(ref _Order, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private int _Setup;
        /// <summary> Номер установки </summary>
        public int Setup
        {
            get => _Setup;
            set {
                if (Set(ref _Setup, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private int _FinishedCount;
        /// <summary> Изготовлено </summary>
        public int FinishedCount
        {
            get => _FinishedCount;
            set {
                if (Set(ref _FinishedCount, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
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
            set {
                if (Set(ref _TotalCount, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private DateTime _StartSetupTime;
        /// <summary> Начало наладки </summary>
        public DateTime StartSetupTime
        {
            get => _StartSetupTime;
            set {
                if (Set(ref _StartSetupTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private DateTime _StartMachiningTime;
        /// <summary> Завершение наладки / начало изготовления </summary>
        public DateTime StartMachiningTime
        {
            get => _StartMachiningTime;
            set {
                if (Set(ref _StartMachiningTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private double _SetupTimeFact;
        /// <summary> Фактическое время наладки </summary>
        public double SetupTimeFact
        {
            get => _SetupTimeFact;
            set
            {
                if (Set(ref _SetupTimeFact, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                    OnPropertyChanged(nameof(SetupRatio));
                    OnPropertyChanged(nameof(SetupRatioTitle));
                }
                
            }
        }

        private DateTime _EndMachiningTime;
        /// <summary> Завершение изготовления </summary>
        public DateTime EndMachiningTime
        {
            get => _EndMachiningTime;
            set {
                if (Set(ref _EndMachiningTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }

        private double _SetupTimePlan;
        /// <summary> Норматив наладки </summary>
        public double SetupTimePlan
        {
            get => _SetupTimePlan;
            set
            {
                if(Set(ref _SetupTimePlan, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                    OnPropertyChanged(nameof(SetupRatio));
                    OnPropertyChanged(nameof(SetupRatioTitle));
                }
            }
        }


        private double _SetupTimePlanForReport;
        /// <summary> Норматив наладки для отчета </summary>
        public double SetupTimePlanForReport
        {
            get => _SetupTimePlanForReport;
            set {
                if (Set(ref _SetupTimePlanForReport, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _SingleProductionTimePlan;
        /// <summary> Штучный норматив </summary>
        public double SingleProductionTimePlan
        {
            get => _SingleProductionTimePlan;
            set {
                if (Set(ref _SingleProductionTimePlan, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _ProductionTimeFact;
        /// <summary> Фасктическое время изготовления </summary>
        public double ProductionTimeFact
        {
            get => _ProductionTimeFact;
            set {
                if (Set(ref _ProductionTimeFact, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private TimeSpan _MachiningTime;
        /// <summary> Машинное время </summary>
        public TimeSpan MachiningTime
        {
            get => _MachiningTime;
            set {
                if (Set(ref _MachiningTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _SetupDowntimes;
        /// <summary> Время простоев в наладке </summary>
        public double SetupDowntimes
        {
            get => _SetupDowntimes;
            set {
                if (Set(ref _SetupDowntimes, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _MachiningDowntimes;
        /// <summary> Время простоев в изготовлении </summary>
        public double MachiningDowntimes
        {
            get => _MachiningDowntimes;
            set {
                if (Set(ref _MachiningDowntimes, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _PartialSetupTime;
        /// <summary> Время простоя "Частичная наладка" </summary>
        public double PartialSetupTime
        {
            get => _PartialSetupTime;
            set {
                if (Set(ref _PartialSetupTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _MaintenanceTime;
        /// <summary> Время простоя "Обслуживание" </summary>
        public double MaintenanceTime
        {
            get => _MaintenanceTime;
            set {
                if (Set(ref _MaintenanceTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _ToolSearchingTime;
        /// <summary> Время простоя "Поиск и получение инструмента" </summary>
        public double ToolSearchingTime
        {
            get => _ToolSearchingTime;
            set {
                if (Set(ref _ToolSearchingTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _MentoringTime;
        /// <summary> Время простоя "Помощь / обучение" </summary>
        public double MentoringTime
        {
            get => _MentoringTime;
            set {
                if (Set(ref _MentoringTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _ContactingDepartmentsTime;
        /// <summary> Время простоя "Обращение в другие службы" </summary>
        public double ContactingDepartmentsTime
        {
            get => _ContactingDepartmentsTime;
            set {
                if (Set(ref _ContactingDepartmentsTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _FixtureMakingTime;
        /// <summary> Время простоя "Изготовление оснастки и калибров" </summary>
        public double FixtureMakingTime
        {
            get => _FixtureMakingTime;
            set {
                if (Set(ref _FixtureMakingTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private double _HardwareFailureTime;
        /// <summary> Время простоя "Отказ оборудования" </summary>
        public double HardwareFailureTime
        {
            get => _HardwareFailureTime;
            set {
                if (Set(ref _HardwareFailureTime, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _OperatorComment;
        /// <summary> Комментарий оператора </summary>
        public string OperatorComment
        {
            get => _OperatorComment;
            set {
                if (Set(ref _OperatorComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _MasterSetupComment;
        /// <summary> Комментарий к отклонениям от нормативов в наладке </summary>
        public string MasterSetupComment
        {
            get => _MasterSetupComment;
            set {
                if (Set(ref _MasterSetupComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _MasterMachiningComment;
        /// <summary> Комментарий к отклонениям от нормативов в изготовлении </summary>
        public string MasterMachiningComment
        {
            get => _MasterMachiningComment;
            set {
                if (Set(ref _MasterMachiningComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _SpecifiedDowntimesComment;
        /// <summary> Комментарий к зарегистрированным простоям </summary>
        public string SpecifiedDowntimesComment
        {
            get => _SpecifiedDowntimesComment;
            set {
                if (Set(ref _SpecifiedDowntimesComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _UnspecifiedDowntimesComment;
        /// <summary> Комментарий к незарегистрированным простоям </summary>
        public string UnspecifiedDowntimesComment
        {
            get => _UnspecifiedDowntimesComment;
            set {
                if (Set(ref _UnspecifiedDowntimesComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _MasterComment;
        /// <summary> Комментарий мастера </summary>
        public string MasterComment
        {
            get => _MasterComment;
            set {
                if (Set(ref _MasterComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private string _EngineerComment;
        /// <summary> Комментарий техотдела </summary>
        public string EngineerComment
        {
            get => _EngineerComment;
            set {
                if (Set(ref _EngineerComment, value))
                {
                    NeedUpdate = true;
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        private bool _NeedUpdate;
        /// <summary> Описание </summary>
        public bool NeedUpdate
        {
            get => _NeedUpdate;
            set { 
                if (Set(ref _NeedUpdate, value))
                {
                    OnPropertyChanged(nameof(NeedUpdate));
                }
            }
        }


        public double SetupRatio => SetupTimePlan / SetupTimeFact;
        public string SetupRatioTitle => SetupRatio is double.NaN or double.PositiveInfinity ? "б/н" : $"{SetupRatio:0%}";
        public double ProductionRatio => FinishedCountFact * SingleProductionTimePlan / ProductionTimeFact;
        public string ProductionRatioTitle => ProductionRatio is double.NaN or double.PositiveInfinity ? "б/и" : $"{ProductionRatio:0%}";
    }
}
