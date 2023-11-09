using DocumentFormat.OpenXml.Wordprocessing;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Win32;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace remeLog.ViewModels;

internal class SettingsWindowViewModel : ViewModel
{
    public SettingsWindowViewModel()
    {
        SetSourceTableCommand = new LambdaCommand(OnSetSourceTableCommandExecuted, CanSetSourceTableCommandExecute);
        SetReportsTableCommand = new LambdaCommand(OnSetReportsTableCommandExecuted, CanSetReportsTableCommandExecute);
        SetDailyReportsDirCommand = new LambdaCommand(OnSetDailyReportsDirCommandExecuted, CanSetDailyReportsDirCommandExecute);
        SourcePath = AppSettings.Instance.SourcePath ?? "";
        ReportsPath = AppSettings.Instance.ReportsPath ?? "";
        DailyReportsDir = AppSettings.Instance.DailyReportsDir ?? "";
        _ = CheckSourceAsync();
        _ = CheckReportsAsync();
        _ = CheckDailyReportsDirAsync();
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


    #region SourcePath

    private string _SourcePath = string.Empty;
    /// <summary> Путь к таблице </summary>
    public string SourcePath
    {
        get => _SourcePath;
        set => Set(ref _SourcePath, value);
    }

    private CheckStatus _SourceCheckStatus = 0;
    /// <summary> Статус таблицы </summary>
    public CheckStatus SourceCheckStatus
    {
        get => _SourceCheckStatus;
        set => Set(ref _SourceCheckStatus, value);
    }


    private string _SourceCheckTip;
    /// <summary> Подсказка для  </summary>
    public string SourceCheckTip
    {
        get => _SourceCheckTip;
        set => Set(ref _SourceCheckTip, value);
    }


    #endregion

    #region ReportsPath

    private string _ReportsPath = string.Empty;
    /// <summary> Путь к файлу с отчетами </summary>
    public string ReportsPath
    {
        get => _ReportsPath;
        set => Set(ref _ReportsPath, value);
    }

    private CheckStatus _ReportsCheckStatus = 0;

    /// <summary> Статус отчетов </summary>
    public CheckStatus ReportsCheckStatus
    {
        get => _ReportsCheckStatus;
        set => Set(ref _ReportsCheckStatus, value);
    }
    
    private string _ReportsCheckTip;
    /// <summary> Подсказка для файла отчетов </summary>
    public string ReportsCheckTip
    {
        get => _ReportsCheckTip;
        set => Set(ref _ReportsCheckTip, value);
    }


    #endregion

    #region DailyReportsDir
    private string _DailyReportsDir = string.Empty;
    /// <summary> Путь к директирии с суточными отчетами </summary>
    public string DailyReportsDir
    {
        get => _DailyReportsDir;
        set => Set(ref _DailyReportsDir, value);
    }

    private CheckStatus _DailyReportsDirCheckStatus = 0;
    /// <summary> Статус директории суточных отчетов </summary>
    public CheckStatus DailyReportsDirCheckStatus
    {
        get => _DailyReportsDirCheckStatus;
        set => Set(ref _DailyReportsDirCheckStatus, value);
    }
    

    private string _DailyReportsDirCheckTip;
    /// <summary> Подсказка для директории с суточными отчетами </summary>
    public string DailyReportsDirCheckTip
    {
        get => _DailyReportsDirCheckTip;
        set => Set(ref _DailyReportsDirCheckTip, value);
    }

    #endregion

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
        SourcePath = dlg.FileName;
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
        ReportsPath = dlg.FileName;
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
        DailyReportsDir = dlg.SelectedPath;
        _ = CheckDailyReportsDirAsync();
    }
    private static bool CanSetDailyReportsDirCommandExecute(object p) => true;
    #endregion


    #endregion

    private async Task CheckSourceAsync()
    {
        await Task.Run(() =>
        {
            SourceCheckStatus = CheckStatus.Sync;
            if (File.Exists(SourcePath))
            {
                SourceCheckStatus = CheckStatus.Ok;
                SourceCheckTip = "Файл корректный";
                return;
            }
            SourceCheckStatus = CheckStatus.Error;
            SourceCheckTip = "Файл отсутствует";
        });
    }

    private async Task CheckReportsAsync()
    {
        await Task.Run(() =>
        {
            ReportsCheckStatus = CheckStatus.Sync;
            if (File.Exists(ReportsPath))
            {
                ReportsCheckStatus = CheckStatus.Ok;
                ReportsCheckTip = "Файл корректный";
                return;
            }
            ReportsCheckStatus = CheckStatus.Error;
            ReportsCheckTip = "Файл отсутствует";
        });
    }

    private async Task CheckDailyReportsDirAsync()
    {
        await Task.Run(() =>
        {
            DailyReportsDirCheckStatus = CheckStatus.Sync;
            switch (DailyReportsDir.CheckDirectoryRights(FileSystemRights.WriteData))
            {
                case CheckDirectoryRightsResult.HasAccess:
                    DailyReportsDirCheckStatus = CheckStatus.Ok;
                    DailyReportsDirCheckTip = "Все в порядке.";
                    break;
                case CheckDirectoryRightsResult.NoAccess:
                    DailyReportsDirCheckStatus = CheckStatus.Error;
                    DailyReportsDirCheckTip = "Нет доступа на запись.";
                    break;
                case CheckDirectoryRightsResult.NotExists:
                    DailyReportsDirCheckStatus = CheckStatus.Error;
                    DailyReportsDirCheckTip = "Директория не существует.";
                    break;
                case CheckDirectoryRightsResult.Error:
                    DailyReportsDirCheckStatus = CheckStatus.Error;
                    DailyReportsDirCheckTip = "Не удалось получить доступ к директории.";
                    break;
            }
            if (Directory.Exists(DailyReportsDir) && DailyReportsDir.CheckDirectoryRights(FileSystemRights.Write) is CheckDirectoryRightsResult.HasAccess)
            {
                DailyReportsDirCheckStatus = CheckStatus.Ok;
                return;
            }
            DailyReportsDirCheckStatus = CheckStatus.Error;
        });
    }


}
