using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class SubStringConverter :　IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str ||
            parameter is not string param)
        {
            return string.Empty;
        }

        string[] p = param.Split(',');
        if (!int.TryParse(p[0], out int startIndex))
        {
            return string.Empty;
        }

        if (p.Length <= 1)
        {
            return str.Substring(startIndex);
        }
        
        if (!int.TryParse(p[1], out int len))
        {
            return string.Empty;
        }
        
        return str.Substring(startIndex, len);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}