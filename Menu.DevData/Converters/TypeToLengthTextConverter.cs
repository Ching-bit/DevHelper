using System.Globalization;
using Avalonia.Data.Converters;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public class TypeToLengthTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ColumnType columnType)
        {
            return string.Empty;
        }

        return columnType switch
        {
            ColumnType.Char or ColumnType.Varchar => ResourceHelper.FindResource<string>("R_STR_LENGTH"),
            ColumnType.Number => ResourceHelper.FindResource<string>("R_STR_PRECISION"),
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}