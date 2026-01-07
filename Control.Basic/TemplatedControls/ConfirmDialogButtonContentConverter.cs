using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Control.Basic;

public class ConfirmDialogButtonContentConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 4 ||
            values[0] is not string text ||
            values[1] is not bool isAutoClick ||
            values[2] is not bool isDefault ||
            values[3] is not int autoClickSeconds)
        {
            return "";
        }
        
        return text +　(isAutoClick && isDefault ? $" ({autoClickSeconds})" : "");
    }
}