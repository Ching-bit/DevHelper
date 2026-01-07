using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class NetDelayToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (double.TryParse(value + "", out double d))
        {
            if (d < 0)
            {
                return "∞";
            }
            else
            {
                return value;
            }
        }

        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}