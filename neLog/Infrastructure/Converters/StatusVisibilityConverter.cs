using libeLog.Infrastructure;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace neLog.Infrastructure.Converters
{
    internal class StatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Status)value)
            {
                case Status.Ok:
                    if (Application.Current.TryFindResource("StatusOkIcon") is Viewbox statusOk)
                    {
                        return statusOk;
                    }
                    break;
                case Status.Warning:
                    if (Application.Current.TryFindResource("StatusWarningIcon") is Viewbox statusWarning)
                    {
                        return statusWarning;
                    }
                    break;
                case Status.Error:
                    if (Application.Current.TryFindResource("StatusErrorIcon") is Viewbox statusErr)
                    {
                        return statusErr;
                    }
                    break;
                case Status.Sync:
                    if (Application.Current.TryFindResource("SyncIcon") is Viewbox sync)
                    {
                        return sync;
                    }
                    break;
            }
            return new object();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}