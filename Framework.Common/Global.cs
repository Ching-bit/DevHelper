using Framework.Utils.Helpers;

namespace Framework.Common;

public static class Global
{
    #region  Public Functions
    public static void InitPlugins()
    {
        List<PluginInfo> pluginInfos = ObjectHelper.FromXmlFile<List<PluginInfo>>(SystemConfig.AppConf.PluginConfPath);

        foreach (PluginInfo info in pluginInfos)
        {
            PluginManager.AddPlugin(info.Assembly, info.Name);
        }
    }
    
    public static void StartPlugins()
    {
        PluginManager.StartPlugins();
    }

    public static void StopPlugins()
    {
        PluginManager.StopPlugins();
    }

    public static void LoginPlugins()
    {
        PluginManager.LoginPlugins();
    }

    public static void LoggedInPlugins()
    {
        PluginManager.LoggedInPlugins();
    }

    public static void LoggedOutPlugins()
    {
        PluginManager.LoggedOutPlugins();
    }

    public static T Get<T>() where T : IPlugin
    {
        IPlugin? plugin = PluginManager.GetPlugin(typeof(T));
        if (plugin is not T t)
        {
            throw new Exception($"Plugin not found: {typeof(T).Name}");
        }

        return t;
    }
    #endregion
    
    
    #region Member Variables
    private static readonly PluginManager PluginManager = new();
    #endregion
}