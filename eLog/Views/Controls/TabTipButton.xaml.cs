using System.Windows;
using System.Windows.Controls;

using Keyboard = eLog.Infrastructure.Extensions.Windows.Keyboard;

namespace eLog.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для TabTipButton.xaml
    /// </summary>
    public partial class TabTipButton : UserControl
    {
        private bool _TabTipStatus;

        public TabTipButton()
        {
            InitializeComponent();
        }

        private void TabTipBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_TabTipStatus)
            {
                Keyboard.KillTabTip(0);
                _TabTipStatus = false;
            }
            else
            {
                Keyboard.RunTabTip();
                _TabTipStatus = true;
            }
        }
    }
}
