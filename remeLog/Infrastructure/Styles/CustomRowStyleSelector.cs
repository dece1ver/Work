using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace remeLog.Infrastructure.Styles
{
    public class CustomRowStyleSelector : StyleSelector
{

    public override Style SelectStyle(object item, DependencyObject container)
    {
        if (item is DataRowBase data && data.Index % 2 == 0)
            return (Style)Application.Current.Resources["rowStyle1"];
        return (Style)Application.Current.Resources["rowStyle2"];
    }
}
}
