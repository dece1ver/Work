using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class PartInfoModel
    {
        /// <summary> Наименование </summary>
        public string Name { get; init; }

        /// <summary> Обозначение </summary>
        public string Number { get; init; }

        /// <summary> Заказ </summary>
        public string Order { get; set; }

        /// <summary> Текущий установ </summary>
        public int Setup { get; set; }

        /// <summary> Количество </summary>
        public int PartsCount { get; set; }

        /// <summary> Плановое время наладки </summary>
        public double SetupTimePlan { get; init; }

        /// <summary> Плановое штучное время </summary>
        public double MachineTimePlan { get; init; }

        public DateTime StartSetupTime { get; set; }
        public DateTime EndSetupTime { get; set; }

        public DateTime StartMachiningTime { get; set; }
        public DateTime EndMachiningTime { get; set; }

        /// <summary> Фактическое время наладки </summary>
        public TimeSpan SetupTimeFact => EndSetupTime - StartSetupTime;

        public string FullName => $"{Name} {Number}";

        public double FullTimePlan => SetupTimePlan + PartsCount * MachineTimePlan;

        /// <summary>Фактическое машинное время </summary>
        public TimeSpan MachineTimeFact => EndMachiningTime - StartMachiningTime;

        /// <summary>
        /// Информация о детали
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <param name="number">Обозначение</param>
        /// <param name="setup">Текущая установка</param>
        /// <param name="order">Заказ</param>
        /// <param name="partsCount">Количество</param>
        /// <param name="setupTimePlan">Плановое время наладки</param>
        /// <param name="machineTimePlan">Плановое штучное время</param>

        public PartInfoModel(string name, string number, int setup, string order, int partsCount, double setupTimePlan, double machineTimePlan)
        {
            Name = name;
            Number = number;
            Setup = setup;
            Order = order;
            PartsCount = partsCount;
            SetupTimePlan = setupTimePlan;
            MachineTimePlan = machineTimePlan;
        }
    }
}
