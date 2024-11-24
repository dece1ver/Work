using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Infrastructure;
using libeLog.Models;
using remeLog.Infrastructure;
using remeLog.Infrastructure.Types;
using remeLog.Models;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace remeLog.ViewModels
{
    internal class SettingsWindowViewModel : ViewModel
    {
        public SettingsWindowViewModel()
        {
            SetQualificationSourceTableCommand = new LambdaCommand(OnSetQualificationSourceTableCommandExecuted, CanSetQualificationSourceTableCommandExecute);
            SetReportsTableCommand = new LambdaCommand(OnSetReportsTableCommandExecuted, CanSetReportsTableCommandExecute);
            SetDailyReportsDirCommand = new LambdaCommand(OnSetDailyReportsDirCommandExecuted, CanSetDailyReportsDirCommandExecute);
            CheckConnectionStringCommand = new LambdaCommand(OnCheckConnectionStringCommandExecuted, CanCheckConnectionStringCommandExecute);

            _DataSource = AppSettings.Instance.DataSource;
            _QualificationSourcePath = new SettingsItem(AppSettings.Instance.QualificationSourcePath ?? "");
            _ReportsPath = new SettingsItem(AppSettings.Instance.ReportsPath ?? "");
            _DailyReportsDir = new SettingsItem(AppSettings.Instance.DailyReportsDir ?? "");
            _ConnectionString = new SettingsItem(AppSettings.Instance.ConnectionString ?? "");
            _InstantUpdateOnMainWindow = AppSettings.Instance.InstantUpdateOnMainWindow;
            Role = AppSettings.Instance.User ??= User.Viewer;

            _ = CheckSourceAsync();
            _ = CheckReportsAsync();
            _ = CheckDailyReportsDirAsync();
            _ = CheckConnectionStringAsync();
        }

        #region Свойства

        public List<DataSource> DataSourceTypes { get; set; } = new() 
        {
            new DataSource(DataSource.Types.Database),
            new DataSource(DataSource.Types.Excel),
        };

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


        private DataSource _DataSource;
        /// <summary> Описание </summary>
        public DataSource DataSource
        {
            get => _DataSource;
            set => Set(ref _DataSource, value);
        }


        private SettingsItem _QualificationSourcePath;
        /// <summary> Путь к таблице с разрядами</summary>
        public SettingsItem QualificationSourcePath
        {
            get => _QualificationSourcePath;
            set => Set(ref _QualificationSourcePath, value);
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


        private User _Role;
        /// <summary> Роль пользователя </summary>
        public User Role
        {
            get => _Role;
            set => Set(ref _Role, value);
        }


        private bool _InstantUpdateOnMainWindow;
        /// <summary> Описание </summary>
        public bool InstantUpdateOnMainWindow
        {
            get => _InstantUpdateOnMainWindow;
            set => Set(ref _InstantUpdateOnMainWindow, value);
        }


        #endregion

        #region Команды

        #region SetQualificationSourceTable
        public ICommand SetQualificationSourceTableCommand { get; }
        private void OnSetQualificationSourceTableCommandExecuted(object p)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            if (dlg.ShowDialog() != true) return;
            QualificationSourcePath.Value = dlg.FileName;
            _ = CheckSourceAsync();
        }
        private static bool CanSetQualificationSourceTableCommandExecute(object p) => true;
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
            FolderBrowserDialog dlg = new();
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
                QualificationSourcePath.Status = Status.Sync;
                QualificationSourcePath.Tip = Constants.StatusTips.Checking;
                if (File.Exists(QualificationSourcePath.Value))
                {
                    QualificationSourcePath.Status = Status.Ok;
                    QualificationSourcePath.Tip = Constants.StatusTips.Ok;
                    return;
                }
                QualificationSourcePath.Status = Status.Error;
                QualificationSourcePath.Tip = Constants.StatusTips.NoFile;
            });
        }

        private async Task CheckReportsAsync()
        {
            await Task.Run(() =>
            {
                ReportsPath.Status = Status.Sync;
                ReportsPath.Tip = Constants.StatusTips.Checking;
                if (File.Exists(ReportsPath.Value))
                {
                    ReportsPath.Status = Status.Ok;
                    ReportsPath.Tip = Constants.StatusTips.Ok;
                    return;
                }
                ReportsPath.Status = Status.Error;
                ReportsPath.Tip = Constants.StatusTips.NoFile;
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
                        ConnectionString.Status = Status.Error;
                        break;
                    case DbResult.Error:
                        ConnectionString.Status = Status.Error;
                        break;
                }
                ConnectionString.Tip = res.message;
            });
        }
    }
}