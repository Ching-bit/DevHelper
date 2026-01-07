using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class BoolAndConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (object? item in values)
        {
            if (item is not bool b)
            {
                continue;
            }

            if (!b)
            {
                return false;
            }
        }

        return true;
    }
}