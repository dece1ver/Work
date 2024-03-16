using System.Windows;
using System.Windows.Controls;

namespace eLog.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для KeyboardControl.xaml
    /// </summary>
    public partial class KeyboardControl : UserControl
    {
        private bool _CyrillicVisibility;

        public KeyboardControl()
        {
            InitializeComponent();
            _CyrillicVisibility = true;
            SetVisibility();
            Visibility = Visibility.Collapsed;
        }

        private void LangButton_Click(object sender, RoutedEventArgs e)
        {
            _CyrillicVisibility = !_CyrillicVisibility;
            SetVisibility();
        }

        private void SetVisibility()
        {
            LangButton.Content = _CyrillicVisibility ? "RU" : "EN";
            CyrillicKeyboard.Visibility = _CyrillicVisibility ? Visibility.Visible : Visibility.Collapsed;
            LatinKeyboard.Visibility = _CyrillicVisibility ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}