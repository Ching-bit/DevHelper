using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ApiViewModel : UniViewModel
{
    #region Menu Lifecycle
    public override void OnMenuInit()
    {
        base.OnMenuInit();
        InitData();
    }

    private void InitData()
    {
        if (View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not ApiInfo apiInfo)
        {
            throw new Exception("Cannot get API instance from the menu");
        }

        ApiInfoModel = new ApiInfoModel(apiInfo);
        IsApiChanged = false;
    }
    #endregion


    #region Commands
    [RelayCommand]
    private void Save()
    {
        if (null == ApiInfoModel || View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not ApiInfo apiInfo)
        {
            throw new Exception("Cannot get API instance from the menu");
        }
        
        foreach (ApiParamSetModel inputParamSet in ApiInfoModel.InputParamSetList)
        {
            foreach (ColumnInfoModel deletedColumn in inputParamSet.ColumnList
                         .Where(x => x.ModifyStatus == ModifyStatus.Deleted).ToList())
            {
                inputParamSet.ColumnList.Remove(deletedColumn);
            }
        }
        
        foreach (ApiParamSetModel outputParamSet in ApiInfoModel.OutputParamSetList)
        {
            foreach (ColumnInfoModel deletedColumn in outputParamSet.ColumnList
                         .Where(x => x.ModifyStatus == ModifyStatus.Deleted).ToList())
            {
                outputParamSet.ColumnList.Remove(deletedColumn);
            }
        }

        if (!Global.Get<IDevData>().UpdateApi(apiInfo,
                ApiInfoModel.InputParamSetList.Select(x => x.GetApiParamSet()).ToList(),
                ApiInfoModel.OutputParamSetList.Select(x => x.GetApiParamSet()).ToList(),
                ApiInfoModel.Remark))
        {
            ShowNotification("R_STR_SAVE_FAILED", NotificationType.Error);
            return;
        }
        
        InitData();
    }

    [RelayCommand]
    private void Restore()
    {
        InitData();
    }
    #endregion
    
    
    #region Properties
    [ObservableProperty] private ApiInfoModel? _apiInfoModel;
    [ObservableProperty] private bool _isApiChanged;
    #endregion
    
}