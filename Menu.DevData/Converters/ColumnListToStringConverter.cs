using System.Collections;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace Menu.DevData;

public class ColumnListToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IList list)
        {
            return value + "";
        }

        StringBuilder sb = new();
        foreach (object? item in list)
        {
            if (item is ColumnInfoModel columnInfoModel)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(columnInfoModel.Name);
            }
        }
        
        return sb.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return "";
    }
}