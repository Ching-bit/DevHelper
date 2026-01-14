using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace UniClient;

public class NavContextMenuDisplayConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        bool display = false;
        foreach (object? value in values)
        {
            if (value is bool and true)
            {
                display = true;
                break;
            }
        }
        
        return display ? parameter : null;
    }
}