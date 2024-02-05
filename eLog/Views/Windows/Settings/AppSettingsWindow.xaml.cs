using DocumentFormat.OpenXml.Office2019.Drawing.Diagram11;
using eLog.Infrastructure;
using eLog.Models;
using libeLog.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace eLog.Views.Windows.Settings;

/// <summary>
/// Логика взаимодействия для AppSettingsWindow.xaml
/// </summary>
public partial class AppSettingsWindow : Window, INotifyPropertyChanged
{
    private string _XlPath;
    private string _OrdersSourcePath;
    private Machine _Machine;
    private string[] _OrderQualifiers;
    private bool _DebugMode;
    private StorageType _StorageType;
    private string _ConnectionString;
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


    public AppSettingsWindow()
    {
        Machines = Enumerable.Range(0, 13).Select(i => new Machine(i)).ToList();
        StorageTypes = new List<StorageType>() 
        {
            new StorageType(StorageType.Types.Excel),
            new StorageType(StorageType.Types.Database),
            new StorageType (StorageType.Types.All) 
        };
        _XlPath = AppSettings.Instance.XlPath;
        _OrdersSourcePath = AppSettings.Instance.OrdersSourcePath;
        _OrderQualifiers = AppSettings.Instance.OrderQualifiers;
        _Machine = Machines.First(x => x.Id == AppSettings.Instance.Machine.Id);
        if (AppSettings.Instance.StorageType is null)
        {
            _StorageType = new StorageType(StorageType.Types.Excel);
        } 
        else
        {
            _StorageType = StorageTypes.First(x => x.Type == AppSettings.Instance.StorageType.Type);
        }
        _ConnectionString = AppSettings.Instance.ConnetctionString ?? "";
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
                    saveDir = dlg.FileName;
                    // Do something with selected folder string
                }
                //SaveFileDialog saveFileDialog = new SaveFileDialog()
                //{
                //    Filter = "Данные JSON (*.json)|*.json",
                //};
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
}
