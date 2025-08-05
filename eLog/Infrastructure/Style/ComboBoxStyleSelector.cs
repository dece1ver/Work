using eLog.Views.Windows.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace eLog.Infrastructure.Style
{
    internal class ComboBoxStyleSelector : StyleSelector
    {
        public System.Windows.Style? SlashStyle { get; set; }
        public System.Windows.Style? DashStyle { get; set; }

        public override System.Windows.Style? SelectStyle(object item, DependencyObject container)
        {
            if (container is FrameworkElement fe && fe.DataContext is EditDetailWindow editDetailWindow)
            {
                return editDetailWindow.NewOrderFormat ? DashStyle : SlashStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}
