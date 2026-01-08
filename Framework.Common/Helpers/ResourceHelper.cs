using Avalonia;
using Avalonia.Controls;

namespace Framework.Common;

public static class ResourceHelper
{
    public static object? FindResource(string resourceName)
    {
        if (null == Application.Current ||
            !Application.Current.TryFindResource(resourceName, out object? value))
        {
            return null;
        }

        return value;
    }
    
    public static string FindStringResource(string resourceName, string? defaultValue = null)
    {
        if (FindResource(resourceName) is not string ret)
        {
            return defaultValue ?? resourceName;
        }

        return ret;
    }
}