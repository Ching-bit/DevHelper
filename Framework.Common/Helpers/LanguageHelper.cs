using System.Diagnostics;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using CommunityToolkit.Mvvm.Messaging;

namespace Framework.Common;

public static class LanguageHelper
{
    public static bool SetLanguage(string lang)
    {
        if (null == Application.Current) { return false; }

        ResourceInclude? currentLangResource = Application.Current.Resources.MergedDictionaries.OfType<ResourceInclude>().FirstOrDefault(x => x.Source?.OriginalString.Contains("/Lang/LangResources") ?? false);
        if (null != currentLangResource)
        {
            Application.Current.Resources.MergedDictionaries.Remove(currentLangResource);
        }

        string appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule?.FileName ?? "");
        string uri = $"avares://{appName}/Resources/Lang/LangResources.{lang}.axaml";
        ResourceInclude resource = new ResourceInclude(new Uri(uri))
        {
            Source = new Uri(uri)
        };
        Application.Current.Resources.MergedDictionaries.Add(resource);
        WeakReferenceMessenger.Default.Send(new LanguageChangedMessage(lang));
        return true;
    }
}