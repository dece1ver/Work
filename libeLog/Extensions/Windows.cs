using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace libeLog.Extensions
{
    public static class Windows
    {
        /// <summary>
        /// Центрирует окно относительно другого окна, без установки Owner.
        /// Подходит для Show и ShowDialog.
        /// </summary>
        /// <param name="window">Окно, которое нужно отцентрировать</param>
        /// <param name="parent">Окно, относительно которого центрируем</param>
        public static void CenterTo(this Window window, Window parent)
        {
            if (window == null || parent == null) return;

            window.WindowStartupLocation = WindowStartupLocation.Manual;

            if (double.IsNaN(window.Width) || double.IsNaN(window.Height))
            {
                window.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                window.Width = window.DesiredSize.Width;
                window.Height = window.DesiredSize.Height;
            }

            window.Left = parent.Left + (parent.Width - window.Width) / 2;
            window.Top = parent.Top + (parent.Height - window.Height) / 2;
        }
    }
}
