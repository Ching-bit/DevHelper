using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Framework.Common;

namespace UniClient;

public class MenuIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MenuType menuType)
        {
            return null;
        }

        return menuType switch
        {
            MenuType.TopItem => ResourceHelper.FindResource("SemiIconGridSquare"),
            MenuType.TopGroup => ResourceHelper.FindResource("SemiIconServer"),
            MenuType.Group => ResourceHelper.FindResource("SemiIconFolderStroked"),
            MenuType.Item => ResourceHelper.FindResource("SemiIconFile"),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}