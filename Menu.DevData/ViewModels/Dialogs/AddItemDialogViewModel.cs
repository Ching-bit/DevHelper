using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;

namespace Menu.DevData;

public partial class AddItemDialogViewModel : ConfirmDialogViewModel
{
    public AddItemDialogViewModel()
    {
        AddItemModel = new AddItemModel();
    }

    [ObservableProperty] private AddItemModel _addItemModel;
}