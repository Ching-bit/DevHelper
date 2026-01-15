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
            MenuType.Columns => ResourceHelper.FindResource("SemiIconGridSquare"),
            MenuType.Tables => ResourceHelper.FindResource("SemiIconServer"),
            MenuType.TableGroup => ResourceHelper.FindResource("SemiIconFolderStroked"),
            MenuType.Table => ResourceHelper.FindResource("SemiIconFile"),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}