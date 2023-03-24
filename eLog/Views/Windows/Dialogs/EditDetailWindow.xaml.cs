using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Infrastructure.Extensions.Windows;
using eLog.Models;
using eLog.Services;
using eLog.Views.Windows.Settings;
using Path = System.IO.Path;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDetailWindow.xaml
    /// </summary>
    public partial class EditDetailWindow : INotifyPropertyChanged, IDataErrorInfo
    {
        public List<PartInfoModel> Parts { get; set; } = new();
        public int PartIndex { get; set; }
        public PartInfoModel Part { get; set; }

        public Visibility KeyboardVisibility
        {
            get => _KeyboardVisibility;
            set
            {
                Set(ref _KeyboardVisibility, value);
                Height = _KeyboardVisibility is Visibility.Visible ? 598 : 475;
            }
        }

        public static string[] OrderMonths => Enumerable.Range(1, 12).Select(x => x.ToString("D2")).ToArray();

        public string OrderQualifier
        {
            get => _OrderQualifier;
            set
            {
                Set(ref _OrderQualifier, value);
                Parts.Clear();
                OnPropertyChanged(nameof(OrderValidation));
                OnPropertyChanged(nameof(NonEmptyOrder));
                OnPropertyChanged(nameof(Part.Order));
                Status = OrderValidation switch
                {
                    OrderValidationTypes.Error => string.Empty,
                    OrderValidationTypes.Empty => $"Изготовление без М/Л",
                    OrderValidationTypes.Valid => $"Выбран заказ: {Part.Order}",
                    _ => Status
                };
                switch (OrderValidation)
                {
                    case OrderValidationTypes.Error when OrderText == Text.WithoutOrderDescription:
                        OrderText = string.Empty;
                        break;
                    case OrderValidationTypes.Error:
                        Status = string.Empty;
                        break;
                    case OrderValidationTypes.Empty:
                        Status = "Изготовление без М/Л";
                        OrderText = Text.WithoutOrderDescription;
                        break;
                    case OrderValidationTypes.Valid:
                        Status = $"Выбран заказ: {Part.Order}";
                        break;
                }
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        public bool NewDetail { get; set; }

        public bool WithSetup
        {
            get => _WithSetup;
            set
            {
                Set(ref _WithSetup, value);
                if (_WithSetup) return;
                Part.StartSetupTime = new DateTime(
                    Part.StartSetupTime.Year,
                    Part.StartSetupTime.Month,
                    Part.StartSetupTime.Day,
                    Part.StartSetupTime.Hour,
                    Part.StartSetupTime.Minute,
                    0, 0);
                StartMachiningTime = StartSetupTime;
                Part.StartMachiningTime = Part.StartSetupTime;
                OnPropertyChanged(nameof(StartMachiningTime));
            }
        }

        public string OrderMonth
        {
            get => _OrderMonth;
            set
            {
                Set(ref _OrderMonth, value);
                Parts.Clear();
                OnPropertyChanged(nameof(OrderValidation));
                OnPropertyChanged(nameof(Part.Order));
                Status = OrderValidation switch
                {
                    OrderValidationTypes.Error => string.Empty,
                    OrderValidationTypes.Empty => $"Изготовление без М/Л",
                    OrderValidationTypes.Valid => $"Выбран заказ: {Part.Order}",
                    _ => Status
                };
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }
        public string OrderText
        {
            get => _OrderText;
            set
            {
                Set(ref _OrderText, value);
                Parts.Clear();
                OnPropertyChanged(nameof(OrderValidation));
                OnPropertyChanged(nameof(Part.Order));
                Status = OrderValidation switch
                {
                    OrderValidationTypes.Error => string.Empty,
                    OrderValidationTypes.Empty => $"Изготовление без М/Л",
                    OrderValidationTypes.Valid => $"Выбран заказ: {Part.Order}",
                    _ => Status
                };
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _Status;
        private string _OrderText;
        private string _OrderMonth;
        private string _OrderQualifier;

        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        public bool CanBeClosed
        {
            get
            {
                if (OrderValidation is OrderValidationTypes.Error || string.IsNullOrWhiteSpace(PartName)) return false;
                if (Part.IsFinished is PartInfoModel.State.Finished) return true;
                var validPlanTimes = (Part.SetupTimePlan > 0 || Part.SetupTimePlan == 0 && PartSetupTimePlan == "-") &&
                                     (Part.SingleProductionTimePlan > 0 || Part.SingleProductionTimePlan == 0 && SingleProductionTimePlan == "-");

                switch (validPlanTimes)
                {
                    // старт наладки
                    case true when Part is { FinishedCount: 0, TotalCount: > 0, StartSetupTime.Ticks: > 0 } && 
                                   string.IsNullOrWhiteSpace(StartMachiningTime) && 
                                   string.IsNullOrWhiteSpace(EndMachiningTime):
                        return true;
                    // старт изготовления
                    case true when Part is { FinishedCount: 0, TotalCount: > 0, SetupIsFinished: true } && string.IsNullOrWhiteSpace(EndMachiningTime):
                        return true;
                    // частичная наладка
                    case true when Part is { FinishedCount: 0, TotalCount: > 0, SetupIsFinished: true } && FinishedCount == "0" && EndMachiningTime == StartMachiningTime:
                        return true;
                    // полная отметка
                    case true when Part is { FinishedCount: > 0, TotalCount: > 0, SetupIsFinished: true, FullProductionTimeFact.Ticks: > 0, MachineTime.Ticks: > 0 }:
                        return true;
                    default:
                        return false;
                }
            }
        }


        public enum OrderValidationTypes {Error, Empty, Valid}

        public OrderValidationTypes OrderValidation
        {
            get
            {
                if (OrderQualifier is Text.WithoutOrderItem)
                {
                    Part.Order = Text.WithoutOrderDescription;
                    return OrderValidationTypes.Empty;
                }

                if (OrderText.Length < 9 ||
                    !char.IsNumber(OrderText[0]) ||
                    !char.IsNumber(OrderText[1]) ||
                    !char.IsNumber(OrderText[2]) ||
                    !char.IsNumber(OrderText[3]) ||
                    !char.IsNumber(OrderText[4]) ||
                    OrderText[5] != '.' ||
                    !char.IsNumber(OrderText[6]) ||
                    !char.IsNumber(OrderText[^1]) ||
                    OrderText.Count(x => x is '.') != 2)
                {
                    Part.Order = string.Empty;
                    return OrderValidationTypes.Error;
                }
                Part.Order = $"{OrderQualifier}-{OrderMonth}/{OrderText}";
                return OrderValidationTypes.Valid;
            }
        }

        public bool NonEmptyOrder => OrderValidation is not OrderValidationTypes.Empty;

        public bool CanIncreaseSetup => Part.Setup < 24;
        public bool CanDecreaseSetup => Part.Setup > 1;

        #region Валидация полного номера М/Л (уже не нужно, оставил на всякий случай)
        //public OrderValidation OrderValidateType
        //{
        //    get
        //    {
        //        if (OrderText is "")
        //        {
        //            return OrderValidationTypes.Empty;
        //        }

        //        if (OrderText.Length < 14 ||
        //            !char.IsLetter(OrderText[0]) ||
        //            !char.IsLetter(OrderText[1]) ||
        //            OrderText[2] != '-' ||
        //            !char.IsNumber(OrderText[3]) ||
        //            !char.IsNumber(OrderText[4]) ||
        //            OrderText[5] != '/' ||
        //            !char.IsNumber(OrderText[6]) ||
        //            !char.IsNumber(OrderText[7]) ||
        //            !char.IsNumber(OrderText[8]) ||
        //            !char.IsNumber(OrderText[9]) ||
        //            !char.IsNumber(OrderText[10]) ||
        //            OrderText[11] != '.' ||
        //            !char.IsNumber(OrderText[12]) ||
        //            OrderText.Split('/')[1].Count(x => x is '.') != 2) return OrderValidationTypes.Error;
        //        Part.Order = OrderText;
        //        return OrderValidationTypes.Valid;
        //    }
        //} 
        #endregion

        #region Обертки требуемых для валидации свойств детали, потому что я хз как отсюда влиять на их сеттеры без влияния на геттеры (никак?).
        public string PartName
        {
            get => Part.Name;
            set
            {
                Part.Name = value;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _StartSetupTime;
        public string StartSetupTime
        {
            get => _StartSetupTime;
            set
            {
                _StartSetupTime = value;
                if (!WithSetup)
                {
                    StartMachiningTime = _StartSetupTime;
                    OnPropertyChanged(nameof(StartMachiningTime));
                }
                Part.StartSetupTime = DateTime.TryParseExact(_StartSetupTime, Text.DateTimeFormat, null, DateTimeStyles.None, out var startSetupTime) 
                    ? startSetupTime 
                    : DateTime.MinValue;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _StartMachiningTime;
        public string StartMachiningTime
        {
            get => _StartMachiningTime;
            set
            {
                _StartMachiningTime = value;
                Part.StartMachiningTime = DateTime.TryParseExact(_StartMachiningTime, Text.DateTimeFormat, null, DateTimeStyles.None, out var startMachiningTime) 
                    ? startMachiningTime 
                    : DateTime.MinValue;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _EndMachiningTime;
        public string EndMachiningTime
        {
            get => _EndMachiningTime;
            set
            {
                _EndMachiningTime = value;
                Part.EndMachiningTime = DateTime.TryParseExact(_EndMachiningTime, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out var endMachiningTime) 
                    ? endMachiningTime 
                    : DateTime.MinValue;
                OnPropertyChanged(nameof(FinishedCount));
                OnPropertyChanged(nameof(MachineTime));
                OnPropertyChanged(nameof(CanBeClosed));
                
            }
        }

        private string _MachineTime;
        public string MachineTime
        {
            get => _MachineTime;
            set
            {
                _MachineTime = value;
                Part.MachineTime = _MachineTime.TimeParse(out var machiningTime) 
                    ? machiningTime 
                    : TimeSpan.Zero;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }


        private string _FinishedCount;
        public string FinishedCount
        {
            get => _FinishedCount;
            set
            {
                _FinishedCount = value;
                Part.FinishedCount = _FinishedCount.GetInt(numberOption: Util.GetNumberOption.OnlyPositive);
                OnPropertyChanged(nameof(EndMachiningTime));
                OnPropertyChanged(nameof(MachineTime));
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _TotalCount;
        public string TotalCount
        {
            get => _TotalCount;
            set
            {
                _TotalCount = value;
                Part.TotalCount = _TotalCount.GetInt(numberOption: Util.GetNumberOption.OnlyPositive);
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _PartSetupTimePlan;
        public string PartSetupTimePlan
        {
            get => _PartSetupTimePlan;
            set
            {
                _PartSetupTimePlan = value;
                Part.SetupTimePlan = _PartSetupTimePlan.GetDouble(numberOption: Util.GetNumberOption.OnlyPositive);
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        private string _SingleProductionTimePlan;
        private Visibility _KeyboardVisibility;
        private bool _WithSetup;

        public string SingleProductionTimePlan
        {
            get => _SingleProductionTimePlan;
            set
            {
                _SingleProductionTimePlan = value;
                Part.SingleProductionTimePlan = _SingleProductionTimePlan.GetDouble(numberOption: Util.GetNumberOption.OnlyPositive); ;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }
        #endregion

        public string Error { get; } = string.Empty;

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case nameof(PartName):
                        if (string.IsNullOrWhiteSpace(Part.Name)) error = Text.ValidationErrors.PartName;
                        break;
                    case nameof(StartSetupTime):
                        if (Part.StartSetupTime == DateTime.MinValue) error = Text.ValidationErrors.StartSetupTime;
                        break;
                    case nameof(StartMachiningTime):
                        if (!string.IsNullOrWhiteSpace(StartMachiningTime) && Part.StartMachiningTime == DateTime.MinValue) error = Text.ValidationErrors.StartMachiningTime;
                        break;
                    case nameof(EndMachiningTime):
                        if ((!string.IsNullOrWhiteSpace(EndMachiningTime) && Part.EndMachiningTime <= Part.StartMachiningTime && FinishedCount != "0") || (string.IsNullOrWhiteSpace(EndMachiningTime) && Part.FinishedCount > 0)) 
                            error = Text.ValidationErrors.EndMachiningTime;
                        break;
                    case nameof(MachineTime):
                        if ((Part.MachineTime == TimeSpan.Zero && !string.IsNullOrWhiteSpace(MachineTime)) ||
                            (Part.MachineTime == TimeSpan.Zero &&
                             (Part.FullProductionTimeFact > TimeSpan.Zero || Part.FinishedCount > 0) &&
                             string.IsNullOrWhiteSpace(MachineTime)))
                            error = Text.ValidationErrors.MachineTime;
                        break;
                    case nameof(FinishedCount):
                        error = Part.FinishedCount switch
                        {
                            0 when string.IsNullOrWhiteSpace(FinishedCount) && Part.FullProductionTimeFact > TimeSpan.Zero => Text.ValidationErrors.FinishedCount,
                            0 when !string.IsNullOrWhiteSpace(FinishedCount) && FinishedCount != "0" => Text.ValidationErrors.FinishedCount,
                            _ => error
                        };
                        break;
                    case nameof(TotalCount):
                        error = Part.TotalCount switch
                        {
                            0 => Text.ValidationErrors.TotalCount,
                            _ => error,
                        };
                        break;
                    case nameof(PartSetupTimePlan):
                        error = Part.SetupTimePlan switch
                        {
                            0 when string.IsNullOrWhiteSpace(PartSetupTimePlan) => Text.ValidationErrors.PartSetupTimePlan,
                            0 when !string.IsNullOrWhiteSpace(PartSetupTimePlan) && PartSetupTimePlan != "-" => Text.ValidationErrors.PartSetupTimePlan,
                            _ => error
                        };
                        break;
                    case nameof(SingleProductionTimePlan):
                        error = Part.SingleProductionTimePlan switch
                        {
                            0 when string.IsNullOrWhiteSpace(SingleProductionTimePlan) => Text.ValidationErrors.SingleProductionTimePlan,
                            0 when !string.IsNullOrWhiteSpace(SingleProductionTimePlan) && SingleProductionTimePlan != "-" => Text.ValidationErrors.SingleProductionTimePlan,
                            _ => error
                        };
                        break;
                    case nameof(OrderText):
                        if (OrderValidation is OrderValidationTypes.Error) error = Text.ValidationErrors.OrderText;
                        break;
                }
                return error;
            }
        }

        public EditDetailWindow(PartInfoModel part, bool newDetail = false)
        {
            NewDetail = newDetail;
            _Status = string.Empty;
            Part = part;
            WithSetup = newDetail || part.StartSetupTime != part.StartMachiningTime;
            var order = Part.Order;
            _OrderText = !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('/')
                ? order.Split('/')[1]
                : Text.WithoutOrderDescription;
            _OrderQualifier =
                !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-')
                    ? order.Split('-')[0]
                    : AppSettings.OrderQualifiers[0];
            _OrderMonth =
                !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-') && order.Contains('/')
                    ? order.Split('-')[1].Split('/')[0]
                    : "01";
            PartName = Part.FullName;

            _FinishedCount = Part.FinishedCount > 0 ? Part.FinishedCount.ToString(CultureInfo.InvariantCulture) : string.Empty;
            _TotalCount = Part.TotalCount > 0 ? Part.TotalCount.ToString(CultureInfo.InvariantCulture) : string.Empty;

            _StartSetupTime = Part.StartSetupTime.ToString(Text.DateTimeFormat);
            _StartMachiningTime = Part.StartMachiningTime != DateTime.MinValue ? Part.StartMachiningTime.ToString(Text.DateTimeFormat) : string.Empty;
            _EndMachiningTime = Part.EndMachiningTime != DateTime.MinValue && Part.EndMachiningTime != Part.StartMachiningTime ? Part.EndMachiningTime.ToString(Text.DateTimeFormat) : string.Empty;
            _MachineTime = Part.MachineTime != TimeSpan.Zero ? Part.MachineTime.ToString(Text.TimeSpanFormat) : string.Empty;

            _PartSetupTimePlan = Part.SetupTimePlan > 0 
                ? Part.SetupTimePlan.ToString(CultureInfo.InvariantCulture) 
                : newDetail ? string.Empty : "-";
            _SingleProductionTimePlan = Part.SingleProductionTimePlan > 0 
                ? Part.SingleProductionTimePlan.ToString(CultureInfo.InvariantCulture) 
                : newDetail ? string.Empty : "-";

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(AppSettings.LocalOrdersFile))
            {
                Status =
                    $"Список заказов обновлен {File.GetLastWriteTime(AppSettings.LocalOrdersFile).ToString(Text.DateTimeFormat)}";
            }
            OnPropertyChanged(nameof(NonEmptyOrder));
            KeyboardVisibility = Visibility.Collapsed;
            var updaterThread = new Thread(UpdateOrders) {IsBackground = true};
            updaterThread.Start();
        }

        /// <summary> Реализация поиска номенклатуры по номеру М/Л (имитация) </summary>
        private void FindOrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (OrderValidation)
            {
                case OrderValidationTypes.Valid:
                    Status = "Поиск заказов...";
                    if (Parts.Count == 0) Parts = Part.Order.GetPartsFromOrder();
                    if (PartIndex > Parts.Count - 1) PartIndex = 0;

                    switch (Parts.Count)
                    {
                        case 0:
                            Status = $"По номеру {Part.Order} заказы не найдены.";
                            return;
                        case 1:
                            Status = $"По номеру {Part.Order} найден заказ.";
                            PartName = Parts[0].Name;
                            //Part.Number = Parts[0].Number;
                            TotalCount = Parts[0].TotalCount.ToString(CultureInfo.InvariantCulture);
                            break;
                        case > 1:
                            Status = $"По номеру {Part.Order} найдено несколько заказов: {Parts.Count}. Переключение на кнопку поиска.";
                            PartName = Parts[PartIndex].Name;
                            //Part.Number = Parts[PartIndex].Number;
                            TotalCount = Parts[PartIndex].TotalCount.ToString(CultureInfo.InvariantCulture);
                            PartIndex++;
                            break;
                    }
                    OnPropertyChanged(nameof(PartName));
                    OnPropertyChanged(nameof(TotalCount));
                    break;
                case OrderValidationTypes.Empty:
                    Part.Order = Text.WithoutOrderDescription;
                    Status = "Изготовление без м/л.";
                    break;
                case OrderValidationTypes.Error:
                    Status = "Номер заказа введен некорректно.";
                    break;
            }
        }

        /// <summary> Убирает наладку </summary>
        private void WithoutSetupButton_Click(object sender, RoutedEventArgs e)
        {
            WithSetup = false;
            OnPropertyChanged(nameof(CanBeClosed));
        }

        /// <summary> Включает наладку </summary>
        private void WithSetupButton_Click(object sender, RoutedEventArgs e)
        {
            WithSetup = true;
            OnPropertyChanged(nameof(CanBeClosed));
        }

        /// <summary> Вставляет текущее время как конец наладки</summary>
        private void EndSetupTimeButton_Click(object sender, RoutedEventArgs e)
        {
            StartMachiningTime = DateTime.Now.ToString(Text.DateTimeFormat);
            OnPropertyChanged(nameof(StartMachiningTime));
            OnPropertyChanged(nameof(CanBeClosed));
        }

        /// <summary> Вставляет текущее время как конец изготовления</summary>
        private void EndProductionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            EndMachiningTime = DateTime.Now.ToString(Text.DateTimeFormat);
            OnPropertyChanged(nameof(EndMachiningTime));
            OnPropertyChanged(nameof(CanBeClosed));
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardVisibility = KeyboardVisibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadPreviousPartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.Parts.Count <= 0) return;
            PartInfoModel prev;
            var parts = AppSettings.Parts
                .Where(x => x.IsFinished is not PartInfoModel.State.InProgress)
                .GroupBy(x => new { x.FullName, x.Order, x.Setup })
                .Select(x => new PartInfoModel()
                {
                    Name = x.Key.FullName, 
                    Order = x.Key.Order,
                    Setup = x.Key.Setup,
                    StartSetupTime = DateTime.Now,
                    SetupTimePlan = x.First().SetupTimePlan,
                    SingleProductionTimePlan = x.First().SingleProductionTimePlan,
                    MachineTime = x.First().MachineTime,
                    TotalCount = x.First().TotalCount, 
                    FinishedCount = x.Sum(p => p.FinishedCount)
                }).ToList();
            switch (parts.Count)
            {
                case 1:
                    prev = parts.First();
                    break;
                case > 1:
                    var dlg = new SetPreviousPartDialogWindow(parts) {Owner = this};
                    this.IsEnabled = false;
                    if (dlg.ShowDialog() != true)
                    {
                        Status = "Отмена заполнения.";
                        this.IsEnabled = true;
                        return;
                    }
                    this.IsEnabled = true;
                    prev = dlg.Part!;
                    break;
                default:
                    Status = "Не найдено подходящих деталей.";
                    return;
            }

            WithSetup = false;
            PartName = prev.FullName;
            var order = prev.Order;
            OrderText = !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('/')
            ? order.Split('/')[1]
            : Text.WithoutOrderDescription;
            OrderQualifier =
            !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-')
            ? order.Split('-')[0]
            : AppSettings.OrderQualifiers[0];
            OrderMonth =
            !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-') && order.Contains('/')
            ? order.Split('-')[1].Split('/')[0]
                    : "01";
            Part.Name = prev.Name;
            Part.Number = prev.Number;
            PartName = prev.FullName;
            OnPropertyChanged(nameof(PartName));

            StartSetupTime = Part.StartSetupTime.ToString(Text.DateTimeFormat);
            OnPropertyChanged(nameof(StartSetupTime));

            if (prev.SetupIsFinished)
            {
                Part.StartSetupTime = new DateTime(
                    Part.StartSetupTime.Year,
                    Part.StartSetupTime.Month,
                    Part.StartSetupTime.Day,
                    Part.StartSetupTime.Hour,
                    Part.StartSetupTime.Minute,
                    0, 0);
                StartMachiningTime = StartSetupTime;
                Part.StartMachiningTime = Part.StartSetupTime;
                StartMachiningTime = Part.StartMachiningTime.ToString(Text.DateTimeFormat);
                OnPropertyChanged(nameof(StartMachiningTime));
                OnPropertyChanged(nameof(WithSetup));
            }

            Part.SetupTimePlan = prev.SetupTimePlan;
            PartSetupTimePlan = Part.SetupTimePlan is 0 ? "-" : Part.SetupTimePlan.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(PartSetupTimePlan));

            Part.SingleProductionTimePlan = prev.SingleProductionTimePlan;
            SingleProductionTimePlan = Part.SingleProductionTimePlan is 0 ? "-" : Part.SingleProductionTimePlan.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(SingleProductionTimePlan));

            Part.MachineTime = prev.MachineTime;
            MachineTime = Part.MachineTime.Ticks is 0 ? string.Empty : Part.MachineTime.ToString(Text.TimeSpanFormat);
            OnPropertyChanged(nameof(MachineTime));

            Part.TotalCount = prev.TotalCount;
            TotalCount = Part.TotalCount.ToString();
            OnPropertyChanged(nameof(TotalCount));
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.Space);
        }


        private void IncrementSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (Part.Setup < 24)
            {
                Part.Setup++;
                if (!WithSetup) WithSetup = true;
            }
            OnPropertyChanged(nameof(CanIncreaseSetup));
            OnPropertyChanged(nameof(CanDecreaseSetup));
        }

        private void DecrementSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (Part.Setup > 1)
            {
                Part.Setup--;
            }
            OnPropertyChanged(nameof(CanIncreaseSetup));
            OnPropertyChanged(nameof(CanDecreaseSetup));
        }

        private void UpdateOrders()
        {
            while (true)
            {
                try
                {
                    if (File.Exists(AppSettings.OrdersSourcePath) &&
                        Path.GetExtension(AppSettings.OrdersSourcePath).ToLower() == ".xlsx")
                    {
                        // если локальный список совпадает с сетевым, то ничего не делаем
                        if (File.Exists(AppSettings.LocalOrdersFile) &&
                            File.GetLastWriteTime(AppSettings.LocalOrdersFile) ==
                            File.GetLastWriteTime(AppSettings.OrdersSourcePath)) break;
                        // если обновляем локальный список, то предыдущий храним как бэкап
                        if (File.Exists(AppSettings.LocalOrdersFile))
                        {
                            if (!File.Exists(AppSettings.BackupOrdersFile) ||
                                File.GetLastWriteTime(AppSettings.BackupOrdersFile) !=
                                File.GetLastWriteTime(AppSettings.LocalOrdersFile))
                                File.Copy(AppSettings.LocalOrdersFile, AppSettings.BackupOrdersFile, true);
                        }
                        File.Copy(AppSettings.OrdersSourcePath, AppSettings.LocalOrdersFile, true);

                        Status = $"Список заказов обновлен {File.GetLastWriteTime(AppSettings.LocalOrdersFile).ToString(Text.DateTimeFormat)}";
                        break;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
                Thread.Sleep(3000);
            }
        }
        private void EditCreatorButton_Click(object sender, RoutedEventArgs e)
        {
            var makerDialog = new EditMakerDialogWindow(Part) {Owner = this};
            if (makerDialog.ShowDialog() != true) return;
            Part.Operator = makerDialog.Operator;
            Part.Shift = makerDialog.Shift;
        }


        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
        {
            var handlers = PropertyChanged;
            if (handlers is null) return;

            var invokationList = handlers.GetInvocationList();
            var args = new PropertyChangedEventArgs(PropertyName);

            foreach (var action in invokationList)
            {
                if (action.Target is DispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action, this, args);
                }
                else
                {
                    action.DynamicInvoke(this, args);
                }
            }
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

        #endregion

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not TextBlock { Parent: Grid grid }) return;

            foreach (UIElement gridChild in grid.Children)
            {
                if (gridChild is AdornedElementPlaceholder { AdornedElement: TextBox textBox } && Validation.GetErrors(textBox) is ICollection<ValidationError> {Count: > 0} errors)
                {
                    MessageBox.Show(errors.First().ErrorContent.ToString(), "Некорректный ввод", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var s = "666";
            if (WindowsUserDialogService.GetBarCode(ref s))
            {
                var tempPart = s.GetPartFromBarCode();
                PartName = tempPart.FullName;
                OnPropertyChanged(nameof(PartName));
                PartSetupTimePlan = tempPart.SetupTimePlan.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(PartSetupTimePlan));
                SingleProductionTimePlan = tempPart.SingleProductionTimePlan.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(SingleProductionTimePlan));
                TotalCount = tempPart.TotalCount.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(TotalCount));

                var order = tempPart.Order;
                OrderText = !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('/')
                    ? order.Split('/')[1]
                    : Text.WithoutOrderDescription;
                OrderQualifier =
                    !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-')
                        ? order.Split('-')[0]
                        : AppSettings.OrderQualifiers[0];
                OrderMonth =
                    !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-') && order.Contains('/')
                        ? order.Split('-')[1].Split('/')[0]
                        : "01";
            }
        }
    }
}
