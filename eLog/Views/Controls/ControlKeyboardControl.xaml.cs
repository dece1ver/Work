using libeLog.WinApi.Windows;
using System.Windows;
using System.Windows.Controls;

namespace eLog.Views.Controls;

/// <summary>
/// Логика взаимодействия для ControlKeyboardControl.xaml
/// </summary>
public partial class ControlKeyboardControl : UserControl
{
    public ControlKeyboardControl()
    {
        InitializeComponent();
    }

    private void BackspaceButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Back);
    }

    private void DelButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Delete);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.A);
        Keyboard.KeyUp(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Delete);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.A);
        Keyboard.KeyPress(Keys.C);
        Keyboard.KeyUp(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Right);
    }

    private void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Enter);
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Z);
        Keyboard.KeyUp(Keys.LControlKey);
    }

    private void ArrowUpButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Up);
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Y);
        Keyboard.KeyUp(Keys.LControlKey);
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyDown(Keys.LControlKey);
        Keyboard.KeyPress(Keys.A);
        Keyboard.KeyPress(Keys.V);
        Keyboard.KeyUp(Keys.LControlKey);
        Keyboard.KeyPress(Keys.Right);
    }

    private void ArrowLeftButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Left);
    }

    private void ArrowDownButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Down);
    }

    private void ArrowRightButton_Click(object sender, RoutedEventArgs e)
    {
        Keyboard.KeyPress(Keys.Right);
    }


}
