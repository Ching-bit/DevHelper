using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.AppEnv;

namespace UniClient.Models;

public partial class SettingModel : UniModel
{
    [ObservableProperty] private string _language = string.Empty;
    [ObservableProperty] private string _theme = string.Empty;
    [ObservableProperty] private bool _canAutoLogin;
    [ObservableProperty] private bool _isAutoLogin;

    public void FromGlobalSetting()
    {
        IGlobalSetting globalSetting = Global.Get<IGlobalSetting>();

        Language = globalSetting.Language;
        Theme = globalSetting.Theme;
        
        CanAutoLogin = null != Global.Get<IAppEnv>().User ?
            SystemConfig.AppConf.CanAutoLogin : // logged in
            false;                              // login window
        IsAutoLogin = globalSetting.IsAutoLogin;
    }

    public void SyncToGlobalSetting()
    {
        IGlobalSetting globalSetting = Global.Get<IGlobalSetting>();

        globalSetting.Language = Language;
        globalSetting.Theme = Theme;
        globalSetting.IsAutoLogin = IsAutoLogin;
        
        globalSetting.Save();
    }
}