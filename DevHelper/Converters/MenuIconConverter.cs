using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Framework.Common;

namespace UniClient;

public class MenuIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MenuConfModel menuConfModel)
        {
            return null;
        }

        return menuConfModel.MenuType switch
        {
            MenuType.Columns => ResourceHelper.FindResource("SemiIconAscend"),
            MenuType.Tables => ResourceHelper.FindResource("SemiIconCalendar"),
            MenuType.Database => ResourceHelper.FindResource("SemiIconArchive"),
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