using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class IsValueInConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string parameterString)
        {
            return false;
        }
        
        string[] validValues = parameterString.Split('|');
        return validValues.Any(validValue => validValue.Equals(value + ""));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}