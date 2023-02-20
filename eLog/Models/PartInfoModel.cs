using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private int _TotalCount;
        private string _Shift;
        private int _FinishedCount;
        private double _SetupTimePlan;
        private double _SingleProductionTimePlan;
        private TimeSpan _MachineTime;
        private DateTime _StartSetupTime;
        private DateTime _StartMachiningTime;
        private DateTime _EndMachiningTime;
        private ObservableCollection<DownTime> _DownTimes;
        private int _Id;
        private bool _IsSynced;
        private string _OperatorComments;

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
        public int TotalCount
        {
            get => _TotalCount;
            set => Set(ref _TotalCount, value);
        }

        /// <summary> Смена </summary>
        public string Shift
        {
            get => _Shift;
            set => Set(ref _Shift, value);
        }

        /// <summary> Количество выпущено</summary>
        public int FinishedCount
        {
            get => _FinishedCount;
            set
            {
                Set(ref _FinishedCount, value);
                OnPropertyChanged(nameof(TotalCountInfo));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(InProduction));
                OnPropertyChanged(nameof(EndDetailInfo));
            }
        }

        /// <summary> Описание количества деталей </summary>
        public string TotalCountInfo => $"{FinishedCount} / {TotalCount} шт";

        /// <summary> Плановое время наладки. Может быть присвоено только при инициализации (предполагается получение из БД). </summary>
        public double SetupTimePlan
        {
            get => _SetupTimePlan;
            set => Set(ref _SetupTimePlan, value);
        }

        /// <summary> Плановое штучное время. Может быть присвоено только при инициализации (предполагается получение из БД). </summary>
        public double SingleProductionTimePlan
        {
            get => _SingleProductionTimePlan;
            set => Set(ref _SingleProductionTimePlan, value);
        }

        /// <summary> Фактическое машинное время, фиксируемое оператором. В расчетах выработки не участвует </summary>
        public TimeSpan MachineTime
        {
            get => _MachineTime;
            set
            {
                Set(ref _MachineTime, value);
            }
        }

        /// <summary> Список простоев </summary>
        public ObservableCollection<DownTime> DownTimes
        {
            get => _DownTimes;
            set => Set(ref _DownTimes, value);
        }

        /// <summary> Id присваивается после записи в таблицу. Нужен для поиска в таблице при редактировании.</summary>
        public int Id
        {
            get => _Id;
            set
            {
                Set(ref _Id, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary> Время начала наладки </summary>
        public DateTime StartSetupTime
        {
            get => _StartSetupTime;
            set
            {
                Set(ref _StartSetupTime, value);
                OnPropertyChanged(nameof(EndSetupInfo));
                OnPropertyChanged(nameof(SetupIsNotFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(CanBeFinished));
                OnPropertyChanged(nameof(InProduction));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(Title));
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
                OnPropertyChanged(nameof(SetupIsNotFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(CanBeFinished));
                OnPropertyChanged(nameof(InProduction));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary> Время завершения изготовления </summary>
        public DateTime EndMachiningTime
        {
            get => _EndMachiningTime;
            set
            {
                Set(ref _EndMachiningTime, value);
                OnPropertyChanged(nameof(SetupIsNotFinished));
                OnPropertyChanged(nameof(CanBeFinished));
                OnPropertyChanged(nameof(IsFinished));
                OnPropertyChanged(nameof(IsStarted));
                OnPropertyChanged(nameof(InProduction));
                OnPropertyChanged(nameof(TotalCountInfo));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(Title));
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
                if (FullProductionTimeFact.TotalMinutes <= 0)
                {
                    var setup = SetupTimeFact.TotalMinutes > 0 ? SetupTimeFact.TotalMinutes : SetupTimePlan;
                    return $"{StartSetupTime.AddMinutes(setup).AddMinutes(TotalCount * SingleProductionTimePlan):dd.MM.yyyy HH:mm} (плановое, норматив {TotalCount} шт по {SingleProductionTimePlan} мин.)";
                }
                return $"{EndMachiningTime:dd.MM.yyyy HH:mm}{(FullProductionTimeFact == TimeSpan.Zero ? string.Empty : $" ({SingleProductionTimePlan * FinishedCount / FullProductionTimeFact.TotalMinutes * 100:N0}%)")}";
            }
        }

        /// <summary> Фактическое время наладки </summary>
        public TimeSpan SetupTimeFact => StartMachiningTime - StartSetupTime;

        /// <summary> Полное название детали (наименование + обозначение) </summary>
        public string FullName => $"{Name} {Number}".Trim();

        public string Title
        {
            get
            {
                if (!IsFinished) return FullName;
                var symbol = IsSynced ? "✓" : "🗘";
                return $"{FullName} {symbol}".Trim();
            }
        }

        public string OperatorComments
        {
            get => _OperatorComments;
            set => Set(ref _OperatorComments, value);
        }

        /// <summary>Фактическое время изготовления </summary>
        public TimeSpan FullProductionTimeFact => EndMachiningTime - StartMachiningTime;

        /// <summary>
        /// Может ли быть завершена деталь.
        /// True если время начала изготовления больше или равно времени начала наладки и если время начала изготовления раньше текущего.
        /// </summary>
        public bool CanBeFinished => StartSetupTime <= StartMachiningTime && DateTime.Now > StartMachiningTime;

        /// <summary>
        /// Завершено ли изготовление детали.
        /// True если время завершения изготовления больше, чем время начала наладки, также если завершена наладка (SetupIsFinished == true) и количество выпущенных деталей больше 0.
        /// </summary>
        public bool IsFinished => EndMachiningTime > StartMachiningTime && SetupIsFinished && FinishedCount > 0 && MachineTime > TimeSpan.Zero;

        /// <summary>
        /// Завершена ли наладка.
        /// True если время завершения наладки больше или равно времени начала наладки.
        /// </summary>
        public bool SetupIsFinished => StartMachiningTime >= StartSetupTime;

        /// <summary>
        /// Идет ли изготовление.
        /// True если время завершена наладка, но не завершено изготовление.
        /// </summary>
        public bool InProduction => SetupIsFinished && !IsFinished;

        /// <summary>
        /// Не завершена ли наладка. Нужно для привязок разметки.
        /// Инвертированное значение свойства SetupIsFinished.
        /// </summary>
        public bool SetupIsNotFinished => !SetupIsFinished;

        /// <summary>
        /// Не завершено ли изготовление. Нужно для привязок разметки.
        /// Инвертированное значение свойства IsFinished.
        /// </summary>
        public bool IsStarted => !IsFinished;

        /// <summary>
        /// Записана ли актуальная информация о детали в таблицу
        /// </summary>
        public bool IsSynced
        {
            get => _IsSynced;
            set
            {
                Set(ref _IsSynced, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Информация о детали.
        /// Статусные свойства выводятся из временных свойств. По-умолчанию завершающие временные свойства присваиваются в DateTime.MinValue. После присваивания времен деталь считается завершенной.
        /// </summary>
        /// <param name="name">Наименование</param>
        /// <param name="number">Обозначение</param>
        /// <param name="setup">Текущая установка</param>
        /// <param name="order">Заказ</param>
        /// <param name="totalCount">Количество</param>
        /// <param name="setupTimePlan">Плановое время наладки</param>
        /// <param name="singleProductionTimePlan">Плановое штучное время</param>
        public PartInfoModel(string name, string number, int setup, string order, int totalCount, double setupTimePlan, double singleProductionTimePlan)
        {
            _Name = name;
            _Number = number;
            _Setup = setup;
            _Order = order;
            FinishedCount = -1;
            TotalCount = totalCount;
            SetupTimePlan = setupTimePlan;
            SingleProductionTimePlan = singleProductionTimePlan;
            Id = -1;
            _DownTimes = new ObservableCollection<DownTime>();
            _OperatorComments = string.Empty;
            _Shift = string.Empty;
        }

        public PartInfoModel()
        {
            _Name = string.Empty;
            _Number = string.Empty;
            _Order = string.Empty;
            StartSetupTime = DateTime.Now;
            _DownTimes = new ObservableCollection<DownTime>();
            Id = -1;
            _OperatorComments = string.Empty;
            _Shift = string.Empty;
        }
    }
}
