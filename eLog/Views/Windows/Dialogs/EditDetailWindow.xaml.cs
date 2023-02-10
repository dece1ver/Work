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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using DocumentFormat.OpenXml.Packaging;
using eLog.Infrastructure;
using eLog.Infrastructure.Extensions;
using eLog.Models;
using eLog.Views.Windows.Settings;
using Path = System.IO.Path;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDetailWindow.xaml
    /// </summary>
    public partial class EditDetailWindow : Window, INotifyPropertyChanged
    {
        public List<PartInfoModel> Parts { get; set; } = new();
        public int PartIndex { get; set; }
        public PartInfoModel Part { get; set; }
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
                switch (OrderValidation)
                {
                    case OrderValidationTypes.Error when OrderText == Text.WithoutOrderDescription:
                        OrderText = string.Empty;
                        break;
                    case OrderValidationTypes.Empty:
                        OrderText = Text.WithoutOrderDescription;
                        break;
                    case OrderValidationTypes.Valid:
                        Status = $"Выбран заказ: {Part.Order}";
                        break;
                }
                OnPropertyChanged(nameof(CanBeClosed));
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
                if (OrderValidation is OrderValidationTypes.Valid) Status = $"Выбран заказ: {Part.Order}";
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
                if (OrderValidation is OrderValidationTypes.Valid) Status = $"Выбран заказ: {Part.Order}";
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

        public bool CanBeClosed => (Part.IsFinished || 
                                    Part is { SetupTimePlan: > 0, PartProductionTimePlan: > 0 } && 
                                    OrderValidation is not OrderValidationTypes.Error &&
                                    Part.SetupTimePlan > 0 && 
                                    Part.PartProductionTimePlan > 0 && 
                                    Part.PartsCount > 0 && 
                                    !string.IsNullOrWhiteSpace(Part.FullName)
                                    );


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

        #region Обертки требуемых для валидации свойств детали, потому что я хз как отсюда заставить обновляться без этого.
        public string PartName
        {
            get => Part.Name;
            set
            {
                Part.Name = value;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        public double PartSetupTimePlan
        {
            get => Part.SetupTimePlan;
            set
            {
                Part.SetupTimePlan = value;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        public double PartPartProductionTimePlan
        {
            get => Part.PartProductionTimePlan;
            set
            {
                Part.PartProductionTimePlan = value;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        } 

        public int PartsCount
        {
            get => Part.PartsCount;
            set
            {
                Part.PartsCount = value;
                OnPropertyChanged(nameof(CanBeClosed));
            }
        } 
        #endregion

        public EditDetailWindow(PartInfoModel part)
        {
            
            Part = part;
            var order = Part.Order;
            OrderText = !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('/')
                ? order.Split('/')[1] 
                : string.Empty;
            OrderQualifier =
                !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-')
                    ? order.Split('-')[0]
                    : AppSettings.OrderQualifiers[0];
            OrderMonth =
                !string.IsNullOrWhiteSpace(order) && order != Text.WithoutOrderDescription && order.Contains('-') && order.Contains('/')
                    ? order.Split('-')[1].Split('/')[0]
                    : "01";
            
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
                            Status = "Заказы не найдены.";
                            return;
                        case 1:
                            Status = "Заказ найден.";
                            PartName = Parts[0].Name;
                            Part.Number = Parts[0].Number;
                            PartsCount = Parts[0].PartsCount;
                            break;
                        case > 1:
                            Status = $"Найдено несколько заказов: {Parts.Count}. Переключение на кнопку поиска.";
                            PartName = Parts[PartIndex].Name;
                            Part.Number = Parts[PartIndex].Number;
                            PartsCount = Parts[PartIndex].PartsCount;
                            PartIndex++;
                            break;
                    }
                    OnPropertyChanged(nameof(PartName));
                    OnPropertyChanged(nameof(PartsCount));
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

        /// <summary> Приравнивает окончание наладки к началу </summary>
        private void EndSetupButton_Click(object sender, RoutedEventArgs e)
        {
            Part.StartMachiningTime = Part.StartSetupTime;
        }

        /// <summary> Вставляет текущее время как конец наладки</summary>
        private void EndSetupTimeButton_Click(object sender, RoutedEventArgs e)
        {
            Part.StartMachiningTime = DateTime.Now;
        }

        /// <summary> Вставляет текущее время как конец изготовления</summary>
        private void EndProductionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            Part.EndMachiningTime = DateTime.Now;
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
                        

                        // сохраняем список заказов локально
                        if (File.Exists(AppSettings.LocalOrdersFile) &&
                            File.GetLastWriteTime(AppSettings.LocalOrdersFile) ==
                            File.GetLastWriteTime(AppSettings.OrdersSourcePath)) break;
                        // перед этим делаем бэкап
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



        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
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

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }

        #endregion
    }
}
