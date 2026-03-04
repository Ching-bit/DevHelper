using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;

namespace Menu.DevData;

public partial class ApiViewModel : UniViewModel
{
    [RelayCommand]
    private void Save()
    {
        
    }

    [RelayCommand]
    private void Restore()
    {
        
    }
    
    [ObservableProperty] private bool _isApiChanged;
}