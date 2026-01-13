using Avalonia.Controls;
using Ursa.Controls;

namespace Control.Basic;

public partial class ConfirmDialog
{
    public static async Task<ConfirmDialogResult> Show<TView, TViewModel>()
        where TView : UserControl, new() 
        where TViewModel : ConfirmDialogViewModel, new()
    {
        TViewModel viewModel = new();
        return await Show<TView>(viewModel);
    }

    public static async Task<ConfirmDialogResult> Show<TView>(ConfirmDialogViewModel viewModel)
        where TView : UserControl, new()
    {
        TView view = new();
        return await Show(view, viewModel);
    }
    
    public static async Task<ConfirmDialogResult> Show(UserControl view, ConfirmDialogViewModel viewModel)
    {
        ConfirmDialogResult? result = await Dialog.ShowCustomModal<ConfirmDialogResult>(view, viewModel);
        return result!;
    }
}