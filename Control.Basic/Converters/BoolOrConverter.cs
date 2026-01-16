using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class BoolOrConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (object? item in values)
        {
            if (item is not bool b)
            {
                continue;
            }

            if (b)
            {
                return true;
            }
        }

        return false;
    }
}