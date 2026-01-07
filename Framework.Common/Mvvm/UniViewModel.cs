using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Framework.Common;

public class UniViewModel : ObservableObject
{
    public Control? View { get; set; }
}