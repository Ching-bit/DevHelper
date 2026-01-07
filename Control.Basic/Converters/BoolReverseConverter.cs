using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class BoolReverseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b)
        {
            return null;
        }

        return !b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b)
        {
            return null;
        }

        return !b;
    }
}