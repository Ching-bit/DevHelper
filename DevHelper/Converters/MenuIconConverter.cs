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

        try
        {
            return menuConfModel.MenuType switch
            {
                MenuType.Columns => ResourceHelper.FindResource<object>("SemiIconAscend"),
                MenuType.Tables => ResourceHelper.FindResource<object>("SemiIconCalendar"),
                MenuType.Database => ResourceHelper.FindResource<object>("SemiIconArchive"),
                MenuType.TableGroup => ResourceHelper.FindResource<object>("SemiIconFolderStroked"),
                MenuType.Table => ResourceHelper.FindResource<object>("SemiIconFile"),
                _ => null
            };
        }
        catch (ArgumentNullException)
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}