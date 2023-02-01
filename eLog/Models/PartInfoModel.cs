using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace eLog.Models
{
    /// <summary>
    /// Информация о детали.
    /// Статусные свойства выводятся из временных свойств. По-умолчанию завершающие временные свойства присваиваются в DateTime.MinValue. После присваивания времен деталь считается завершенной.
    /// </summary>
    public class PartInfoModel : ViewModel
    {
        private string _Name;
        private string _Number;
        private string _Order;
        private int _Setup;
        private int _PartsCount;
        private int _PartsFinished;
        private readonly double _SetupTimePlan;
        private readonly double _MachineTimePlan;
        private TimeSpan _MachineTime;
        private DateTime _StartSetupTime;
        private DateTime _StartMachiningTime;
        private DateTime _EndMachiningTime;

        /// <summary> Наименование </summary>
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }

        /// <summary> Обозначение </summary>
        public string Number
        {
            get => _Number;
            set => Set(ref _Number, value);
        }

        /// <summary> Заказ </summary>
        public string Order
        {
            get => _Order;
            set => Set(ref _Order, value);
        }

        /// <summary> Текущий установ </summary>
        public int Setup
        {
            get => _Setup;
            set => Set(ref _Setup, value);
        }

        /// <summary> Количество по заказу</summary>
        public int PartsCount
        {
            get => _PartsCount;
            set => Set(ref _PartsCount, value);
        }

        /// <summary> Количество выпущено</summary>
        public int PartsFinished
        {
            get => _PartsFinished;
            set
            {
                Set(ref _PartsFinished, value);
                OnPropertyChanged(nameof(PartsCountInfo));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(EndDetailInfo));
            }
        }

        /// <summary> Описание количества деталей </summary>
        public string PartsCountInfo => PartsFinished > 0 ? $"{PartsFinished} / {PartsCount} шт" : $"{PartsCount} шт";

        /// <summary> Плановое время наладки. Может быть присвоено только при инициализации (предполагается получение из БД). </summary>
        public double SetupTimePlan
        {
            get => _SetupTimePlan;
            init => Set(ref _SetupTimePlan, value);
        }

        /// <summary> Плановое штучное время. Может быть присвоено только при инициализации (предполагается получение из БД). </summary>
        public double MachineTimePlan
        {
            get => _MachineTimePlan;
            init => Set(ref _MachineTimePlan, value);
        }

        /// <summary> Фактическое машинное время, фиксируемое оператором. В расчетах выработки не участвует </summary>
        public TimeSpan MachineTime
        {
            get => _MachineTime;
            set => Set(ref _MachineTime, value);
        }

        /// <summary> Id присваивается после записи в таблицу. Нужен для поиска в таблице при редактировании.</summary>
        public int Id { get; set; }

        /// <summary> Время начала наладки </summary>
        public DateTime StartSetupTime
        {
            get => _StartSetupTime;
            set
            {
                Set(ref _StartSetupTime, value);
                OnPropertyChanged(nameof(EndSetupInfo));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(SetupIsNotFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(CanBeFinished));
            }
        }

        /// <summary> Время завершения наладки / время начала изготовления </summary>
        public DateTime StartMachiningTime
        {
            get => _StartMachiningTime;
            set
            {
                Set(ref _StartMachiningTime, value);
                OnPropertyChanged(nameof(EndSetupInfo));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(SetupIsNotFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(CanBeFinished));
            }
        }

        /// <summary> Время завершения изготовления </summary>
        public DateTime EndMachiningTime
        {
            get => _EndMachiningTime;
            set
            {
                Set(ref _EndMachiningTime, value);
                OnPropertyChanged(nameof(CanBeFinished));
                OnPropertyChanged(nameof(IsFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(PartsCountInfo));
                OnPropertyChanged(nameof(EndDetailInfo));
            }
        }

        /// <summary> Информация об окончании наладки </summary>
        public string EndSetupInfo =>
            StartMachiningTime == DateTime.MinValue 
                ? $"{StartSetupTime.AddMinutes(SetupTimePlan):dd.MM.yyyy HH:mm} (плановое, норматив {SetupTimePlan} мин.)" 
                : $"{StartMachiningTime:dd.MM.yyyy HH:mm}{(SetupTimeFact == TimeSpan.Zero ? string.Empty : $" ({(SetupTimePlan / SetupTimeFact.TotalMinutes * 100):N0}%)")}";

        /// <summary> Информация о завершении изготовления </summary>
        public string EndDetailInfo
        {
            get
            {
                if (EndMachiningTime == DateTime.MinValue)
                {
                    var setup = SetupTimeFact.TotalMinutes > 0 ? SetupTimeFact.TotalMinutes : SetupTimePlan;
                    return $"{StartSetupTime.AddMinutes(setup).AddMinutes(PartsCount * MachineTimePlan):dd.MM.yyyy HH:mm} (плановое, норматив {PartsCount} шт по {MachineTimePlan} мин.)";
                }
                return $"{EndMachiningTime:dd.MM.yyyy HH:mm}{(MachineTimeFact == TimeSpan.Zero ? string.Empty : $" ({MachineTimePlan * PartsFinished / MachineTimeFact.TotalMinutes * 100:N0}%)")}";
            }
        }

        /// <summary> Фактическое время наладки </summary>
        public TimeSpan SetupTimeFact => StartMachiningTime - StartSetupTime;

        /// <summary> Полное название детали (наименование + обозначение) </summary>
        public string FullName => $"{Name} {Number}";

        /// <summary>Фактическое машинное время </summary>
        public TimeSpan MachineTimeFact => EndMachiningTime - StartMachiningTime;

        /// <summary>
        /// Может ли быть завершена деталь.
        /// True если время начала изготовления больше или равно времени начала наладки и если время начала изготовления раньше текущего.
        /// </summary>
        public bool CanBeFinished => StartSetupTime <= StartMachiningTime && DateTime.Now > StartMachiningTime;

        /// <summary>
        /// Завершено ли изготовление детали.
        /// True если время завершения изготовления больше, чем время начала наладки и если завершена наладка (SetupIsFinished == true).
        /// </summary>
        public bool IsFinished => EndMachiningTime > StartMachiningTime && SetupIsFinished;

        /// <summary>
        /// Завершена ли наладка.
        /// True если время завершения наладки больше или равно времени начала наладки.
        /// </summary>
        public bool SetupIsFinished => StartMachiningTime >= StartSetupTime;

        /// <summary>
        /// Не завершена ли наладка. Нужно для привязок разметки.
        /// Инвертированное значение свойства SetupIsFinished.
        /// </summary>
        public bool SetupIsNotFinished => !SetupIsFinished;

        /// <summary>
        /// Не завершена ли наладка. Нужно для привязок разметки.
        /// Инвертированное значение свойства IsFinished.
        /// </summary>
        public bool IsStarted => !IsFinished;

        /// <summary>
        /// Информация о детали.
        /// Статусные свойства выводятся из временных свойств. По-умолчанию завершающие временные свойства присваиваются в DateTime.MinValue. После присваивания времен деталь считается завершенной.
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
            Id = -1;
        }
    }
}
