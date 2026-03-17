using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;

namespace UniClient;

public partial class SettingDialogViewModel : ConfirmDialogViewModel
{
    public SettingDialogViewModel()
    {
        Setting.FromGlobalSetting();
    }
    
    [ObservableProperty] private SettingModel _setting = new();
}