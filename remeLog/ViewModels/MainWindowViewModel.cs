using DocumentFormat.OpenXml.Bibliography;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
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

namespace remeLog.ViewModels;

internal class MainWindowViewModel : ViewModel, IOverlay
{
    public MainWindowViewModel()
    {
        CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
        EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
        ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
        TestProcessCommand = new LambdaCommand(OnTestProcessCommandExecuted, CanTestProcessCommandExecute);

        

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



    private DateTime _Date = DateTime.Today.AddDays(-1);
    /// <summary> Дата отчета </summary>
    public DateTime Date
    {
        get => _Date;
        set => Set(ref _Date, value);
    }

    private ObservableCollection<CombinedParts> _Parts = new();
    /// <summary> Объединенный список объединенных списков </summary>
    public ObservableCollection<CombinedParts> Parts
    {
        get => _Parts;
        set => Set(ref _Parts, value);
    }

    private CombinedParts _Skt21_104Parts = new(Machines.HuyndaiSkt21_104);
    /// <summary> Объединенный список по станку SKT21 #1 </summary>
    public CombinedParts Skt21_104Parts
    {
        get => _Skt21_104Parts;
        set => Set(ref _Skt21_104Parts, value);
    }

    private CombinedParts _Skt21_105Parts = new(Machines.HuyndaiSkt21_105);
    /// <summary> Объединенный список по станку SKT21 #2 </summary>
    public CombinedParts Skt21_105Parts
    {
        get => _Skt21_105Parts;
        set => Set(ref _Skt21_105Parts, value);
    }

    private CombinedParts _L230Parts = new(Machines.HuyndaiL230A);
    /// <summary> Объединенный список по станку L230 </summary>
    public CombinedParts L230Parts
    {
        get => _L230Parts;
        set => Set(ref _L230Parts, value);
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

    private CombinedParts _GS1500 = new (Machines.GoodwayGs1500);
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
        get => _XH6300;
        set => Set(ref _XH6300, value);
    }

    private CombinedParts _MV134 = new(Machines.QuaserMv134);
    /// <summary> Объединенный список по станку MV134 </summary>
    public CombinedParts MV134
    {
        get => _MV134;
        set => Set(ref _MV134, value);
    }

    private CombinedParts _i200 = new (Machines.MazakIntegrexI200);
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
                AppSettings.Save();
                Status = "Параметры сохранены";
            }
        }
    }
    private static bool CanEditSettingsCommandExecute(object p) => true;
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

    #region TestProcessCommand
    public ICommand TestProcessCommand { get; }
    private void OnTestProcessCommandExecuted(object p)
    {
        
        _ = LoadPartsAsync();
    }

    private async Task LoadPartsAsync()
    {
        await Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() => { Parts.Clear(); });
            Status = "Загрузка информации...";
            ProgressBarVisibility = Visibility.Visible;
            string[] machines = {
            Machines.HuyndaiSkt21_104,
            Machines.HuyndaiSkt21_105,
            Machines.HuyndaiL230A,
            Machines.MazakQts200Ml,
            Machines.GoodwayGs1500,
            Machines.MazakQts350,
            Machines.MazakNexus5000,
            Machines.HuyndaiXH6300,
            Machines.VictorA110,
            Machines.QuaserMv134,
            Machines.MazakIntegrexI200,
            };
            Skt21_104Parts.Parts.Clear();
            Skt21_105Parts.Parts.Clear();
            L230Parts.Parts.Clear();
            QTS200.Parts.Clear();
            GS1500.Parts.Clear();
            QTS350.Parts.Clear();
            N5000.Parts.Clear();
            XH6300.Parts.Clear();
            A110.Parts.Clear();
            MV134.Parts.Clear();
            i200.Parts.Clear();
            

            List<Part> data = new List<Part>();
            for (int i = 0; i < 5000000; i++)
            {
                data.Add(new Part(i, "60%", "100%", "Просто", DateTime.Today, machines[new Random().Next(machines.Length)], "День", "Бабохин К.Г.", "Корпус клапана АР110-01-001", "УЧ-3/00045.1.1", 10, 1, DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1), DateTime.Now, 60, 40, 60, 75, 10, 15));
            }

            foreach (Part part in data)
            {
                switch (part.Machine)
                {
                    case Machines.HuyndaiSkt21_104:
                        Skt21_104Parts.Parts.Add(part);
                        break;
                    case Machines.HuyndaiSkt21_105:
                        Skt21_105Parts.Parts.Add(part);
                        break;
                    case Machines.HuyndaiL230A:
                        L230Parts.Parts.Add(part);
                        break;
                    case Machines.MazakQts200Ml:
                        QTS200.Parts.Add(part);
                        break;
                    case Machines.GoodwayGs1500:
                        GS1500.Parts.Add(part);
                        break;
                    case Machines.MazakQts350:
                        QTS350.Parts.Add(part);
                        break;
                    case Machines.MazakNexus5000:
                        N5000.Parts.Add(part);
                        break;
                    case Machines.HuyndaiXH6300:
                        XH6300.Parts.Add(part);
                        break;
                    case Machines.VictorA110:
                        A110.Parts.Add(part);
                        break;
                    case Machines.QuaserMv134:
                        MV134.Parts.Add(part);
                        break;
                    case Machines.MazakIntegrexI200:
                        i200.Parts.Add(part);
                        break;
                    default:
                        break;
                }
            }
            Application.Current.Dispatcher.Invoke(() => {
                Parts.Add(Skt21_104Parts);
                Parts.Add(Skt21_105Parts);
                Parts.Add(L230Parts);
                Parts.Add(QTS200);
                Parts.Add(GS1500);
                Parts.Add(QTS350);
                Parts.Add(N5000);
                Parts.Add(XH6300);
                Parts.Add(A110);
                Parts.Add(MV134);
                Parts.Add(i200);
            });
            Status = "";
            ProgressBarVisibility = Visibility.Collapsed;
        });
    }

    private bool CanTestProcessCommandExecute(object p) => true;
    #endregion

    #endregion

    private void UpdateParts()
    {
        
        
    } 

    private void BackgroundWorker()
    {
        while (true)
        {

        }
    }
}
