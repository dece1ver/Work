using libeLog;
using libeLog.Base;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Extensions;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Part = remeLog.Models.Part;

namespace remeLog.ViewModels
{
    public class DailyReportWindowViewModel : ViewModel
    {
        public DailyReportWindowViewModel((ICollection<Part> parts, DateTime date, string machine) shiftInfo)
        {
            _Status = string.Empty;
            _Parts = shiftInfo.parts.ToList();
            _ShiftDate = shiftInfo.date;
            _Machine = shiftInfo.machine;
            _DayDowntimesReason = string.Empty;
            _NightDowntimesReason = string.Empty;
            _DayMasterComment = string.Empty;
            _NightMasterComment = string.Empty;
            _CurrentMaster = string.Empty;
            Database.ReadMasters(out _Masters);

            var readDayDbResult = Database.ReadShiftInfo(new ShiftInfo(ShiftDate, ShiftType.Day, _Machine), out var dbDayShfts);
            switch (readDayDbResult)
            {
                case not libeLog.Models.DbResult.Ok:
                    MessageBox.Show("Не удалось получить доступ к дневным сменам в базе данных, внесение новой информации может привести к потере изначальных данных.", "Сообщите разработчику.", MessageBoxButton.OK, MessageBoxImage.Warning); ;
                    break;
            }
            if (dbDayShfts is { Count: 1})
            {
                // делать ли отдельное поле для читаемого простоя из бд на случай разницы?
                var shift = dbDayShfts.First();
                _CurrentMaster = shift.Master;
                _DayDowntimesReason = shift.DowntimesComment;
                _DayMasterComment = shift.CommonComment;
            }
            var readNightDbResult = Database.ReadShiftInfo(new ShiftInfo(ShiftDate, ShiftType.Night, _Machine), out var dbNightShfts);
            switch (readNightDbResult)
            {
                case not libeLog.Models.DbResult.Ok:
                    MessageBox.Show("Не удалось получить доступ к ночным сменам в базе данных, внесение новой информации может привести к потере изначальных данных.", "Сообщите разработчику.", MessageBoxButton.OK, MessageBoxImage.Warning); ;
                    break;
            }
            if (dbNightShfts is { Count: 1 })
            {
                // делать ли отдельное поле для читаемого простоя из бд на случай разницы?
                var shift = dbNightShfts.First();
                _CurrentMaster = shift.Master;
                _NightDowntimesReason = shift.DowntimesComment;
                _NightMasterComment = shift.CommonComment;
            }

            UpdateShiftInfoCommand = new LambdaCommand(OnUpdateShiftInfoCommandExecuted, CanUpdateShiftInfoCommandExecute);
        }

        private DateTime _ShiftDate;
        /// <summary> Дата смены </summary>
        public DateTime ShiftDate
        {
            get => _ShiftDate;
            set => Set(ref _ShiftDate, value);
        }

        private List<Part> _Parts;
        /// <summary> Детали </summary>
        public List<Part> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }


