using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using Ursa.Controls;

namespace Framework.Common;

public class UniWindow : UrsaWindow
{
    protected UniWindow()
    {
        _ = ViewModelLocator.SetViewModel(this);
    }
    
    public static void NavigateTo(UniWindow targetWindow, UniWindow currentWindow)
    {
        IClassicDesktopStyleApplicationLifetime? desktop = (IClassicDesktopStyleApplicationLifetime?)Application.Current?.ApplicationLifetime;
        if (null != desktop)
        {
            desktop.MainWindow = targetWindow;
        }
        targetWindow.Show();
        currentWindow.Close();
    }

    public static void NavigateTo<T>(UniWindow currentWindow) where T : UniWindow, new()
    {
        T targetWindow = new();
        NavigateTo(targetWindow, currentWindow);
    }
    
    public static void NavigateTo<T>(UniViewModel currentViewModel) where T : UniWindow, new()
    {
        if (currentViewModel.View?.GetVisualRoot() is not UniWindow currentWindow)
        {
            return;
        }
        NavigateTo<T>(currentWindow);
    }
}