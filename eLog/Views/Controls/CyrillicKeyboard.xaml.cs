using libeLog.WinApi.Windows;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Keyboard = libeLog.WinApi.Windows.Keyboard;

namespace eLog.Views.Controls;

/// <summary>
/// Логика взаимодействия для CyrillicKeyboard.xaml
/// </summary>
public partial class CyrillicKeyboard : UserControl
{
    public CyrillicKeyboard()
    {
        InitializeComponent();
    }

    private void QButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Q);
    }

    private void WButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.W);
    }

    private void EButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.E);
    }

    private void RButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.R);
    }

    private void TButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.T);
    }

    private void YButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Y);
    }

    private void UButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.U);
    }

    private void IButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.I);
    }

    private void OButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.O);
    }

    private void PButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.P);
    }

    private void LsbButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Oem4);
    }

    private void AButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.A);
    }

    private void SButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.S);
    }

    private void DButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.D);
    }

    private void FButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.F);
    }

    private void GButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.G);
    }

    private void HButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.H);
    }

    private void JButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.J);
    }

    private void KButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.K);
    }

    private void LButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.L);
    }

    private void ColonButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Oem1);
    }

    private void ApButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Oem7);
    }

    private void ZButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Z);
    }

    private void XButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.X);
    }

    private void CButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.C);
    }

    private void VButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.V);
    }

    private void BButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.B);
    }

    private void NButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.N);
    }

    private void MButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.M);
    }

    private void LtButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Oemcomma);
    }

    private void GtButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.OemPeriod);
    }

    private void QmButton_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyPress(Keys.Oem6);
    }

    private void Add1Button_Click(object sender, RoutedEventArgs e)
    {
        KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.Ru)).Activate();
        Keyboard.KeyDown(Keys.LShiftKey);
        Keyboard.KeyPress(Keys.Oem2);
        Keyboard.KeyUp(Keys.LShiftKey);
    }
}
