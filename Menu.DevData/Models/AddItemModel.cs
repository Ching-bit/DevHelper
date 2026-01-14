using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;

namespace Menu.DevData;

public partial class AddItemModel : UniModel
{
    public AddItemModel()
    {
        Name = string.Empty;
        Description = string.Empty;
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
}