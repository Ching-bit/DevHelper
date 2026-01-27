using Framework.Common;

namespace Plugin.AppEnv;

public interface IUserSetting : IPlugin
{
    public void Save();
}