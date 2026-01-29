using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;

namespace Menu.DevData;

public partial class SystemSettingDialogViewModel : ConfirmDialogViewModel
{
    public SystemSettingDialogViewModel()
    {
        SystemSettingModel = new SystemSettingModel();
        SystemSettingModel.FromUserSetting();
    }
    
    [ObservableProperty] private SystemSettingModel _systemSettingModel;
}