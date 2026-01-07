using Avalonia;
using Avalonia.Styling;
using Ursa.Themes.Semi;

namespace Framework.Common;

public static class ThemeHelper
{
    private static readonly Dictionary<string, ThemeVariant> ThemeVariants = new()
    {
        {nameof(ThemeConst.Light), ThemeVariant.Light},
        {nameof(ThemeConst.Dark), ThemeVariant.Dark},
        {nameof(ThemeConst.Aquatic), SemiTheme.Aquatic},
        {nameof(ThemeConst.Desert), SemiTheme.Desert},
        {nameof(ThemeConst.Dusk), SemiTheme.Dusk},
        {nameof(ThemeConst.NightSky), SemiTheme.NightSky}
    };
    
    public static void SetTheme(string theme)
    {
        if (null == Application.Current ||
            !ThemeVariants.TryGetValue(theme, out ThemeVariant? themeVariant))
        {
            return;
        }
        
        Application.Current.RequestedThemeVariant = themeVariant;
    }
}