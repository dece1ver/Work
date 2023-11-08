using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using remeLog.Infrastructure.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace remeLog.ViewModels;

internal class SettingsWindowViewModel : ViewModel
{
    public SettingsWindowViewModel()
    {
        SetSourceTableCommand = new LambdaCommand(OnSetSourceTableCommandExecuted, CanSetSourceTableCommandExecute);
        SetReportsTableCommand = new LambdaCommand(OnSetReportsTableCommandExecuted, CanSetReportsTableCommandExecute);
        SetCopyDirCommand = new LambdaCommand(OnSetCopyDirCommandExecuted, CanSetCopyDirCommandExecute);
        _ = CheckSourceAsync();
        _ = CheckReports();
        _ = CheckCopyDir();
    }

    #region Свойства
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


    private string _SourcePath = string.Empty;
    /// <summary> Путь к таблице </summary>
    public string SourcePath
    {
        get => _SourcePath;
        set => Set(ref _SourcePath, value);
    }


    private string _ReportsPath = string.Empty;
    /// <summary> Путь к файлу с отчетами </summary>
    public string ReportsPath
    {
        get => _ReportsPath;
        set => Set(ref _ReportsPath, value);
    }

    private string _LogsCopyDir = string.Empty;
    /// <summary> Путь к директирии с копией логов </summary>
    public string LogsCopyDir
    {
        get => _LogsCopyDir;
        set => Set(ref _LogsCopyDir, value);
    }


    private CheckStatus _SourceCheckStatus = 0;
    /// <summary> Статус таблицы </summary>
    public CheckStatus SourceCheckStatus
    {
        get => _SourceCheckStatus;
        set => Set(ref _SourceCheckStatus, value);
    }

    private CheckStatus _ReportsCheckStatus = 0;
    /// <summary> Статус отчетов </summary>
    public CheckStatus ReportsCheckStatus
    {
        get => _ReportsCheckStatus;
        set => Set(ref _ReportsCheckStatus, value);
    }

    private CheckStatus _CopyDirCheckStatus = 0;
    /// <summary> Статус отчетов </summary>
    public CheckStatus CopyDirCheckStatus
    {
        get => _CopyDirCheckStatus;
        set => Set(ref _CopyDirCheckStatus, value);
    }

    #endregion

    #region Команды

    #region SetSourceTable
    public ICommand SetSourceTableCommand { get; }
    private void OnSetSourceTableCommandExecuted(object p)
    {
        SourceCheckStatus = (CheckStatus)new Random().Next(0, 3);
        Status = $"Source: {SourceCheckStatus} | Reports: {ReportsCheckStatus} | CopyDir: {CopyDirCheckStatus}";
    }
    private static bool CanSetSourceTableCommandExecute(object p) => true;
    #endregion

    #region SetReportsTable
    public ICommand SetReportsTableCommand { get; }
    private void OnSetReportsTableCommandExecuted(object p)
    {
        ReportsCheckStatus = (CheckStatus)new Random().Next(0, 3);
        Status = $"Source: {SourceCheckStatus} | Reports: {ReportsCheckStatus} | CopyDir: {CopyDirCheckStatus}";
    }
    private static bool CanSetReportsTableCommandExecute(object p) => true;
    #endregion

    #region SetCopyDir
    public ICommand SetCopyDirCommand { get; }
    private void OnSetCopyDirCommandExecuted(object p)
    {
        CopyDirCheckStatus = (CheckStatus)new Random().Next(0, 3);
        Status = $"Source: {SourceCheckStatus} | Reports: {ReportsCheckStatus} | CopyDir: {CopyDirCheckStatus}";
    }
    private static bool CanSetCopyDirCommandExecute(object p) => true;
    #endregion

    #endregion

    private async Task CheckSourceAsync()
    {
        await Task.Run(() =>
        {
            SourceCheckStatus = CheckStatus.Sync;
            Thread.Sleep(1000);
            if (File.Exists(SourcePath))
            {
                SourceCheckStatus |= CheckStatus.Ok;
                return;
            }
            SourceCheckStatus = CheckStatus.Error;
        });
    }

    private async Task CheckReports()
    {
        await Task.Run(() =>
        {
            ReportsCheckStatus = CheckStatus.Sync;
            Thread.Sleep(1000);
            if (File.Exists(ReportsPath))
            {
                ReportsCheckStatus |= CheckStatus.Ok;
                return;
            }
            ReportsCheckStatus = CheckStatus.Error;
        });
    }

    private async Task CheckCopyDir()
    {
        await Task.Run(() =>
        {
            CopyDirCheckStatus = CheckStatus.Sync;
            Thread.Sleep(1000);
            if (Directory.Exists(LogsCopyDir))
            {
                CopyDirCheckStatus |= CheckStatus.Ok;
                return;
            }
            CopyDirCheckStatus = CheckStatus.Error;
        });
    }
}
