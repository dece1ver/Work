using eLog.Infrastructure;
using eLog.Models;
using eLog.Services.Interfaces;
using eLog.Views.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using eLog.Infrastructure.Extensions;
using eLog.Views.Windows.Settings;
using OperatorsEditWindow = eLog.Views.Windows.Settings.OperatorsEditWindow;

namespace eLog.Services
{
    internal class WindowsUserDialogService : IUserDialogService
    {
        public bool Confirm(string message, string caption, bool exclamation = false)
        {
            return MessageBox.Show(
                       message,
                       caption,
                       MessageBoxButton.YesNo,
                       exclamation
                           ? MessageBoxImage.Exclamation
                           : MessageBoxImage.Question)
                   == MessageBoxResult.Yes;
        }

        public bool Edit(object item)
        {
            return item switch
            {
                ObservableCollection<Operator> operators => EditOperators(ref operators),
                AppSettingsModel settings => EditSettings(ref settings),
                PartInfoModel part => EditDetail(ref part),
                _ => false,
            };
        }

        public void ShowError(string message, string caption) =>
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);

        public void ShowInfo(string message, string caption) =>
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string caption) =>
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);

        public static bool EditOperators(ref ObservableCollection<Operator> operators)
        {
            var dlg = new OperatorsEditWindow()
            {
                Operators = operators,
                Owner = Application.Current.MainWindow,
            };
            if (dlg.ShowDialog() != true) return false;

            operators = dlg.Operators;

            return true;
        }

        public static bool EditSettings(ref AppSettingsModel settings)
        {
            var dlg = new AppSettingsWindow()
            {
                AppSettings = settings,
                CurrentMachine = settings.Machine,
                Owner = Application.Current.MainWindow,
            };
            if (dlg.ShowDialog() != true) return false;

            settings = dlg.AppSettings;

            return true;
        }

        public static bool GetBarCode(ref string barCode)
        {
            var dlg = new ReadBarCodeWindow()
            {
                BarCode = string.Empty,
                Owner = Application.Current.MainWindow,
            };
            if (dlg.ShowDialog() != true) return false;
            barCode = dlg.BarCode;
            return true;
        }

        public static EndSetupResult GetSetupResult()
        {
            var dlg = new EndSetupDialogWindow()
            {
                Owner = Application.Current.MainWindow,
            };
            _ = dlg.ShowDialog();
            return dlg.EndSetupResult;
        }

        public static bool EditDetail(ref PartInfoModel part)
        {
            var tempPart = new PartInfoModel(part.Name, part.Number, part.Setup, part.Order, part.TotalCount,
                part.SetupTimePlan, part.SingleProductionTimePlan)
            {
                StartSetupTime = part.StartSetupTime,
                StartMachiningTime = part.StartMachiningTime,
                EndMachiningTime = part.EndMachiningTime,
                MachineTime = part.MachineTime,
                Id = part.Id,
                FinishedCount = part.FinishedCount,
                Shift = part.Shift,
                OperatorComments = part.OperatorComments
            };
            var dlg = new EditDetailWindow(tempPart)
            {
                Owner = Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true) return false;
            part = dlg.Part;
            if (part.StartMachiningTime != DateTime.MinValue && part.SetupTimeFact.TotalMinutes < 0) part.StartMachiningTime = DateTime.MinValue;
            if (part.EndMachiningTime != DateTime.MinValue && part.FullProductionTimeFact.TotalMinutes < 0) part.EndMachiningTime = DateTime.MinValue;
            return true;
        }

        public static bool FinishDetail(ref PartInfoModel part)
        {
            var tempPart = new PartInfoModel(part.Name, part.Number, part.Setup, part.Order, part.TotalCount,
                part.SetupTimePlan, part.SingleProductionTimePlan)
            {
                StartSetupTime = part.StartSetupTime,
                StartMachiningTime = part.StartMachiningTime,
                EndMachiningTime = part.EndMachiningTime,
                MachineTime = part.MachineTime,
                Id = part.Id,
                FinishedCount = part.FinishedCount,
                Shift = part.Shift,
                OperatorComments = part.OperatorComments

            };
            var dlg = new EndDetailDialogWindow(tempPart)
            {
                Owner = Application.Current.MainWindow,
                PartsFinishedText = tempPart.FinishedCount > 0 ? tempPart.FinishedCount.ToString() : string.Empty,
                MachineTimeText = part.MachineTime.TotalMinutes > 0 ? part.MachineTime.ToString(Text.TimeSpanFormat) : string.Empty,
            };

            if (dlg.ShowDialog() != true) return false;
            dlg.Part.EndMachiningTime = DateTime.Now;
            part = dlg.Part;
            return true;
        }

        public static DownTime.Types? SetDownTimeType()
        {
            var dlg = new SetDownTimeDialogWindow()
            {
                Owner = Application.Current.MainWindow
            };

            if (dlg.ShowDialog() is true) return dlg.Type;
            return null;
        }
    }
}