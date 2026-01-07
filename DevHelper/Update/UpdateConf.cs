using Plugin.AppEnv;

namespace Framework.Common;

public class UpdateConf
{
    private string _updateListPath = string.Empty;
    public string UpdateListPath
    {
        get => _updateListPath.Replace("${Platform}", Global.Get<IAppEnv>().OsType);
        set => _updateListPath = value;
    }

    private string _changeLogPath = string.Empty;
    public string ChangeLogPath
    {
        get => _changeLogPath.Replace("${Platform}", Global.Get<IAppEnv>().OsType);
        set => _changeLogPath = value;
    }

    private string _remoteClientDir = string.Empty;
    public string RemoteClientDir
    {
        get => _remoteClientDir.Replace("${Platform}", Global.Get<IAppEnv>().OsType);
        set => _remoteClientDir = value;
    }
    
    public bool IsForce { get; set; } = true;
}