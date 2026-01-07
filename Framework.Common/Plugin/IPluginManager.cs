namespace Framework.Common;

public interface IPluginManager
{
    public IPlugin? GetPlugin(Type interfaceType);
    public bool AddPlugin(string assembly, string name);
    public bool StartPlugins();
    public bool StopPlugins();
    public bool LoginPlugins();
    public bool LoggedInPlugins();
    public bool LoggedOutPlugins();
}