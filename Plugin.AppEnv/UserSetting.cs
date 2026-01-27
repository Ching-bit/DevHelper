using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.AppEnv;

public class UserSetting : IUserSetting
{
    public void OnStart() { }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn()
    {
        string confPath = Path.Combine(Global.Get<IAppEnv>().UserDataDir, $"{nameof(UserSetting)}.xml");
        if (!File.Exists(confPath))
        {
            Save();
            return;
        }
        UserSetting userSetting = ObjectHelper.FromXmlDir<UserSetting>(Global.Get<IAppEnv>().UserDataDir);
        ObjectHelper.Copy(this, userSetting);
    }

    public void OnLoggedOut() { }


    public void Save()
    {
        ObjectHelper.ToXmlDir(Global.Get<IAppEnv>().UserDataDir, this);
    }
}