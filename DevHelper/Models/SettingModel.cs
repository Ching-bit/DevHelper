using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.AppEnv;

namespace UniClient.Models;

public partial class SettingModel : UniModel
{
    [ObservableProperty] private string _language = string.Empty;
    [ObservableProperty] private string _theme = string.Empty;

    public void FromGlobalSetting()
    {
        IGlobalSetting globalSetting = Global.Get<IGlobalSetting>();

        Language = globalSetting.Language;
        Theme = globalSetting.Theme;
    }

    public void SyncToGlobalSetting()
    {
        IGlobalSetting globalSetting = Global.Get<IGlobalSetting>();

        globalSetting.Language = Language;
        globalSetting.Theme = Theme;
        
        globalSetting.Save();
    }
}