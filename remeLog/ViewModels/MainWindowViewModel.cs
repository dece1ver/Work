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
using System.Windows.Input;
using System.Windows.Threading;
using static libeLog.Constants;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace remeLog.ViewModels
{
    internal class MainWindowViewModel : ViewModel, IOverlay
    {
        private readonly object lockObject = new object();

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

        private CombinedParts _Skt21_104 = new(Machines.HuyndaiSkt21_104);
        /// <summary> Объединенный список по станку SKT21 #1 </summary>
        public CombinedParts Skt21_104
        {
            get => _Skt21_104;
            set => Set(ref _Skt21_104, value);
        }

        private CombinedParts _Skt21_105 = new(Machines.HuyndaiSkt21_105);
        /// <summary> Объединенный список по станку SKT21 #2 </summary>
        public CombinedParts Skt21_105
        {
            get => _Skt21_105;
            set => Set(ref _Skt21_105, value);
        }

        private CombinedParts _L230 = new(Machines.HuyndaiL230A);
        /// <summary> Объединенный список по станку L230 </summary>
        public CombinedParts L230
        {
            get => _L230;
            set => Set(ref _L230, value);
        }

        private CombinedParts _QTS200 = new(Machines.MazakQts200Ml);
        /// <summary> Объединенный список по станку QTS200 </summary>
        public CombinedParts QTS200
        {
            get => _QTS200;
            set => Set(ref _QTS200, value);
        }

        private CombinedParts _QTS350 = new(Machines.MazakQts350);
        /// <summary> Объединенный список по станку QTS350 </summary>
        public CombinedParts QTS350
        {
            get => _QTS350;
            set => Set(ref _QTS350, value);
        }

        private CombinedParts _GS1500 = new(Machines.GoodwayGs1500);
        /// <summary> Объединенный список по станку GS1500 </summary>
        public CombinedParts GS1500
        {
            get => _GS1500;
            set => Set(ref _GS1500, value);
        }

        private CombinedParts _N5000 = new(Machines.MazakNexus5000);
        /// <summary> Объединенный список по станку Nexus5000 </summary>
        public CombinedParts N5000
        {
            get => _N5000;
            set => Set(ref _N5000, value);
        }

        private CombinedParts _XH6300 = new(Machines.HuyndaiXH6300);
        /// <summary> Объединенный список по станку XH6300 </summary>
        public CombinedParts XH6300
        {
            get => _XH6300;
            set => Set(ref _XH6300, value);
        }

        private CombinedParts _A110 = new(Machines.VictorA110);
        /// <summary> Объединенный список по станку Victor </summary>
        public CombinedParts A110
        {
            get => _A110;
            set => Set(ref _A110, value);
        }

        private CombinedParts _MV134 = new(Machines.QuaserMv134);
        /// <summary> Объединенный список по станку MV134 </summary>
        public CombinedParts MV134
        {
            get => _MV134;
            set => Set(ref _MV134, value);
        }

        private CombinedParts _i200 = new(Machines.MazakIntegrexI200);
        /// <summary> Объединенный список по станку _i200 </summary>
        public CombinedParts i200
        {
            get => _i200;
            set => Set(ref _i200, value);
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
                    Status = "Загрузка информации...";
                    Application.Current.Dispatcher.Invoke(() => { Parts.Clear(); });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Skt21_104.FromDate = FromDate;
                        Skt21_104.ToDate = ToDate;
                        Skt21_104.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Skt21_105.FromDate = FromDate;
                        Skt21_105.ToDate = ToDate;
                        Skt21_105.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        L230.FromDate = FromDate;
                        L230.ToDate = ToDate;
                        L230.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        QTS200.FromDate = FromDate;
                        QTS200.ToDate = ToDate;
                        QTS200.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GS1500.FromDate = FromDate;
                        GS1500.ToDate = ToDate;
                        GS1500.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        QTS350.FromDate = FromDate;
                        QTS350.ToDate = ToDate;
                        QTS350.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        N5000.FromDate = FromDate;
                        N5000.ToDate = ToDate;
                        N5000.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        XH6300.FromDate = FromDate;
                        XH6300.ToDate = ToDate;
                        XH6300.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        A110.FromDate = FromDate;
                        A110.ToDate = ToDate;
                        A110.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MV134.FromDate = FromDate;
                        MV134.ToDate = ToDate;
                        MV134.Parts.Clear();
                    });
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        i200.FromDate = FromDate;
                        i200.ToDate = ToDate;
                        i200.Parts.Clear();
                    });


                    ObservableCollection<Part> data = new ObservableCollection<Part>();

                    try
                    {
                        data = Database.ReadPartsByShiftDate(FromDate, ToDate);

                        foreach (Part part in data)
                        {
                            switch (part.Machine)
                            {
                                case Machines.HuyndaiSkt21_104:
                                    Application.Current.Dispatcher.Invoke(() => { Skt21_104.Parts.Add(part); });
                                    break;
                                case Machines.HuyndaiSkt21_105:
                                    Application.Current.Dispatcher.Invoke(() => { Skt21_105.Parts.Add(part); });
                                    break;
                                case Machines.HuyndaiL230A:
                                    Application.Current.Dispatcher.Invoke(() => { L230.Parts.Add(part); });
                                    break;
                                case Machines.MazakQts200Ml:
                                    Application.Current.Dispatcher.Invoke(() => { QTS200.Parts.Add(part); });
                                    break;
                                case Machines.GoodwayGs1500:
                                    Application.Current.Dispatcher.Invoke(() => { GS1500.Parts.Add(part); });
                                    break;
                                case Machines.MazakQts350:
                                    Application.Current.Dispatcher.Invoke(() => { QTS350.Parts.Add(part); });
                                    break;
                                case Machines.MazakNexus5000:
                                    Application.Current.Dispatcher.Invoke(() => { N5000.Parts.Add(part); });
                                    break;
                                case Machines.HuyndaiXH6300:
                                    Application.Current.Dispatcher.Invoke(() => { XH6300.Parts.Add(part); });
                                    break;
                                case Machines.VictorA110:
                                    Application.Current.Dispatcher.Invoke(() => { A110.Parts.Add(part); });
                                    break;
                                case Machines.QuaserMv134:
                                    Application.Current.Dispatcher.Invoke(() => { MV134.Parts.Add(part); });
                                    break;
                                case Machines.MazakIntegrexI200:
                                    Application.Current.Dispatcher.Invoke(() => { i200.Parts.Add(part); });
                                    break;
                                default:
                                    break;
                            }
                        }
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Parts.Add(Skt21_104);
                            Parts.Add(Skt21_105);
                            Parts.Add(L230);
                            Parts.Add(QTS200);
                            Parts.Add(GS1500);
                            Parts.Add(QTS350);
                            Parts.Add(N5000);
                            Parts.Add(XH6300);
                            Parts.Add(A110);
                            Parts.Add(MV134);
                            Parts.Add(i200);
                        });
                    }
                    catch (SqlException sqlEx)
                    {
                        var (_, message) = sqlEx.Number switch
                        {
                            -1 => (DbResult.Error, StatusTips.NoConnectionToDb),
                            18456 => (DbResult.AuthError, StatusTips.AuthFailedToDb),
                            _ => (DbResult.Error, $"Ошибка БД №{sqlEx.Number}"),
                        };
                        MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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