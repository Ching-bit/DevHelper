using Avalonia;
using Avalonia.Controls;

namespace Framework.Common;

public static class ResourceHelper
{
    public static T FindResource<T>(string resourceName, T? defaultValue = default)
    {
        if (FindResourceInner(resourceName) is not T ret)
        {
            if (null == defaultValue)
            {
                throw new ArgumentNullException($"Resource '{resourceName}' not found");
            }
            return defaultValue;
        }
        
        return ret;
    }
    
    private static object? FindResourceInner(string resourceName)
    {
        if (null == Application.Current ||
            !Application.Current.TryFindResource(resourceName, out object? value))
        {
            return null;
        }

        return value;
    }
}