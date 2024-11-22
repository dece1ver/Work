using DocumentFormat.OpenXml.Office2019.Drawing.Diagram11;
using eLog.Infrastructure;
using eLog.Models;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace eLog.Views.Windows.Settings
{
    /// <summary>
    /// Сохранение настроек происходит в вызывающем окне при DialogResult == true
    /// </summary>
    public partial class AppSettingsWindow : Window, INotifyPropertyChanged
    {
        private string _XlPath;
        private string _GoogleCredentialsPath;
        private string _GsId;
        private bool _WriteToGs;
        private string _OrdersSourcePath;
        private Machine _Machine;
        private string[] _OrderQualifiers;
        private bool _DebugMode;
        private StorageType _StorageType;
        private string _ConnectionString;
        private string _SmtpAddress;
        private int _SmtpPort;
        private string _SmtpUsername;
        private string _PathToRecievers;
        private int _secretMenuCounter;

        public List<StorageType> StorageTypes { get; }
        public IReadOnlyList<Machine> Machines { get; }

        public string XlPath
        {
            get => _XlPath;
            set => Set(ref _XlPath, value);
        }

        public string OrdersSourcePath
        {
            get => _OrdersSourcePath;
            set => Set(ref _OrdersSourcePath, value);
        }

        public string GoogleCredentialsPath
        {
            get => _GoogleCredentialsPath;
            set => Set(ref _GoogleCredentialsPath, value);
        }

        public string GsId
        {
            get => _GsId;
            set => Set(ref _GsId, value);
        }



        /// <summary> Писать ли в гугл таблицу </summary>
        public bool WriteToGs
        {
            get => _WriteToGs;
            set => Set(ref _WriteToGs, value);
        }


        public Machine Machine
        {
            get => _Machine;
            set => Set(ref _Machine, value);
        }

        public string[] OrderQualifiers
        {
            get => _OrderQualifiers;
            set => Set(ref _OrderQualifiers, value);

        }

        public StorageType StorageType
        {
            get => _StorageType;
            set => Set(ref _StorageType, value);
        }


        public bool DebugMode
        {
            get => _DebugMode;
            set => Set(ref _DebugMode, value);
        }

        public string ConnectionString
        {
            get => _ConnectionString;
            set => Set(ref _ConnectionString, value);
        }


        
        /// <summary> Адрес SMTP сервера </summary>
        public string SmtpAddress
        {
            get => _SmtpAddress;
            set => Set(ref _SmtpAddress, value);
        }


        
        /// <summary> SMTP порт </summary>
        public int SmtpPort
        {
            get => _SmtpPort;
            set => Set(ref _SmtpPort, value);
        }


        
        /// <summary> Отправитель </summary>
        public string SmtpUsername
        {
            get => _SmtpUsername;
            set => Set(ref _SmtpUsername, value);
        }


        
        /// <summary> Путь к файлу с получателями </summary>
        public string PathToRecievers
        {
            get => _PathToRecievers;
            set => Set(ref _PathToRecievers, value);
        }


        private int _TimerForNotify;
        /// <summary> Таймер для уведомлений (в часах) </summary>
        public int TimerForNotify
        {
            get => _TimerForNotify;
            set => Set(ref _TimerForNotify, value);
        }

        private bool _EnableWriteShiftHandover;
        /// <summary> Передача смены </summary>
        public bool EnableWriteShiftHandover
        {
            get => _EnableWriteShiftHandover;
            set => Set(ref _EnableWriteShiftHandover, value);
        }


        public AppSettingsWindow()
        {
            Machines = AppSettings.Machines;
            StorageTypes = new List<StorageType>()
            {
                new StorageType(StorageType.Types.Excel),
                new StorageType(StorageType.Types.Database),
                new StorageType (StorageType.Types.All)
            };
            _StorageType = AppSettings.Instance.StorageType;
            _XlPath = AppSettings.Instance.XlPath;
            _OrdersSourcePath = AppSettings.Instance.OrdersSourcePath;
            _OrderQualifiers = AppSettings.Instance.OrderQualifiers;
            _Machine = Machines.First(x => x.Id == AppSettings.Instance.Machine.Id);
            _GoogleCredentialsPath = AppSettings.Instance.GoogleCredentialsPath ?? "";
            _GsId = AppSettings.Instance.GsId ?? "";
            _WriteToGs = AppSettings.Instance.WiteToGs;
            _ConnectionString = AppSettings.Instance.ConnectionString ?? "";
            _SmtpAddress = AppSettings.Instance.SmtpAddress ?? "";
            _SmtpPort = AppSettings.Instance.SmtpPort;
            _SmtpUsername = AppSettings.Instance.SmtpUsername ?? "";
            _PathToRecievers = AppSettings.Instance.PathToRecievers ?? "";
            _TimerForNotify = AppSettings.Instance.TimerForNotify;
            _EnableWriteShiftHandover = AppSettings.Instance.EnableWriteShiftHandover;
            _DebugMode = AppSettings.Instance.DebugMode;
            InitializeComponent();
        }

        private void SetXlPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Книга Excel с макросами (*.xlsm)|*.xlsm|Книга Excel(*.xlsx)|*.xlsx",
                DefaultExt = "xlsm"
            };
            if (dlg.ShowDialog() != true) return;
            XlPath = dlg.FileName;
        }
        private void SetOrdersSourceButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };
            if (dlg.ShowDialog() != true) return;
            OrdersSourcePath = dlg.FileName;
        }

        private void ExportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string saveDir = "";
                if (File.Exists(XlPath) && Directory.GetParent(XlPath) is { FullName: { } parent })
                {
                    saveDir = parent;
                }
                if (string.IsNullOrEmpty(saveDir))
                {
                    var dlg = new CommonOpenFileDialog
                    {
                        Title = "Расположение экспорта конфигурации.",
                        IsFolderPicker = true,
                        AddToMostRecentlyUsedList = false,
                        AllowNonFileSystemItems = false,
                        EnsureFileExists = true,
                        EnsurePathExists = true,
                        EnsureReadOnly = false,
                        EnsureValidNames = true,
                        Multiselect = false,
                        ShowPlacesList = true
                    };

                    if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        saveDir = dlg.FileName!;
                    }
                    if (string.IsNullOrEmpty(saveDir)) return;

                }
                var tempPath = Path.Combine(saveDir, "configs", AppSettings.Instance.Machine.SafeName, DateTime.Now.ToString("dd-MM-yy HH-mm"));
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                var exportPath = Path.Combine(tempPath, "config.json");
                File.Copy(AppSettings.ConfigFilePath, exportPath);
                MessageBox.Show($"Параменры экспортированы:\n{exportPath}", "Экспорт");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось выполнить экспорт из-за непредвиденной ошибки.\n{ex.Message}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new()
                {
                    Filter = "JSON (*.json)|*.json",
                    DefaultExt = "json"
                };
                if (dlg.ShowDialog() != true) return;
                var json = File.ReadAllText(dlg.FileName);
                var settings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                var parts = AppSettings.Instance.Parts;
                var machine = AppSettings.Instance.Machine;
                var operators = AppSettings.Instance.Operators;
                var currentOperator = AppSettings.Instance.CurrentOperator;
                var currentShift = AppSettings.Instance.CurrentShift;
                var isShiftStarted = AppSettings.Instance.IsShiftStarted;

                var tempName = $"{AppSettings.ConfigFilePath}.temp";
                File.Copy(AppSettings.ConfigFilePath, tempName, true);
                JsonConvert.PopulateObject(json, AppSettings.Instance, settings);

                AppSettings.Instance.Parts = parts;
                AppSettings.Instance.Machine = machine;
                AppSettings.Instance.Operators = operators;
                AppSettings.Instance.CurrentOperator = currentOperator;
                AppSettings.Instance.CurrentShift = currentShift;
                AppSettings.Instance.IsShiftStarted = isShiftStarted;

                StorageType = AppSettings.Instance.StorageType;
                XlPath = AppSettings.Instance.XlPath;
                OrdersSourcePath = AppSettings.Instance.OrdersSourcePath;
                OrderQualifiers = AppSettings.Instance.OrderQualifiers;
                Machine = Machines.First(x => x.Id == AppSettings.Instance.Machine.Id);
                GoogleCredentialsPath = AppSettings.Instance.GoogleCredentialsPath ?? "";
                GsId = AppSettings.Instance.GsId ?? "";
                WriteToGs = AppSettings.Instance.WiteToGs;
                ConnectionString = AppSettings.Instance.ConnectionString ?? "";
                SmtpAddress = AppSettings.Instance.SmtpAddress ?? "";
                SmtpPort = AppSettings.Instance.SmtpPort;
                SmtpUsername = AppSettings.Instance.SmtpUsername ?? "";
                PathToRecievers = AppSettings.Instance.PathToRecievers ?? "";
                TimerForNotify = AppSettings.Instance.TimerForNotify;
                DebugMode = AppSettings.Instance.DebugMode;

                File.Copy(tempName, AppSettings.ConfigFilePath, true);
                AppSettings.Instance.ReadConfig();
                File.Delete(tempName);


                MessageBox.Show($"Параметры импортированы", "Импорт");
            }
            catch (Exception ex)
            {
                AppSettings.Instance.ReadConfig();
                MessageBox.Show($"Не удалось выполнить импорт конфигурации.\n{ex.Message}", "Импорт", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null!)
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

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null!)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }


        #endregion

        private void CheckDbConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    connection.Close();
                    MessageBox.Show("Ок", $"Подключение доступно.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException sqlEx)
            {
                switch (sqlEx.Number)
                {
                    case 18456:
                        MessageBox.Show($"Ошибка авторизации №{sqlEx.Number}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show($"Ошибка №{sqlEx.Number}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}", $"{ex.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _secretMenuCounter++;
            if (_secretMenuCounter >= 5)
            {
                ConnectionStringTextBox.IsEnabled = true;
            }
        }

        private void SetGoogleCredentialsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "JSON (*.json)|*.json",
                DefaultExt = "json"
            };
            if (dlg.ShowDialog() != true) return;
            GoogleCredentialsPath = dlg.FileName;
        }

        private void SetPathToRecieversButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new();
            if (dlg.ShowDialog() != true) return;
            PathToRecievers = dlg.FileName;
        }
    }
}