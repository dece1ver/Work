using DocumentFormat.OpenXml.Wordprocessing;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Win32;
using neLog.Infrastructure;
using libeLog.Infrastructure;
using neLog.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace neLog.ViewModels;

internal class SettingsWindowViewModel : ViewModel
{
    public SettingsWindowViewModel()
    {
        SetStorageTableCommand = new LambdaCommand(OnSetStorageTableCommandExecuted, CanSetStorageTableCommandExecute);
        SetOrdersTableCommand = new LambdaCommand(OnSetOrdersTableCommandExecuted, CanSetOrdersTableCommandExecute);
        CheckConnectionStringCommand = new LambdaCommand(OnCheckConnectionStringCommandExecuted, CanCheckConnectionStringCommandExecute);

        _StorageTablePath = new SettingsItem(AppSettings.Instance.StorageTablePath ?? "");
        _OrdersTablePath = new SettingsItem(AppSettings.Instance.OrdersTablePath ?? "");
        _ConnectionString = new SettingsItem(AppSettings.Instance.ConnectionString ?? "");

        _ = CheckStorageTableAsync();
        _ = CheckOrdersTableAsync();
        _ = CheckConnectionStringAsync();
    }



    #region Свойства

    public List<StorageType> StorageTypes { get; set; } = new() {
        new StorageType(StorageType.Types.Database),
        new StorageType(StorageType.Types.Excel),
        new StorageType(StorageType.Types.All),
    };


    private StorageType _StorageType;
    /// <summary> Описание </summary>
    public StorageType StorageType
    {
        get => _StorageType;
        set => Set(ref _StorageType, value);
    }


    private string _StatusText = string.Empty;
    /// <summary> Текст статусбара </summary>
    public string StatusText
    {
        get => _StatusText;
        set => Set(ref _StatusText, value);
    }

    private double _Progress = 1;
    /// <summary> Значение прогрессбара </summary>
    public double Progress
    {
        get => _Progress;
        set
        {
            Set(ref _Progress, value);
            OnPropertyChanged(nameof(ProgressBarVisibility));
        }
    }

    private double _ProgressMaxValue = 1;
    /// <summary> Максимальное значение прогрессбара </summary>
    public double ProgressMaxValue
    {
        get => _ProgressMaxValue;
        set
        {
            Set(ref _ProgressMaxValue, value);
            OnPropertyChanged(nameof(ProgressBarVisibility));
        }
    }

    private Visibility _ProgressBarVisibility = Visibility.Hidden;
    /// <summary> Видимость прогрессбара </summary>
    public Visibility ProgressBarVisibility
    {
        get => _ProgressBarVisibility;
        set => Set(ref _ProgressBarVisibility, value);
    }

    private SettingsItem _StorageTablePath;
    /// <summary> Путь к таблице </summary>
    public SettingsItem StorageTablePath
    {
        get => _StorageTablePath;
        set => Set(ref _StorageTablePath, value);
    }

    private SettingsItem _OrdersTablePath;
    /// <summary> Путь к файлу с отчетами </summary>
    public SettingsItem OrdersTablePath
    {
        get => _OrdersTablePath;
        set => Set(ref _OrdersTablePath, value);
    }

    private SettingsItem _DailyReportsDir;
    /// <summary> Путь к директирии с суточными отчетами </summary>
    public SettingsItem DailyReportsDir
    {
        get => _DailyReportsDir;
        set => Set(ref _DailyReportsDir, value);
    }

    private SettingsItem _ConnectionString;
    /// <summary> Строка подключения к БД </summary>
    public SettingsItem ConnectionString
    {
        get => _ConnectionString;
        set => Set(ref _ConnectionString, value);
    }


    #endregion

    #region Команды

    #region SetStorageTable
    public ICommand SetStorageTableCommand { get; }
    private void OnSetStorageTableCommandExecuted(object p)
    {
        OpenFileDialog dlg = new()
        {
            Filter = "Excel книга с макросами (*.xlsm)|*.xlsm",
            DefaultExt = "xlsm"
        };
        if (dlg.ShowDialog() != true) return;
        StorageTablePath.Value = dlg.FileName;
        _ = CheckStorageTableAsync();
    }
    private static bool CanSetStorageTableCommandExecute(object p) => true;
    #endregion

