using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ApiInfoModel : UniModel
{
    public ApiInfoModel(ApiInfo apiInfo)
    {
        Id = apiInfo.Id;
        Name = apiInfo.Name;
        Description = apiInfo.Description;
        
        foreach (ApiParamSet apiParamSet in apiInfo.InputParamSets)
        {
            InputParamSetList.Add(new ApiParamSetModel(apiParamSet));
        }
        
        foreach (ApiParamSet apiParamSet in apiInfo.OutputParamSets)
        {
            OutputParamSetList.Add(new ApiParamSetModel(apiParamSet));
        }
        
        Remark = apiInfo.Remark;
    }
    
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private ObservableCollection<ApiParamSetModel> _inputParamSetList = [];
    [ObservableProperty] private ObservableCollection<ApiParamSetModel> _outputParamSetList = [];
    [ObservableProperty] private string _remark = string.Empty;
}