        private string _Machine;
        /// <summary> Описание </summary>
        public string Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }


        private List<string> _Masters;
        /// <summary> Мастера </summary>
        public List<string> Masters
        {
            get => _Masters;
            set => Set(ref _Masters, value);
        }


        private string _CurrentMaster;
        /// <summary> Текущий мастер </summary>
        public string CurrentMaster
        {
            get => _CurrentMaster;
            set => Set(ref _CurrentMaster, value);
        }

        private string _Status;
        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }


        private string _DayDowntimesReason;
        /// <summary> Описание </summary>
        public string DayDowntimesReason
        {
            get => _DayDowntimesReason;
            set => Set(ref _DayDowntimesReason, value);
        }

        private string _NightDowntimesReason;
        /// <summary> Описание </summary>
        public string NightDowntimesReason
        {
            get => _NightDowntimesReason;
            set => Set(ref _NightDowntimesReason, value);
        }


        private string _DayMasterComment;
        /// <summary> Описание </summary>
        public string DayMasterComment
        {
            get => _DayMasterComment;
            set => Set(ref _DayMasterComment, value);
        }

        private string _NightMasterComment;
        /// <summary> Описание </summary>
        public string NightMasterComment
        {
            get => _NightMasterComment;
            set => Set(ref _NightMasterComment, value);
        }

        public bool DayDowntimesNeedAttention => UnspecifiedDayDowntimesRatio is > 0.1 or < -0.1;
        public bool NightDowntimesNeedAttention => UnspecifiedNightDowntimesRatio is > 0.1 or < -0.1;

        public List<Part> DayParts => Parts.Where(p => p.Shift == "День").ToList();
        public List<Part> NightParts => Parts.Where(p => p.Shift == "Ночь").ToList();
        public int DayOrders => Parts.Where(p => p.Shift == new Shift(ShiftType.Day).Name).GroupBy(p => p.Order).Count();
        public int NightOrders => Parts.Where(p => p.Shift == new Shift(ShiftType.Night).Name).GroupBy(p => p.Order).Count();
        public int DayFinishedCount => Parts.Where(p => p.Shift == new Shift(ShiftType.Day).Name).Sum(p => p.FinishedCount);
        public int NightFinishedCount => Parts.Where(p => p.Shift == new Shift(ShiftType.Night).Name).Sum(p => p.FinishedCount);
        public int DaySetups => Parts.Count(p => p.StartSetupTime != p.StartMachiningTime && p.Shift == new Shift(ShiftType.Day).Name);
        public int NightSetups => Parts.Count(p => p.StartSetupTime != p.StartMachiningTime && p.Shift == new Shift(ShiftType.Night).Name);
        public string DayOperator => DayParts.Count > 0 ? DayParts.First().Operator : "Нет оператора.";
        public string NightOperator => NightParts.Count > 0 ? NightParts.First().Operator : "Нет оператора.";

        public string DayWorkInfo => $"М/Л: {DayOrders}, Наладок: {DaySetups}, Деталей: {DayFinishedCount}";
        public string NightWorkInfo => $"М/Л: {NightOrders}, Наладок: {NightSetups}, Деталей: {NightFinishedCount}";

        public double UnspecifiedDayDowntimes => Parts.UnspecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Day);
        public double UnspecifiedNightDowntimes => Parts.UnspecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Night);
        public double SpecifiedDayDowntimes => Parts.SpecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Day);
        public double SpecifiedNightDowntimes => Parts.SpecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Night);
        public double UnspecifiedDayDowntimesRatio => Parts.UnspecifiedDowntimesRatio(ShiftDate, ShiftDate, ShiftType.Day);
        public double UnspecifiedNightDowntimesRatio => Parts.UnspecifiedDowntimesRatio(ShiftDate, ShiftDate, ShiftType.Night);
        public double SpecifiedDayDowntimesRatio => Parts.SpecifiedDowntimesRatio(ShiftDate, ShiftDate, ShiftType.Day);
        public double SpecifiedNightDowntimesRatio => Parts.SpecifiedDowntimesRatio(ShiftDate, ShiftDate, ShiftType.Night);
        public double TotalWorkedDayTime => (int)ShiftType.Day - Parts.UnspecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Day);
        public double TotalWorkedNightTime => (int)ShiftType.Night - Parts.UnspecifiedDowntimes(ShiftDate, ShiftDate, ShiftType.Night);

        #region UpdateShiftInfo
        public ICommand UpdateShiftInfoCommand { get; }
        private void OnUpdateShiftInfoCommandExecuted(object p)
        {
            if (MessageBox.Show("Обновить информацию?", "Вы точно уверены?", MessageBoxButton.YesNo, MessageBoxImage.Question) is MessageBoxResult.No) 
                return;
            var dayShift = new ShiftInfo(null, ShiftDate, ShiftType.Day, Machine, CurrentMaster, UnspecifiedDayDowntimes, DayDowntimesReason, DayMasterComment);
            var nightShift = new ShiftInfo(null, ShiftDate, ShiftType.Night, Machine, CurrentMaster, UnspecifiedNightDowntimes, NightDowntimesReason, NightMasterComment);
            var dayWriteResult = Database.WriteShiftInfo(dayShift);
            switch (dayWriteResult)
            {
                case libeLog.Models.DbResult.Ok:
                    Status = "Информация о дневной смене записана.";
                    break;
                case libeLog.Models.DbResult.AuthError:
                    Status = "Ошибка авторизации.";
                    break;
                case libeLog.Models.DbResult.Error:
                    Status = "Ошибка авторизации.";
                    break;
                case libeLog.Models.DbResult.NoConnection:
                    Status = "Нет соединения с БД.";
                    break;
            }
            var nightWriteResult = Database.WriteShiftInfo(nightShift);
            switch (nightWriteResult)
            {
                case libeLog.Models.DbResult.Ok when dayWriteResult is libeLog.Models.DbResult.Ok:
                    Status = "Информация о сменах записана.";
                    break;
                case libeLog.Models.DbResult.Ok:
                    Status = "Информация о ночной смене записана.";
                    break;
                case libeLog.Models.DbResult.AuthError:
                    Status = "Ошибка авторизации.";
                    break;
                case libeLog.Models.DbResult.Error:
                    Status = "Ошибка БД.";
                    break;
                case libeLog.Models.DbResult.NoConnection:
                    Status = "Нет соединения с БД.";
                    break;
            }
        }
        private static bool CanUpdateShiftInfoCommandExecute(object p) => true;
        #endregion

    }
}
