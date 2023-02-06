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
        public PartInfoModel Part { get; set; }

        private string _Status;
        /// <summary> Статус </summary>
        public string Status
        {
            get => _Status;
            set => Set(ref _Status, value);
        }

        public bool CanBeClosed => (Part.IsFinished || Part.SetupTimePlan > 0 && Part.PartProductionTimePlan > 0);


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
            if (string.IsNullOrWhiteSpace(Part.Order))
            {
                Status = "Номер заказа не может быть пустым.";
            }
            else
            {
                var tempPart = Part.Order.GetPartFromOrder();
                if (tempPart is null)
                {
                    Status = "Заказ не найден.";
                    return;
                }
                Part.Name = tempPart.Name;
                Part.Number = tempPart.Number;
                Part.PartsCount = tempPart.PartsCount;
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
