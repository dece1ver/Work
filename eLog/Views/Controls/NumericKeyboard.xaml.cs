using libeLog.WinApi.Windows;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Keyboard = libeLog.WinApi.Windows.Keyboard;

namespace eLog.Views.Controls;

/// <summary>
/// Логика взаимодействия для NumericKeyboard.xaml
/// </summary>
public partial class NumericKeyboard : UserControl
{
    private const int firstPressDelay = 500;
    private const int everyPressDelay = 50;
    private bool isBackspacePressed = false;
    private bool isDelPressed = false;
    private DispatcherTimer backspaceRepeatTimer;
    private DispatcherTimer delRepeatTimer;


    public NumericKeyboard()
    {
        InitializeComponent();
        backspaceRepeatTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(everyPressDelay) };
        delRepeatTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(everyPressDelay) };
        backspaceRepeatTimer.Tick += BackspaceRepeatTimer_Tick;
        delRepeatTimer.Tick += DelRepeatTimer_Tick; ;
    }

    private void DelRepeatTimer_Tick(object? sender, EventArgs e)
    {
        if (isDelPressed)
        {
            Keyboard.KeyPress(Keys.Delete);
        }
    }

    private void BackspaceRepeatTimer_Tick(object? sender, EventArgs e)
    {
        if (isBackspacePressed)
        {
            Keyboard.KeyPress(Keys.Back);
        }
    }

    private void Button7_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D7);
    }

    private void Button8_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D8);
    }

    private void Button9_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D9);
    }


    private async Task BackspaceClick()
    {

        Keyboard.KeyPress(Keys.Back);
        isBackspacePressed = true;
        await Task.Delay(firstPressDelay);
        if (isBackspacePressed) backspaceRepeatTimer.Start();
    }

    private async Task DelClick()
    {

        Keyboard.KeyPress(Keys.Delete);
        isDelPressed = true;
        await Task.Delay(firstPressDelay);
        if (isDelPressed) delRepeatTimer.Start();
    }

    private void Button4_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D4);
    }

    private void Button5_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D5);
    }

    private void Button6_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D6);
    }

    private void ButtonDel_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Delete);
    }

    private void Button1_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D1);
    }

    private void Button2_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D2);
    }

    private void Button3_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D3);
    }

    private void ButtonClear_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.A);
        Keyboard.KeyUp(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Delete);
    }

    private void Button0_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.D0);
    }

    private void ButtonDot_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(KeyboardLayout.Current is KeyboardLayout.En ? Keys.OemPeriod : Keys.Oem2);
    }

    private void ButtonColon_Click(object sender, RoutedEventArgs e)
    {
        if (KeyboardLayout.Current is KeyboardLayout.En)
        {
            Keyboard.KeyDown(Keys.LShiftKey);
            Keyboard.KeyPress(Keys.Oem1);
            Keyboard.KeyUp(Keys.LShiftKey);
        }
        else
        {
            Keyboard.KeyDown(Keys.LShiftKey);
            Keyboard.KeyPress(Keys.D6);
            Keyboard.KeyUp(Keys.LShiftKey);
        }
    }

    private void SpaceButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Space);
    }

    private void ButtonDash_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.OemMinus);
    }


    private void ButtonBackspace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!isBackspacePressed) _ = BackspaceClick();
    }

    private void ButtonBackspace_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        isBackspacePressed = false;
        backspaceRepeatTimer.Stop();
    }
    private void ButtonBackspace_MouseLeave(object sender, MouseEventArgs e)
    {
        isBackspacePressed = false;
        backspaceRepeatTimer.Stop();
    }

    private void ButtonDel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!isDelPressed) _ = DelClick();
    }
    private void ButtonDel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        isDelPressed = false;
        delRepeatTimer.Stop();
    }
    private void ButtonDel_MouseLeave(object sender, MouseEventArgs e)
    {
        isDelPressed = false;
        delRepeatTimer.Stop();
    }
}
