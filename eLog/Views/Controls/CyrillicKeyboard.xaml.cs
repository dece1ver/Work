using eLog.Infrastructure.Extensions.Windows;
using System;
using System.Collections.Generic;
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
            if (Keyboard.GetKeyboardLayout() == 1033)
            {
                Keyboard.KeyDown(Keys.Alt);
                Keyboard.KeyPress(Keys.LShiftKey);
                Keyboard.KeyUp(Keys.Alt);
                Keyboard.KeyPress(Keys.Q);
            }
            else
            {
                Keyboard.KeyPress(Keys.Q);
            }
        }

        private void WButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void YButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void IButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LsbButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void JButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void KButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ColonButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ZButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LtButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GtButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void QmButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add1Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
