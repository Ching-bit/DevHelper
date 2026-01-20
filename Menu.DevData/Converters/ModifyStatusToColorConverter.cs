using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Framework.Common;

namespace Menu.DevData;

public class ModifyStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ModifyStatus modifyStatus ||
            ResourceHelper.FindResource("SemiColorText0") is not SolidColorBrush normalColor ||
            ResourceHelper.FindResource("SemiColorPrimary") is not SolidColorBrush modifyColor ||
            ResourceHelper.FindResource("SemiColorSuccess") is not SolidColorBrush addColor ||
            ResourceHelper.FindResource("SemiColorDanger") is not SolidColorBrush deleteColor)
        {
            return new SolidColorBrush(Colors.Black);
        }

        return modifyStatus switch
        {
            ModifyStatus.Normal => normalColor,
            ModifyStatus.Added => addColor,
            ModifyStatus.Modified => modifyColor,
            ModifyStatus.Deleted => deleteColor,
            _ => normalColor
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}