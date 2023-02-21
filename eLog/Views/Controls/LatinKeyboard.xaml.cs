using eLog.Infrastructure.Extensions.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Keyboard = eLog.Infrastructure.Extensions.Windows.Keyboard;

namespace eLog.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для LatinKeyboard.xaml
    /// </summary>
    public partial class LatinKeyboard : UserControl
    {
        public LatinKeyboard()
        {
            InitializeComponent();
        }

        private void QButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.Q);
        }

        private void WButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.W);
        }

        private void EButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.E);
        }

        private void RButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.R);
        }

        private void TButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.T);
        }

        private void YButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.Y);
        }

        private void UButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.U);
        }

        private void IButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.I);
        }

        private void OButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.O);
        }

        private void PButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.P);
        }

        private void LsbButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.A);
        }

        private void SButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.S);
        }

        private void DButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.D);
        }

        private void FButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.F);
        }

        private void GButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.G);
        }

        private void HButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.H);
        }

        private void JButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.J);
        }

        private void KButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.K);
        }

        private void LButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.L);
        }

        private void ColonButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void ApButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void ZButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.Z);
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.X);
        }

        private void CButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.C);
        }

        private void VButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.V);
        }

        private void BButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.B);
        }

        private void NButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.N);
        }

        private void MButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
            Keyboard.KeyPress(Keys.M);
        }

        private void LtButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void GtButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void QmButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }

        private void Add1Button_Click(object sender, RoutedEventArgs e)
        {
            KeyboardLayout.Load(CultureInfo.GetCultureInfo(KeyboardLayout.En)).Activate();
        }
    }
}
