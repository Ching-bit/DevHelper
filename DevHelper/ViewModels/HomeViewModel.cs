using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Menu.DevData;
using Plugin.AppEnv;

namespace UniClient;

public partial class HomeViewModel : UniViewModel
{
    [RelayCommand]
    private async Task SystemSetting()
    {
        ConfirmDialogResult result = await ConfirmDialog.Show<SystemSettingDialog, SystemSettingDialogViewModel>();
        if (!result.IsConfirmed || result.ReturnParameter is not SystemSettingModel systemSettingModel)
        {
            return;
        }
        
        systemSettingModel.ToUserSetting();
        Global.Get<IUserSetting>().Save();
    }
    
}