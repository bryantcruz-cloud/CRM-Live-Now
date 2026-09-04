using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace LiveNow.CRM.Helpers;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility visibility && visibility != Visibility.Visible;
    }
}
