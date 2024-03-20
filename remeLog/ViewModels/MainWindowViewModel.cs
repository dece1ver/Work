using DocumentFormat.OpenXml.Bibliography;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using remeLog.Infrastructure;
using remeLog.Models;
using remeLog.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static libeLog.Constants;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace remeLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
        private readonly object lockObject = new();

        public MainWindowViewModel()
        {
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
            LoadPartsInfoCommand = new LambdaCommand(OnLoadPartsInfoCommandExecuted, CanLoadPartsInfoCommandExecute);
            ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
            ShowPartsInfoCommand = new LambdaCommand(OnShowPartsInfoCommandExecuted, CanShowPartsInfoCommandExecute);
            IncreaseDateCommand = new LambdaCommand(OnIncreaseDateCommandExecuted, CanIncreaseDateCommandExecute);
            DecreaseDateCommand = new LambdaCommand(OnDecreaseDateCommandExecuted, CanDecreaseDateCommandExecute);
            SetYesterdayDateCommand = new LambdaCommand(OnSetYesterdayDateCommandExecuted, CanSetYesterdayDateCommandExecute);
            SetWeekDateCommand = new LambdaCommand(OnSetWeekDateCommandExecuted, CanSetWeekDateCommandExecute);
            _Machines = new();
            if (AppSettings.Instance.InstantUpdateOnMainWindow) { _ = LoadPartsAsync(); }
            // var backgroundWorker = new Thread(BackgroundWorker) { IsBackground = true };
            // backgroundWorker.Start();
        }


        private Overlay _Overlay = new(false);

        public Overlay Overlay
        {
            get => _Overlay;
            set => Set(ref _Overlay, value);
        }

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

        public Visibility ProgressBarVisibility
        {
            get => _ProgressBarVisibility;
            set => Set(ref _ProgressBarVisibility, value);
        }

        private Visibility _ProgressBarVisibility = Visibility.Hidden;



        private DateTime _FromDate = DateTime.Today.AddDays(-1);
        public DateTime FromDate
        {
            get => _FromDate;
            set
            {
                if (Set(ref _FromDate, value) && AppSettings.Instance.InstantUpdateOnMainWindow)
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private DateTime _ToDate = DateTime.Today.AddDays(-1);
        public DateTime ToDate
        {
            get => _ToDate;
            set
            {
                if (Set(ref _ToDate, value) && AppSettings.Instance.InstantUpdateOnMainWindow)
                {
                    _ = LoadPartsAsync();
                }
            }
        }

        private ObservableCollection<CombinedParts> _Parts = new();
        /// <summary> Объединенный список объединенных списков </summary>
        public ObservableCollection<CombinedParts> Parts
        {
            get => _Parts;
            set => Set(ref _Parts, value);
        }



        private List<string> _Machines;
        /// <summary> Описание </summary>
        public List<string> Machines
        {
            get => _Machines;
            set => Set(ref _Machines, value);
        }


        #region Команды

        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }
        private static void OnCloseApplicationCommandExecuted(object p)
        {
            Application.Current.Shutdown();
        }
        private static bool CanCloseApplicationCommandExecute(object p) => true;
        #endregion

        #region EditSettings
        public ICommand EditSettingsCommand { get; }
        private void OnEditSettingsCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                SettingsWindow settingsWindow = new SettingsWindow() { Owner = Application.Current.MainWindow };
                if (settingsWindow.ShowDialog() == true && settingsWindow.DataContext is SettingsWindowViewModel settings)
                {
                    AppSettings.Instance.DataSource = settings.DataSource;
                    AppSettings.Instance.SourcePath = settings.SourcePath.Value;
                    AppSettings.Instance.ReportsPath = settings.ReportsPath.Value;
                    AppSettings.Instance.DailyReportsDir = settings.DailyReportsDir.Value;
                    AppSettings.Instance.ConnectionString = settings.ConnectionString.Value;
                    AppSettings.Instance.InstantUpdateOnMainWindow = settings.InstantUpdateOnMainWindow;
                    AppSettings.Save();
                    Status = "Параметры сохранены";
                }
            }
        }
        private static bool CanEditSettingsCommandExecute(object p) => true;
        #endregion

        #region LoadPartsInfo
        public ICommand LoadPartsInfoCommand { get; }
        private async void OnLoadPartsInfoCommandExecuted(object p)
        {
            await LoadPartsAsync();
        }
        private static bool CanLoadPartsInfoCommandExecute(object p) => true;
        #endregion

        #region ShowAbout
        public ICommand ShowAboutCommand { get; }
        private void OnShowAboutCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                MessageBox.Show("О программе.");
            }
        }
        private static bool CanShowAboutCommandExecute(object p) => true;
        #endregion

        #region ShowPartsInfo
        public ICommand ShowPartsInfoCommand { get; }
        private void OnShowPartsInfoCommandExecuted(object p)
        {
            using (Overlay = new())
            {
                var partsInfo = (CombinedParts)p;
                partsInfo.FromDate = FromDate;
                partsInfo.ToDate = ToDate;
                var partsInfoWindow = new PartsInfoWindow(partsInfo) { Owner = Application.Current.MainWindow };
                partsInfoWindow.Show();
            }
        }
        private static bool CanShowPartsInfoCommandExecute(object p) => true;
        #endregion

        #region IncreaseDateCommand
        public ICommand IncreaseDateCommand { get; }
        private void OnIncreaseDateCommandExecuted(object p)
        {

            FromDate = FromDate.AddDays(1);
            ToDate = ToDate.AddDays(1);
        }
        private bool CanIncreaseDateCommandExecute(object p) => true;
        #endregion

        #region DecreaseDateCommand
        public ICommand DecreaseDateCommand { get; }
        private void OnDecreaseDateCommandExecuted(object p)
        {

            FromDate = FromDate.AddDays(-1);
            ToDate = ToDate.AddDays(-1);
        }
        private bool CanDecreaseDateCommandExecute(object p) => true;
        #endregion

        #region SetYesterdayDateCommand
        public ICommand SetYesterdayDateCommand { get; }
        private void OnSetYesterdayDateCommandExecuted(object p)
        {

            FromDate = DateTime.Today.AddDays(-1);
            ToDate = FromDate;
        }
        private bool CanSetYesterdayDateCommandExecute(object p) => true;
        #endregion

        #region SetWeekDateCommand
        public ICommand SetWeekDateCommand { get; }
        private void OnSetWeekDateCommandExecuted(object p)
        {

            FromDate = ToDate.AddDays(-7);
        }
        private bool CanSetWeekDateCommandExecute(object p) => true;
        #endregion

        #endregion

        private async Task LoadPartsAsync()
        {
            
            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    ProgressBarVisibility = Visibility.Visible;
                    Status = "Получение списка станков...";
                    switch (Machines.ReadMachines())
                    {
                        case DbResult.AuthError:
                            MessageBox.Show("Не удалось получить спискок станков из-за неудачной авторизации в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case DbResult.Error:
                            MessageBox.Show("Не удалось получить спискок станков из-за ошибки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case DbResult.NoConnection:
                            MessageBox.Show("Нет соединения с базой данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Parts.Clear();
                        foreach (var machine in Machines)
                        {
                            Parts.Add(new CombinedParts(machine, FromDate, ToDate));
                        }
                        return true;
                    });
                    Status = "Загрузка информации...";
                    foreach (var part in Parts)
                    {
                        try
                        {
                            part.Parts = Database.ReadPartsByShiftDateAndMachine(FromDate, ToDate, part.Machine);
                        }
                        catch (SqlException sqlEx)
                        {
                            var message = sqlEx.Number switch
                            {
                                SqlErrorCode.NoConnection => StatusTips.NoConnectionToDb,
                                SqlErrorCode.AuthError => StatusTips.AuthFailedToDb,
                                _ => $"Ошибка БД №{sqlEx.Number}\n{sqlEx.Message}",
                            };
                            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    Status = "";
                    ProgressBarVisibility = Visibility.Collapsed;
                }
            });
        }

        private void BackgroundWorker()
        {
            while (true)
            {

            }
        }
    }
}