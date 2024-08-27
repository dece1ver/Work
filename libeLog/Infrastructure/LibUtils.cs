using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace libeLog.Infrastructure
{
    public class LibUtils
    {
        public static bool IsValid(DependencyObject obj)
        {
            return !Validation.GetHasError(obj) &&
            LogicalTreeHelper.GetChildren(obj)
            .OfType<DependencyObject>()
            .All(IsValid);
        }
    }
}
