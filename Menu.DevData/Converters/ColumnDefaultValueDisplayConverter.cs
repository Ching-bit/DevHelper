using System.Globalization;
using Avalonia.Data.Converters;
using Plugin.DevData;

namespace Menu.DevData;

public class ColumnDefaultValueDisplayConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3 || values[0] is not bool hasDefaultValue || values[1] is not string defaultValue || values[2] is not ColumnType columnType)
        {
            return "[ERROR]";
        }

        if (!hasDefaultValue)
        {
            return string.Empty;
        }

        if (columnType is ColumnType.Char or ColumnType.Varchar)
        {
            return $"'{defaultValue}'";
        }
        else
        {
            return defaultValue;
        }
    }
}