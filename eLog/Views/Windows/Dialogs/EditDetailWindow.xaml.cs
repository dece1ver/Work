using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Path = System.IO.Path;

namespace eLog.Views.Windows.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для EditDetailWindow.xaml
    /// </summary>
    public partial class EditDetailWindow : Window, INotifyPropertyChanged
    {
        public List<PartInfoModel> Parts { get; set; } = new();
        public int PartIndex { get; set; } = 0;
        public PartInfoModel Part { get; set; }

        private string _Status;
        private string _OrderText = string.Empty;

        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        public bool CanBeClosed => (Part.IsFinished || Part is { SetupTimePlan: > 0, PartProductionTimePlan: > 0 } && OrderValidateType is not OrderValidateTypes.Error);

        public string OrderText
        {
            get => _OrderText;
            set
            {
                Set(ref _OrderText, value);
                Parts.Clear();
                OnPropertyChanged(nameof(OrderValidateType));
                OnPropertyChanged(nameof(CanBeClosed));
            }
        }

        public enum OrderValidateTypes {Error, Empty, Valid}

        public OrderValidateTypes OrderValidateType
        {
            get
            {
                if (OrderText is "")
                {
                    return OrderValidateTypes.Empty;
                }

                if (OrderText.Length < 14 ||
                    !char.IsLetter(OrderText[0]) ||
                    !char.IsLetter(OrderText[1]) ||
                    OrderText[2] != '-' ||
                    !char.IsNumber(OrderText[3]) ||
                    !char.IsNumber(OrderText[4]) ||
                    OrderText[5] != '/' ||
                    !char.IsNumber(OrderText[6]) ||
                    !char.IsNumber(OrderText[7]) ||
                    !char.IsNumber(OrderText[8]) ||
                    !char.IsNumber(OrderText[9]) ||
                    !char.IsNumber(OrderText[10]) ||
                    OrderText[11] != '.' ||
                    !char.IsNumber(OrderText[12]) ||
                    OrderText.Split('/')[1].Count(x => x is '.') != 2) return OrderValidateTypes.Error;
                Part.Order = OrderText;
                return OrderValidateTypes.Valid;
            }
        }

        public EditDetailWindow(PartInfoModel part)
        {
            Part = part;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(AppSettings.LocalOrdersFile))
            {
                Status =
                    $"Список заказов обновлен {File.GetLastWriteTime(AppSettings.LocalOrdersFile):dd.MM.yyyy HH:mm}";
            }
            var updaterThread = new Thread(UpdateOrders);
            updaterThread.Start();
        }

        /// <summary> Реализация поиска номенклатуры по номеру М/Л (имитация) </summary>
        private void FindOrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            switch (OrderValidateType)
            {
                case OrderValidateTypes.Valid:
                    Status = "Поиск заказов...";
                    if (Parts.Count == 0) Parts = OrderText.GetPartsFromOrder();
                    if (PartIndex > Parts.Count - 1) PartIndex = 0;

                    switch (Parts.Count)
                    {
                        case 0:
                            Status = "Заказы не найдены.";
                            return;
                        case 1:
                            Status = "Заказ найден.";
                            Part.Name = Parts[0].Name;
                            Part.Number = Parts[0].Number;
                            Part.PartsCount = Parts[0].PartsCount;
                            break;
                        case > 1:
                            Status = $"Найдено несколько заказов: {Parts.Count}. Переключение на кнопку поиска.";
                            Part.Name = Parts[PartIndex].Name;
                            Part.Number = Parts[PartIndex].Number;
                            Part.PartsCount = Parts[PartIndex].PartsCount;
                            PartIndex++;
                            break;
                    }
                    break;
                case OrderValidateTypes.Empty:
                    Status = "Изготовление без м/л.";
                    break;
                case OrderValidateTypes.Error:
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
