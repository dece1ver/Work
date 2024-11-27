using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace remeLog.Infrastructure.Converters
{
    public class UserRoleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is User currentUserRole && parameter is string allowedRolesString)
            {
                var allowedRoles = allowedRolesString.Split(',')
                    .Select(r => (User)Enum.Parse(typeof(User), r.Trim()))
                    .ToList();

                return allowedRoles.Contains(currentUserRole)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
