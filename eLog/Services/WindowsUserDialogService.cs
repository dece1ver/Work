﻿using eLog.Infrastructure;
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

        public static (EndDetailResult, int, TimeSpan) GetFinishResult()
        {
            var dlg = new EndDetailDialogWindow()
            {
                Owner = Application.Current.MainWindow,
            };
            _ = dlg.ShowDialog();
            return (dlg.EndDetailResult, dlg.PartsCount, dlg.MachineTime);
        }

        public static bool EditDetail(ref PartInfoModel part)
        {
            var tempPart = new PartInfoModel(part.Name, part.Number, part.Setup, part.Order, part.PartsCount,
                part.SetupTimePlan, part.MachineTimePlan)
            {
                StartSetupTime = part.StartSetupTime,
                StartMachiningTime = part.StartMachiningTime,
                EndMachiningTime = part.EndMachiningTime,
                MachineTime = part.MachineTime,
                Id = part.Id,
                PartsFinished = part.PartsFinished

            };
            var dlg = new EditDetailWindow(tempPart)
            {
                Owner = Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true) return false;
            part = dlg.Part;
            return true;
        }
    }
}