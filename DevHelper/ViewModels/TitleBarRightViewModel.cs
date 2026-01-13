using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using UniClient.Models;

namespace UniClient;

public partial class TitleBarRightViewModel : UniViewModel
{
    [RelayCommand]
    private async Task Setting()
    {
        ConfirmDialogResult confirmResult = await ConfirmDialog.Show<SettingDialog, SettingDialogViewModel>();
        if (!confirmResult.IsConfirmed || confirmResult.ReturnParameter is not SettingModel settingModel)
        {
            return;
        }
        settingModel.SyncToGlobalSetting();
    }
    
}