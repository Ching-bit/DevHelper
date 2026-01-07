using Framework.Utils.Helpers;

namespace Framework.Common;

public class PluginManager : IPluginManager
{
    #region Constructors
    public PluginManager()
    {
        _pluginList = [];
        _pluginDict = [];
    }
    #endregion
    
    
    #region Base Interfaces
    public IPlugin? GetPlugin(Type interfaceType)
    {
        if (!_pluginDict.TryGetValue(interfaceType, out PluginInfo? pluginInfo) ||
            !pluginInfo.IsAvailable)
        {
            return null;
        }

        return pluginInfo.Instance;
    }

    public bool AddPlugin(string assembly, string name)
    {
        PluginInfo pluginInfo = new()
        {
            Assembly = assembly,
            Name = name,
            ClassType = DllHelper.GetType(assembly + ".dll", assembly, name),
        };

        if (null == pluginInfo.ClassType) { return false; }

        Type[] interfaceTypes = pluginInfo.ClassType.GetInterfaces();
        if (2 != interfaceTypes.Length)
        {
            // Plugin is not standard
            return false;
        }

        pluginInfo.InterfaceType = interfaceTypes[0];
        pluginInfo.Instance = DllHelper.CreateInstance<IPlugin>(assembly + ".dll", assembly, name);
        if (null == pluginInfo.Instance) { return false; }

        _pluginList.Remove(pluginInfo);
        _pluginList.Add(pluginInfo);
        _pluginDict.Remove(pluginInfo.InterfaceType);
        _pluginDict.Add(pluginInfo.InterfaceType, pluginInfo);
        
        return true;
    }

    public bool StartPlugins()
    {
        bool isSuccessful = true;
        foreach (PluginInfo pluginInfo in _pluginList)
        {
            isSuccessful = isSuccessful && StartPlugin(pluginInfo);
        }

        return isSuccessful;
    }

    public bool StopPlugins()
    {
        bool isSuccessful = true;
        foreach (PluginInfo pluginInfo in _pluginList)
        {
            isSuccessful = isSuccessful && StopPlugin(pluginInfo);
        }

        return isSuccessful;
    }

    public bool LoginPlugins()
    {
        bool isSuccessful = true;
        foreach (PluginInfo pluginInfo in _pluginList)
        {
            isSuccessful = isSuccessful && LoggingPlugin(pluginInfo);
        }

        return isSuccessful;
    }

    public bool LoggedInPlugins()
    {
        bool isSuccessful = true;
        foreach (PluginInfo pluginInfo in _pluginList)
        {
            isSuccessful = isSuccessful && LoggedPlugin(pluginInfo);
        }

        return isSuccessful;
    }

    public bool LoggedOutPlugins()
    {
        bool isSuccessful = true;
        foreach (PluginInfo pluginInfo in _pluginList)
        {
            isSuccessful = isSuccessful && LogoutPlugin(pluginInfo);
        }

        return isSuccessful;
    }
    #endregion
    
    
    #region Private Functions
    private static bool StartPlugin(PluginInfo pluginInfo)
    {
        if (null == pluginInfo.Instance) { return false; }

        try
        {
            pluginInfo.Instance.OnStart();
            pluginInfo.IsAvailable = true;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private static bool StopPlugin(PluginInfo pluginInfo)
    {
        if (null == pluginInfo.Instance) { return true; }

        try
        {
            pluginInfo.Instance.OnStop();
            pluginInfo.IsAvailable = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool LoggingPlugin(PluginInfo pluginInfo)
    {
        if (null == pluginInfo.Instance) { return false; }

        try
        {
            pluginInfo.Instance.OnLogin();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool LoggedPlugin(PluginInfo pluginInfo)
    {
        if (null == pluginInfo.Instance) { return false; }

        try
        {
            pluginInfo.Instance.OnLoggedIn();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool LogoutPlugin(PluginInfo pluginInfo)
    {
        if (null == pluginInfo.Instance) { return true; }

        try
        {
            pluginInfo.Instance.OnLoggedOut();
            return true;
        }
        catch
        {
            return false;
        }
    }
    #endregion


    #region Member Variables
    private readonly List<PluginInfo> _pluginList;
    private readonly Dictionary<Type, PluginInfo> _pluginDict;
    #endregion
}