using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace UniClient;

public class TargetDatabasesToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
            case IList { Count: 0 }:
                return "*";
            case IList targetDatabases:
                StringBuilder sb = new();
                foreach (object targetDatabase in targetDatabases)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(targetDatabase + "");
                }
                return sb.ToString();
            default:
                return string.Empty;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}