using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using UniClient.Models;
using Ursa.Controls;

namespace UniClient;

public partial class TitleBarRightViewModel : UniViewModel
{
    [RelayCommand] private async Task Setting()
    {
        ConfirmDialogResult? result =
            await Dialog.ShowCustomModal<SettingDialog, SettingDialogViewModel, ConfirmDialogResult>(
                new SettingDialogViewModel(),
                options: new DialogOptions
                {
                    // Title = ResourceHelper.FindStringResource("R_STR_SETTING", string.Empty),
                    Mode = DialogMode.Info,
                    CanDragMove = true,
                    IsCloseButtonVisible = true,
                    CanResize = false
                });
        if (true != result?.IsConfirmed ||
            result.ReturnParameter is not SettingModel settingModel)
        {
            return;
        }
        settingModel.SyncToGlobalSetting();
    }
    
}