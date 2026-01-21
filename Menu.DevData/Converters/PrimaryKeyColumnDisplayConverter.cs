using System.Globalization;
using Avalonia.Data.Converters;

namespace Menu.DevData;

public class PrimaryKeyColumnDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "✓" : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is "✓";
    }
}