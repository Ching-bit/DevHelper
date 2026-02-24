using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Framework.Common;

namespace Menu.DevData;

public class ModifyStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModifyStatus modifyStatus)
        {
            return new SolidColorBrush(Colors.Black);
        }

        try
        {
            SolidColorBrush normalColor = ResourceHelper.FindResource<SolidColorBrush>("SemiColorText0");
            SolidColorBrush modifyColor = ResourceHelper.FindResource<SolidColorBrush>("SemiColorPrimary");
            SolidColorBrush addColor = ResourceHelper.FindResource<SolidColorBrush>("SemiColorSuccess");
            SolidColorBrush deleteColor = ResourceHelper.FindResource<SolidColorBrush>("SemiColorDanger");
            
            return modifyStatus switch
            {
                ModifyStatus.Normal => normalColor,
                ModifyStatus.Added => addColor,
                ModifyStatus.Modified => modifyColor,
                ModifyStatus.Deleted => deleteColor,
                _ => normalColor
            };
        }
        catch (ArgumentNullException)
        {
            return new SolidColorBrush(Colors.Black);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}