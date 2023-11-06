using eLog.Infrastructure;
using eLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using eLog.Infrastructure.Extensions;
using Text = eLog.Infrastructure.Extensions.Text;
using libeLog.Extensions;

namespace eLog.Views.Windows.Dialogs;

/// <summary>
/// Логика взаимодействия для EndSetupDialogWindow.xaml
/// </summary>
public partial class EndDetailDialogWindow : Window, INotifyPropertyChanged, IDataErrorInfo
{
    private TimeSpan _MachineTime = TimeSpan.Zero;
    private int _PartsFinished;
    private string _MachineTimeText = string.Empty;
    private string _FinishedCount = string.Empty;
    private Visibility _KeyboardVisibility;
    public EndDetailResult EndDetailResult { get; set; }

    public Visibility KeyboardVisibility
    {
        get => _KeyboardVisibility;
        set
        {
            Set(ref _KeyboardVisibility, value);
            Height = _KeyboardVisibility is Visibility.Visible ? 436 : 296;
        }
    }

    public Part Part { get; set; }

    public string Status { get; set; }

    public string MachineTimeText
    {
        get => _MachineTimeText;
        set
        {
            Set(ref _MachineTimeText, value);
            if (_MachineTimeText.TimeParse(out _MachineTime))
            {
                Part.MachineTime = _MachineTime;
            }
            OnPropertyChanged(nameof(Valid));
            OnPropertyChanged(nameof(FinishedCount));
            Part.MachineTime = _MachineTime;
        }
    }

    public string FinishedCount
    {
        get => _FinishedCount;
        set
        {
            Set(ref _FinishedCount, value);
            OnPropertyChanged(nameof(Valid));
            Part.FinishedCount = _FinishedCount.GetInt();
            Status = Part.FinishedCount switch
            {
                0 when string.IsNullOrWhiteSpace(FinishedCount) => string.Empty,
                0 when !string.IsNullOrWhiteSpace(FinishedCount) &&
                       FinishedCount.Replace("0", "").Length == 0 =>
                    "При завершении с 0 будет выполнено неполное завершение наладки. (Машинное время должно быть пустым)",
                0 when !string.IsNullOrWhiteSpace(FinishedCount) =>
                    "Неверно указано количество изготовленных деталей.",
                _ => string.Empty,
            };
            
            
            OnPropertyChanged(nameof(Status));
        }
    }

    public bool Valid
    {
        get
        {
            var result = int.TryParse(FinishedCount, out _PartsFinished) && _PartsFinished > 0 &&
                         MachineTimeText.TimeParse(out _MachineTime) && _MachineTime.TotalSeconds > 0
                         || _PartsFinished == 0 && !string.IsNullOrWhiteSpace(_FinishedCount) &&
                         FinishedCount.Replace("0", "").Length == 0;
            if (!result) return result;
            Part.FinishedCount = _PartsFinished;
            Part.MachineTime = _MachineTime;
            return result;
        }
    }

    public EndDetailDialogWindow(Part part)
    {
        Part = part;
        Status = string.Empty;
        InitializeComponent();
    }

    public string Error { get; } = string.Empty;

    public string this[string columnName]
    {
        get
        {
            var error = string.Empty;
            switch (columnName)
            {
                case nameof(MachineTimeText):
                    if (Part.MachineTime == TimeSpan.Zero && FinishedCount != "0") error = Text.ValidationErrors.MachineTime;
                    break;
                case nameof(FinishedCount):
                    if (Part.FinishedCount == 0 && FinishedCount != "0") error = Text.ValidationErrors.FinishedCount;
                    break;
            }
            return error;
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

    private void KeyboardButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardVisibility = KeyboardVisibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PartsCountTextBox.Focus();
        KeyboardVisibility = Visibility.Collapsed;
    }

    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock { Parent: Grid grid }) return;

        foreach (UIElement gridChild in grid.Children)
        {
            if (gridChild is AdornedElementPlaceholder { AdornedElement: TextBox textBox } && Validation.GetErrors(textBox) is ICollection<ValidationError> { Count: > 0 } errors)
            {
                MessageBox.Show(errors.First().ErrorContent.ToString(), "Некорректный ввод", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
