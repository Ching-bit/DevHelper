using Framework.Common;
using Ursa.Controls;

namespace Control.Basic;

public partial class MessageDialog
{
    public static async Task<bool> Show(string message, bool isAutoClick = false, bool isOkDefault = true, bool isCancelButtonVisible = false)
    {
        ConfirmDialogResult? result = await Dialog.ShowCustomModal<ConfirmDialogResult>(
            new MessageDialog
            {
                Message = ResourceHelper.FindStringResource(message),
                IsAutoClick = isAutoClick,
                IsOkDefault = isOkDefault,
                IsCancelButtonVisible = isCancelButtonVisible
            },
            new ConfirmDialogViewModel(),
            options: new DialogOptions
            {
                Mode = DialogMode.Info,
                CanDragMove = true,
                IsCloseButtonVisible = false,
                CanResize = false
            });
        return result?.IsConfirmed ?? false;
    }
}