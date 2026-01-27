using System.Globalization;
using Avalonia.Data.Converters;
using Plugin.DevData;

namespace Menu.DevData;

public class ColumnLengthDisplayConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not ColumnType columnType || values[1] is not int length)
        {
            return string.Empty;
        }

        if (columnType is ColumnType.Number or ColumnType.Char or ColumnType.Varchar)
        {
            return length.ToString();
        }

        return "-";
    }
}