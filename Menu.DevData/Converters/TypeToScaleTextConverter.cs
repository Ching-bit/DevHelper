using System.Globalization;
using Avalonia.Data.Converters;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public class TypeToScaleTextConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ColumnType columnType)
        {
            return string.Empty;
        }

        if (columnType is ColumnType.Char or ColumnType.Varchar)
        {
            return ResourceHelper.FindStringResource("R_STR_MAX_BYTES_PER_CHAR");
        }
        else if (columnType is ColumnType.Number)
        {
            return ResourceHelper.FindStringResource("R_STR_SCALE");
        }
        
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}