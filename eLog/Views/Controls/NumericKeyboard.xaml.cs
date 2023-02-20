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
    /// Логика взаимодействия для NumericKeyboard.xaml
    /// </summary>
    public partial class NumericKeyboard : UserControl
    {
        public NumericKeyboard()
        {
            InitializeComponent();
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

        private void ButtonBackspace_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.KeyPress(Keys.Back);
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
            Keyboard.KeyPress(Keyboard.GetKeyboardLayout() == 1033 ? Keys.OemPeriod : Keys.Oem2);
        }

        private void ButtonColon_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.GetKeyboardLayout() == 1033)
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
    }
}
