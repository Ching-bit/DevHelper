using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ApiInfoModel : UniModel
{
    public ApiInfoModel()
    {
        Name = string.Empty;
        Description = string.Empty;
    }
    
    public ApiInfoModel(ApiInfo apiInfo) : this()
    {
        
    }
    
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
}