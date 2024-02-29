using DocumentFormat.OpenXml.Bibliography;
using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using neLog.Infrastructure;
using neLog.Models;
using neLog.Views;
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

namespace neLog.ViewModels;

internal class MainWindowViewModel : ViewModel, IOverlay
{
    private readonly object lockObject = new object();
    
    public MainWindowViewModel()
    {
        CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
        EditSettingsCommand = new LambdaCommand(OnEditSettingsCommandExecuted, CanEditSettingsCommandExecute);
        ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
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
                AppSettings.Instance.StorageType = settings.StorageType; 
                AppSettings.Instance.StorageTablePath = settings.StorageTablePath.Value; 
                AppSettings.Instance.OrdersTablePath = settings.OrdersTablePath.Value; 
                AppSettings.Instance.WorkersTablePath = settings.WorkersTablePath.Value; 
                AppSettings.Instance.ConnectionString = settings.ConnectionString.Value;
                AppSettings.Instance.DebugMode = settings.DebugMode;
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


    #endregion


    private void BackgroundWorker()
    {
        while (true)
        {

        }
    }
}
