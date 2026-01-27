using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UniClient;

public class NavContextMenuDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MenuType menuType)
        {
            return null;
        }
        
        if (MenuType.Columns == menuType)
        {
            return null;
        }

        return parameter;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}