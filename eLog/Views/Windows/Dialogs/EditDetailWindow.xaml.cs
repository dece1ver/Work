using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using eLog.Views.Windows.Settings;
using Path = System.IO.Path;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDetailWindow.xaml
    /// </summary>
    public partial class EditDetailWindow : Window, INotifyPropertyChanged, IDataErrorInfo
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
                Height = _KeyboardVisibility is Visibility.Visible ? 592 : 469;
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

        public bool WithSetup => Part.StartSetupTime != Part.StartMachiningTime;

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
        private string _OrderMonth = OrderMonths[0];
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
                if (Part.IsFinished) return true;
                if (Part is { SetupTimePlan: > 0, SingleProductionTimePlan: > 0, TotalCount: > 0, SetupIsNotFinished: true, EndMachiningTime.Ticks: 0 }) return true;
                if (Part is { SetupTimePlan: > 0, SingleProductionTimePlan: > 0, TotalCount: > 0, SetupIsFinished: true, EndMachiningTime.Ticks: 0, FinishedCount: 0 }) return true;
                if (Part is { SetupTimePlan: > 0, SingleProductionTimePlan: > 0, TotalCount: > 0, SetupIsFinished: true, EndMachiningTime.Ticks: > 0, FinishedCount: > 0, MachineTime.TotalSeconds: > 0 }) return true;

                //var res = (Part.IsFinished ||
                //           Part is { SetupTimePlan: > 0, SingleProductionTimePlan: > 0, TotalCount: > 0 } &&
                //           OrderValidation is not OrderValidationTypes.Error &&
                //           !string.IsNullOrWhiteSpace(Part.FullName));
                //if (res && Part.EndMachiningTime > Part.StartMachiningTime && Part.MachineTime <= TimeSpan.Zero)
                //    res = false;
                return false;
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
                OnPropertyChanged(nameof(CanBeClosed));
                OnPropertyChanged(nameof(FinishedCount));
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

        public string Error { get; }

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case nameof(PartName):
                        if (string.IsNullOrWhiteSpace(Part.Name)) error = "Пустое имя";
                        break;
                    case nameof(StartSetupTime):
                        if (Part.StartSetupTime == DateTime.MinValue) error = "Некорректное время начала наладки";
                        break;
                    case nameof(StartMachiningTime):
                        if (!string.IsNullOrWhiteSpace(StartMachiningTime) && Part.StartMachiningTime == DateTime.MinValue) error = "Некорректное время начала изготовления";
                        break;
                    case nameof(EndMachiningTime):
                        if (!string.IsNullOrWhiteSpace(StartMachiningTime) && Part.EndMachiningTime <= Part.StartMachiningTime) error = "Некорректное время завершения изготовления";
                        break;
                    case nameof(MachineTime):
                        if (Part.MachineTime == TimeSpan.Zero && (Part.FullProductionTimeFact > TimeSpan.Zero || Part.FinishedCount > 0)) error = "Некорректное машинное время";
                        break;
                    case nameof(FinishedCount):
                        error = Part.FinishedCount switch
                        {
                            0 when string.IsNullOrWhiteSpace(FinishedCount) && Part.FullProductionTimeFact > TimeSpan.Zero => "Обязательный параметр если указано время завершения изготовления.",
                            0 when !string.IsNullOrWhiteSpace(FinishedCount) => "Некорректное количество завершенных деталей",
                            _ => error
                        };
                        break;
                    case nameof(TotalCount):
                        error = Part.TotalCount switch
                        {
                            0 when string.IsNullOrWhiteSpace(FinishedCount) => "Обязательный параметр.",
                            0 when Part.EndMachiningTime > Part.StartMachiningTime =>
                                "Некорректное плановое количество деталей",
                            _ => error
                        };
                        if (Part.TotalCount == 0) error = "Некорректное плановое количество деталей";
                        break;
                    case nameof(PartSetupTimePlan):
                        error = Part.SetupTimePlan switch
                        {
                            0 when string.IsNullOrWhiteSpace(PartSetupTimePlan) => "Обязательный параметр. При отсутствии указать \"-\"",
                            0 when !string.IsNullOrWhiteSpace(PartSetupTimePlan) &&
                                   PartSetupTimePlan != "-" => "Некорректный норматив на наладку",
                            _ => error
                        };
                        break;
                    case nameof(SingleProductionTimePlan):
                        error = Part.SingleProductionTimePlan switch
                        {
                            0 when string.IsNullOrWhiteSpace(SingleProductionTimePlan) => "Обязательный параметр. При отсутствии указать \"-\"",
                            0 when !string.IsNullOrWhiteSpace(SingleProductionTimePlan) && SingleProductionTimePlan != "-" => "Некорректный норматив на изготовление",
                            _ => error
                        };
                        break;
                    case nameof(OrderText):
                        if (OrderValidation is OrderValidationTypes.Error) error = "Некорректный номер заказа.";
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
            _EndMachiningTime = Part.EndMachiningTime != DateTime.MinValue ? Part.EndMachiningTime.ToString(Text.DateTimeFormat) : string.Empty;
            _MachineTime = Part.MachineTime != TimeSpan.Zero ? Part.MachineTime.ToString(Text.TimeSpanFormat) : string.Empty;

            _PartSetupTimePlan = Part.SetupTimePlan > 0 ? Part.SetupTimePlan.ToString(CultureInfo.InvariantCulture) : string.Empty;
            _SingleProductionTimePlan = Part.SingleProductionTimePlan > 0 ? Part.SingleProductionTimePlan.ToString(CultureInfo.InvariantCulture) : string.Empty;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(AppSettings.LocalOrdersFile))
            {
                Status =
                    $"Список заказов обновлен {File.GetLastWriteTime(AppSettings.LocalOrdersFile):dd.MM.yyyy HH:mm}";
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
            OnPropertyChanged(nameof(WithSetup));
        }

        /// <summary> Включает наладку </summary>
        private void WithSetupButton_Click(object sender, RoutedEventArgs e)
        {
            StartMachiningTime = string.Empty;
            OnPropertyChanged(nameof(StartMachiningTime));
            OnPropertyChanged(nameof(WithSetup));
        }

        /// <summary> Вставляет текущее время как конец наладки</summary>
        private void EndSetupTimeButton_Click(object sender, RoutedEventArgs e)
        {
            StartMachiningTime = DateTime.Now.ToString(Text.DateTimeFormat);
            OnPropertyChanged(nameof(StartMachiningTime));
        }

        /// <summary> Вставляет текущее время как конец изготовления</summary>
        private void EndProductionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            EndMachiningTime = DateTime.Now.ToString(Text.DateTimeFormat);
            OnPropertyChanged(nameof(EndMachiningTime));
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardVisibility = KeyboardVisibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LoadPreviousPartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.Parts.Count <= 0) return;
            var prev = AppSettings.Parts[^1];
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

            Part.StartSetupTime = prev.EndMachiningTime;
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
            PartSetupTimePlan = Part.SetupTimePlan.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(PartSetupTimePlan));

            Part.SingleProductionTimePlan = prev.SingleProductionTimePlan;
            SingleProductionTimePlan = Part.SingleProductionTimePlan.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(SingleProductionTimePlan));

            Part.MachineTime = prev.MachineTime;
            MachineTime = Part.MachineTime.ToString(Text.TimeSpanFormat);
            OnPropertyChanged(nameof(MachineTime));

            Part.TotalCount = prev.TotalCount;
            TotalCount = Part.TotalCount.ToString();
            OnPropertyChanged(nameof(TotalCount));
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.Space);
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

                        Status = $"Список заказов обновлен {File.GetLastWriteTime(AppSettings.LocalOrdersFile):dd.MM.yyyy HH:mm}";
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


        
    }
}
