using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Framework.Common;

public class UniViewModel : ObservableObject
{
    public Control? View { get; set; }

    public virtual void OnLoaded(object? sender, RoutedEventArgs e) { }
}