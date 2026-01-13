using System.Globalization;
using Avalonia.Data.Converters;

namespace Menu.DevData;

public class ColumnDataDictTooltipConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str)
        {
            return string.Empty;
        }
        
        return str.Replace("; ", Environment.NewLine);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}