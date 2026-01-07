using Avalonia;
using Avalonia.Controls;

namespace Framework.Common;

public static class ResourceHelper
{
    public static string FindStringResource(string resourceName, string? defaultValue = null)
    {
        if (null == Application.Current ||
            !Application.Current.TryFindResource(resourceName, out object? value) ||
            value is not string ret)
        {
            return defaultValue ?? resourceName;
        }

        return ret;
    }
}