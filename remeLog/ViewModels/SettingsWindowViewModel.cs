using DocumentFormat.OpenXml.Wordprocessing;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Win32;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Types;
using remeLog.Models;
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

namespace remeLog.ViewModels;

internal class SettingsWindowViewModel : ViewModel
{
    public SettingsWindowViewModel()
    {
        SetSourceTableCommand = new LambdaCommand(OnSetSourceTableCommandExecuted, CanSetSourceTableCommandExecute);
        SetReportsTableCommand = new LambdaCommand(OnSetReportsTableCommandExecuted, CanSetReportsTableCommandExecute);
        SetDailyReportsDirCommand = new LambdaCommand(OnSetDailyReportsDirCommandExecuted, CanSetDailyReportsDirCommandExecute);
        CheckConnectionStringCommand = new LambdaCommand(OnCheckConnectionStringCommandExecuted, CanCheckConnectionStringCommandExecute);

        _DataSource = AppSettings.Instance.DataSource;
        _SourcePath = new SettingsItem(AppSettings.Instance.SourcePath ?? "");
        _ReportsPath = new SettingsItem(AppSettings.Instance.ReportsPath ?? "");
        _DailyReportsDir = new SettingsItem(AppSettings.Instance.DailyReportsDir ?? "");
        _ConnectionString = new SettingsItem(AppSettings.Instance.ConnectionString ?? "");

        _ = CheckSourceAsync();
        _ = CheckReportsAsync();
        _ = CheckDailyReportsDirAsync();
        _ = CheckConnectionStringAsync();
    }

    #region Свойства

    public List<DataSource> DataSourceTypes { get; set; } = new() {
        new DataSource(DataSource.Types.Database),
        new DataSource(DataSource.Types.Excel),
    };

    private string _Status = string.Empty;
    /// <summary> Статус </summary>
    public string Status
    {
        get => _Status;
        set => Set(ref _Status, value);
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


    private DataSource _DataSource;
    /// <summary> Описание </summary>
    public DataSource DataSource
    {
        get => _DataSource;
        set => Set(ref _DataSource, value);
    }


    private SettingsItem _SourcePath;
    /// <summary> Путь к таблице </summary>
    public SettingsItem SourcePath
    {
        get => _SourcePath;
        set => Set(ref _SourcePath, value);
    }

    private SettingsItem _ReportsPath;
    /// <summary> Путь к файлу с отчетами </summary>
    public SettingsItem ReportsPath
    {
        get => _ReportsPath;
        set => Set(ref _ReportsPath, value);
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

    #region SetSourceTable
    public ICommand SetSourceTableCommand { get; }
    private void OnSetSourceTableCommandExecuted(object p)
    {
        OpenFileDialog dlg = new()
        {
            Filter = "Excel книга с макросами (*.xlsm)|*.xlsm",
            DefaultExt = "xlsm"
        };
        if (dlg.ShowDialog() != true) return;
        SourcePath.Value = dlg.FileName;
        _ = CheckSourceAsync();
    }
    private static bool CanSetSourceTableCommandExecute(object p) => true;
    #endregion

    #region SetReportsTable
    public ICommand SetReportsTableCommand { get; }
    private void OnSetReportsTableCommandExecuted(object p)
    {
        OpenFileDialog dlg = new()
        {
            Filter = "Excel книга с макросами (*.xlsm)|*.xlsm",
            DefaultExt = "xlsm"
        };
        if (dlg.ShowDialog() != true) return;
        ReportsPath.Value = dlg.FileName;
        _ = CheckReportsAsync();
    }
    private static bool CanSetReportsTableCommandExecute(object p) => true;
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

    private async Task CheckSourceAsync()
    {
        await Task.Run(() =>
        {
            SourcePath.Status = CheckStatus.Sync;
            SourcePath.Tip = Constants.StatusTips.Checking;
            if (File.Exists(SourcePath.Value))
            {
                SourcePath.Status = CheckStatus.Ok;
                SourcePath.Tip = Constants.StatusTips.Ok;
                return;
            }
            SourcePath.Status = CheckStatus.Error;
            SourcePath.Tip = Constants.StatusTips.NoFile;
        });
    }

    private async Task CheckReportsAsync()
    {
        await Task.Run(() =>
        {
            ReportsPath.Status = CheckStatus.Sync;
            ReportsPath.Tip = Constants.StatusTips.Checking;
            if (File.Exists(ReportsPath.Value))
            {
                ReportsPath.Status = CheckStatus.Ok;
                ReportsPath.Tip = Constants.StatusTips.Ok;
                return;
            }
            ReportsPath.Status = CheckStatus.Error;
            ReportsPath.Tip = Constants.StatusTips.NoFile;
        });
    }

    private async Task CheckDailyReportsDirAsync()
    {
        await Task.Run(() =>
        {
            DailyReportsDir.Status = CheckStatus.Sync;
            DailyReportsDir.Tip = Constants.StatusTips.Checking;
            switch (DailyReportsDir.Value.CheckDirectoryRights(FileSystemRights.WriteData))
            {
                case CheckDirectoryRightsResult.HasAccess:
                    DailyReportsDir.Status = CheckStatus.Ok;
                    DailyReportsDir.Tip = Constants.StatusTips.Ok;
                    break;
                case CheckDirectoryRightsResult.NoAccess:
                    DailyReportsDir.Status = CheckStatus.Warning;
                    DailyReportsDir.Tip = Constants.StatusTips.NoWriteAccess;
                    break;
                case CheckDirectoryRightsResult.NotExists:
                    DailyReportsDir.Status = CheckStatus.Error;
                    DailyReportsDir.Tip = Constants.StatusTips.NoAccessToDirectory;
                    break;
                case CheckDirectoryRightsResult.Error:
                    DailyReportsDir.Status = CheckStatus.Error;
                    DailyReportsDir.Tip = Constants.StatusTips.AccessError;
                    break;
            }
        });
    }

    private async Task CheckConnectionStringAsync()
    {
        await Task.Run(() =>
        {
            ConnectionString.Status = CheckStatus.Sync;
            ConnectionString.Tip = Constants.StatusTips.Checking;
            var res = ConnectionString.Value.CheckDbConnection();
            switch (res.result)
            {
                case DbResult.Ok:
                    ConnectionString.Status = CheckStatus.Ok;
                    break;
                case DbResult.AuthError:
                    ConnectionString.Status= CheckStatus.Error;
                    break;
                case DbResult.Error:
                    ConnectionString.Status = CheckStatus.Error;
                    break;
            }
            ConnectionString.Tip = res.message;
        });
    }
}
