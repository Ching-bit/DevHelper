using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class NumberEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!double.TryParse(value + "", out double n1) ||
            !double.TryParse(parameter + "", out double n2))
        {
            return false;
        }
        
        return Math.Abs(n1 - n2) < 1e-10;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}