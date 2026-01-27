using System.Runtime.InteropServices;
using Framework.Common;

namespace Plugin.AppEnv;

public interface IAppEnv : IPlugin
{
    public string AppDir { get; }
    public string AppFullPath { get; }
    public string AppName { get; }
    public string AppNameWithExtension { get; }
    public OSPlatform? OsPlatform { get; }
    public Architecture CpuArchitecture { get; }
    public string OsType { get; }
    public UserInfo? User { get; set; }
    public string UserDataDir { get; }
}