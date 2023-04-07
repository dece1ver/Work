using eLog.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using Newtonsoft.Json;

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
        private byte _Setup;
        private int _TotalCount;
        private string _Shift;
        private int _FinishedCount;
        private double _SetupTimePlan;
        private double _SingleProductionTimePlan;
        private Operator _Operator;
        private TimeSpan _MachineTime;
        private DateTime _StartSetupTime;
        private DateTime _StartMachiningTime;
        private DateTime _EndMachiningTime;
        private ObservableCollection<DownTime> _DownTimes;
        private int _Id;
        private bool _IsSynced;
        private string _OperatorComments;

        public enum State { Finished, PartialSetup, InProgress }

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
        public byte Setup
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

        /// <summary> Оператор </summary>
        public Operator Operator
        {
            get => _Operator;
            set => Set(ref _Operator, value);
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
        [JsonIgnore]
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
            set
            {
                if (Set(ref _DownTimes, value))
                {
                    OnPropertyChanged(nameof(DownTimesIsClosed));
                }
            }
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
                OnPropertyChanged(nameof(IsFinished));
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
                OnPropertyChanged(nameof(EndSetupInfo));
                OnPropertyChanged(nameof(EndDetailInfo));
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary> Информация об окончании наладки </summary>
        [JsonIgnore]
        public string EndSetupInfo
        {
            get
            {
                var endSetupTime = SetupIsFinished ? StartMachiningTime : StartSetupTime
                    .AddMinutes(SetupTimePlan)
                    .Add(Util.GetBreaksBetween(StartSetupTime, StartSetupTime.AddMinutes(SetupTimePlan), false));
                if (StartSetupTime == StartMachiningTime) return "Без наладки";
                var result = endSetupTime > StartSetupTime ? endSetupTime.ToString(Text.DateTimeFormat) : "-";
                if (result == "-") return result;
                var breaks = Util.GetBreaksBetween(StartSetupTime, endSetupTime);
                var breaksInfo = breaks.Ticks > 0 ? $" + перерывы: {breaks.TotalMinutes} мин" : string.Empty;
                var planInfo = SetupTimePlan > 0
                    ? $" (Плановое: норматив {SetupTimePlan} мин{breaksInfo})"
                    : string.Empty;
                var productivity = SetupTimePlan > 0
                    ? $" ({SetupTimePlan / SetupTimeFact.TotalMinutes * 100:N0}%)"
                    : string.Empty;
                switch (IsFinished)
                {
                    case State.Finished:
                        result += productivity;
                        break;
                    case State.PartialSetup:
                        result += " (Неполная наладка)";
                        break;
                    case State.InProgress when FullSetupTimeFact.Ticks > 0:
                        result += productivity;
                        break;
                    case State.InProgress when FullSetupTimeFact.Ticks < 0:
                        result += planInfo;
                        break;
                }
                return $"{result}{(breaks.Ticks > 0 && SetupTimeFact.Ticks > 0 && IsFinished is State.InProgress ? breaksInfo : string.Empty)}";
            }
        }

        /// <summary> Информация о завершении изготовления </summary>
        [JsonIgnore]
        public string EndDetailInfo
        {
            get
            {
                var startMachiningTime = SetupIsFinished
                    ? StartMachiningTime 
                    : StartSetupTime
                        .AddMinutes(SetupTimePlan)
                        .Add(Util.GetBreaksBetween(StartSetupTime, StartSetupTime.AddMinutes(SetupTimePlan), false));

                var endMachiningTime = EndMachiningTime > StartMachiningTime
                    ? EndMachiningTime
                    : startMachiningTime
                        .AddMinutes(TotalCount * SingleProductionTimePlan)
                        .Add(Util.GetBreaksBetween(startMachiningTime, startMachiningTime.AddMinutes(TotalCount * SingleProductionTimePlan)));
                var result = endMachiningTime > startMachiningTime && EndSetupInfo != "-" ? endMachiningTime.ToString(Text.DateTimeFormat) : "-";
                if (result == "-") return result;
                var breaks = Util.GetBreaksBetween(startMachiningTime, endMachiningTime);
                var breaksInfo = breaks.Ticks > 0 ? $" + перерывы: {breaks.TotalMinutes} мин" : string.Empty;
                var planInfo = SingleProductionTimePlan > 0
                    ? $" (Плановое: {TotalCount} шт по {SingleProductionTimePlan} мин{breaksInfo})"
                    : string.Empty;
                var productivity = $" ({FinishedCount * SingleProductionTimePlan / ProductionTimeFact.TotalMinutes * 100:N0}%)";
                switch (IsFinished)
                {
                    case State.Finished:
                        result += productivity;
                        break;
                    case State.InProgress:
                        result += planInfo;
                        break;
                    case State.PartialSetup:
                        result = "Без изготовления";
                        break;
                }
                return $"{result}{(breaks.Ticks > 0 && ProductionTimeFact.Ticks > 0 && IsFinished is not State.InProgress ? breaksInfo : string.Empty)}";
            }
        }

        /// <summary> Фактическое время наладки </summary>
        [JsonIgnore] public TimeSpan FullSetupTimeFact => StartMachiningTime - StartSetupTime;


        /// <summary> Фактическое время наладки с учетом перерывов </summary>
        [JsonIgnore] public TimeSpan SetupTimeFact => FullSetupTimeFact - Util.GetBreaksBetween(StartSetupTime, StartMachiningTime);

        /// <summary> Полное название детали (наименование + обозначение) </summary>
        [JsonIgnore] public string FullName => $"{Name} {Number}".Trim();

        [JsonIgnore]
        public string Title
        {
            get
            {
                if (IsFinished == State.InProgress) return FullName;
                var symbol = IsSynced ? "✓" : "🗘";
                return $"{symbol} {FullName}".Trim();
            }
        }

        public string OperatorComments
        {
            get => _OperatorComments;
            set => Set(ref _OperatorComments, value);
        }

        /// <summary>Фактическое время изготовления </summary>
        [JsonIgnore] public TimeSpan FullProductionTimeFact => EndMachiningTime - StartMachiningTime;

        /// <summary> Время изготовления с учетом перерывов </summary>
        [JsonIgnore]
        public TimeSpan ProductionTimeFact =>
            FullProductionTimeFact - Util.GetBreaksBetween(StartMachiningTime, EndMachiningTime);

        /// <summary>
        /// Может ли быть завершена деталь.
        /// True если время начала изготовления больше или равно времени начала наладки и если время начала изготовления раньше текущего.
        /// </summary>
        [JsonIgnore] public bool CanBeFinished => StartSetupTime <= StartMachiningTime && DateTime.Now > StartMachiningTime;

        /// <summary>
        /// Статус изготовления детали.
        /// Finished если время завершения изготовления больше, чем время начала наладки, также если завершена наладка (SetupIsFinished == true) и количество выпущенных деталей больше 0.
        /// PartialSetup если время завершения изготовления больше равно времени завершения наладки и количество выпущенных деталей 0.
        /// </summary>
        [JsonIgnore]
        public State IsFinished
        {
            get
            {
                if (EndMachiningTime > StartMachiningTime && SetupIsFinished && FinishedCount > 0 &&
                    MachineTime > TimeSpan.Zero) return State.Finished;
                if (EndMachiningTime == StartMachiningTime && SetupIsFinished && FinishedCount == 0) return State.PartialSetup;
                return State.InProgress;
            }
        }

        /// <summary>
        /// Завершена ли наладка.
        /// True если нет незавершенных простоев.
        /// </summary>
        public bool DownTimesIsClosed => LastDownTime is null || !LastDownTime.InProgress;

        /// <summary>
        /// Завершена ли наладка.
        /// True если нет незавершенных простоев.
        /// </summary>
        [JsonIgnore] public DownTime? LastDownTime => DownTimes.LastOrDefault();

        /// <summary>
        /// Завершена ли наладка.
        /// True если время завершения наладки больше или равно времени начала наладки.
        /// </summary>
        [JsonIgnore] public bool SetupIsFinished => StartMachiningTime >= StartSetupTime;

        /// <summary>
        /// Идет ли изготовление.
        /// True если время завершена наладка, но не завершено изготовление.
        /// </summary>
        [JsonIgnore] public bool InProduction => SetupIsFinished && IsFinished == State.InProgress;

        /// <summary>
        /// Не завершена ли наладка. Нужно для привязок разметки.
        /// Инвертированное значение свойства SetupIsFinished.
        /// </summary>
        [JsonIgnore] public bool SetupIsNotFinished => !SetupIsFinished;

        /// <summary>
        /// Не завершено ли изготовление. Нужно для привязок разметки.
        /// Инвертированное значение свойства IsFinished.
        /// </summary>
        [JsonIgnore] public bool IsStarted => IsFinished == State.InProgress;

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
        public PartInfoModel(string name, string number, byte setup, string order, int totalCount, double setupTimePlan, double singleProductionTimePlan)
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
            _Operator = AppSettings.CurrentOperator!;
            _Shift = string.Empty;
        }
    }
}