    #region SetOrdersTable
    public ICommand SetOrdersTableCommand { get; }
    private void OnSetOrdersTableCommandExecuted(object p)
    {
        OpenFileDialog dlg = new()
        {
            Filter = "Excel книга с макросами (*.xlsm)|*.xlsm",
            DefaultExt = "xlsm"
        };
        if (dlg.ShowDialog() != true) return;
        OrdersTablePath.Value = dlg.FileName;
        _ = CheckOrdersTableAsync();
    }
    private static bool CanSetOrdersTableCommandExecute(object p) => true;
    #endregion

    #region SetDailyReportsDir
    public ICommand SetDailyReportsDirCommand { get; }
    private void OnSetDailyReportsDirCommandExecuted(object p)
    {
        FolderBrowserDialog dlg = new()
            ;
        if (dlg.ShowDialog() != DialogResult.OK) return;
        DailyReportsDir.Value = dlg.SelectedPath;
        _ = CheckDailyReportsDirAsync();
    }
    private static bool CanSetDailyReportsDirCommandExecute(object p) => true;
    #endregion

    #region CheckConnectionString
    public ICommand CheckConnectionStringCommand { get; }
    private void OnCheckConnectionStringCommandExecuted(object p)
    {
        _ = CheckConnectionStringAsync();
    }
    private static bool CanCheckConnectionStringCommandExecute(object p) => true;
    #endregion


    #endregion

    private async Task CheckStorageTableAsync()
    {
        await Task.Run(() =>
        {
            StorageTablePath.Status = Status.Sync;
            StorageTablePath.Tip = Constants.StatusTips.Checking;
            if (File.Exists(StorageTablePath.Value))
            {
                StorageTablePath.Status = Status.Ok;
                StorageTablePath.Tip = Constants.StatusTips.Ok;
                return;
            }
            StorageTablePath.Status = Status.Error;
            StorageTablePath.Tip = Constants.StatusTips.NoFile;
        });
    }

    private async Task CheckOrdersTableAsync()
    {
        await Task.Run(() =>
        {
            OrdersTablePath.Status = Status.Sync;
            OrdersTablePath.Tip = Constants.StatusTips.Checking;
            if (File.Exists(OrdersTablePath.Value))
            {
                OrdersTablePath.Status = Status.Ok;
                OrdersTablePath.Tip = Constants.StatusTips.Ok;
                return;
            }
            OrdersTablePath.Status = Status.Error;
            OrdersTablePath.Tip = Constants.StatusTips.NoFile;
        });
    }

    private async Task CheckDailyReportsDirAsync()
    {
        await Task.Run(() =>
        {
            DailyReportsDir.Status = Status.Sync;
            DailyReportsDir.Tip = Constants.StatusTips.Checking;
            switch (DailyReportsDir.Value.CheckDirectoryRights(FileSystemRights.WriteData))
            {
                case CheckDirectoryRightsResult.HasAccess:
                    DailyReportsDir.Status = Status.Ok;
                    DailyReportsDir.Tip = Constants.StatusTips.Ok;
                    break;
                case CheckDirectoryRightsResult.NoAccess:
                    DailyReportsDir.Status = Status.Warning;
                    DailyReportsDir.Tip = Constants.StatusTips.NoWriteAccess;
                    break;
                case CheckDirectoryRightsResult.NotExists:
                    DailyReportsDir.Status = Status.Error;
                    DailyReportsDir.Tip = Constants.StatusTips.NoAccessToDirectory;
                    break;
                case CheckDirectoryRightsResult.Error:
                    DailyReportsDir.Status = Status.Error;
                    DailyReportsDir.Tip = Constants.StatusTips.AccessError;
                    break;
            }
        });
    }

    private async Task CheckConnectionStringAsync()
    {
        await Task.Run(() =>
        {
            ConnectionString.Status = Status.Sync;
            ConnectionString.Tip = Constants.StatusTips.Checking;
            var res = ConnectionString.Value.CheckDbConnection();
            switch (res.result)
            {
                case DbResult.Ok:
                    ConnectionString.Status = Status.Ok;
                    break;
                case DbResult.AuthError:
                    ConnectionString.Status= Status.Error;
                    break;
                case DbResult.Error:
                    ConnectionString.Status = Status.Error;
                    break;
            }
            ConnectionString.Tip = res.message;
        });
    }
}
