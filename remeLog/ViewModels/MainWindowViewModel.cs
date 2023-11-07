using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
using remeLog.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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
            MessageBox.Show("Редактирование настроек.");
            //if (!WindowsUserDialogService.EditSettings()) return;
            //OnPropertyChanged(nameof(Machine));
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
        SettingsWindow settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }
    private bool CanTestProcessCommandExecute(object p) => true;
    #endregion

    #endregion

}
