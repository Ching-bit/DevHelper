using Framework.Common;

namespace Plugin.AppEnv;

public interface IGlobalSetting : IPlugin
{
    public string Language { get; set; }
    public string Theme { get; set; }
    public string SelectedConn { get; set; }
    public List<UserInfo> LastUsers { get; set; }
    public bool IsRememberAccount { get; set; }
    public bool IsAutoLogin { get; set; }

    public void Save();

    public ConnConf GetSelectedTradeConnection();
}