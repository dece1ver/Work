using libeLog;
using libeLog.Base;
using libeLog.Extensions;
using libeLog.Interfaces;
using libeLog.Models;
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

internal class SettingsWindowViewModel : ViewModel
{
    public SettingsWindowViewModel()
    {
        ShowAboutCommand = new LambdaCommand(OnShowAboutCommandExecuted, CanShowAboutCommandExecute);
    }

    private string _Status = string.Empty;
    /// <summary> Статус </summary>
    public string Status
    {
        get => _Status;
        set => Set(ref _Status, value);
    }


    #region Команды

    private static bool CanCloseApplicationCommandExecute(object p) => true;

    #region ShowAbout
    public ICommand ShowAboutCommand { get; }
    private void OnShowAboutCommandExecuted(object p)
    {
        
    }
    private static bool CanShowAboutCommandExecute(object p) => true;
    #endregion

    #endregion

}
