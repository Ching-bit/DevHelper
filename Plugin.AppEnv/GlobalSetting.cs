using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.AppEnv;

public class GlobalSetting : IGlobalSetting
{
    #region IPlugin
    public void OnStart()
    {
        GlobalSetting globalSetting = ObjectHelper.FromXmlDir<GlobalSetting>(SystemConfig.AppConf.UserDataDir);
        ObjectHelper.Copy(this, globalSetting);
    }

    public void OnStop() { }

    public void OnLogin() { }
    
    public void OnLoggedIn() { }

    public void OnLoggedOut() { }
    #endregion
    

    #region IGlobalSetting

    private string _language = string.Empty;
    public string Language
    {
        get => _language;
        set
        {
            _language = value;
            LanguageHelper.SetLanguage(_language);
        }
    }

    private string _theme = string.Empty;
    public string Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            ThemeHelper.SetTheme(_theme);
        }
    }

    public string SelectedConn { get; set; } = string.Empty;

    public List<UserInfo> LastUsers { get; set; } = [];

    public bool IsRememberAccount { get; set; } = false;

    public bool IsAutoLogin { get; set; } = false;

    public void Save()
    {
        ObjectHelper.ToXmlDir(SystemConfig.AppConf.UserDataDir, this);
    }

    public ConnConf GetSelectedTradeConnection()
    {
        if (!SystemConfig.ConnConfDict.TryGetValue(ConnConfType.Trade, out List<ConnConf>? connConfs))
        {
            throw new ArgumentException("Empty trade connection list in the configuration file");
        }
        
        ConnConf? connConf = connConfs.FirstOrDefault(x => x.Name == Global.Get<IGlobalSetting>().SelectedConn);
        if (null == connConf)
        {
            throw new ArgumentException("Selected connection name not in the configuration file");
        }
        
        return connConf;
    }
    #endregion
    

    public override string ToString()
    {
        return ObjectHelper.ToJson(this);
    }
}