using System.Diagnostics;
using System.Runtime.InteropServices;
using Framework.Common;

namespace Plugin.AppEnv;

internal class AppEnv : IAppEnv
{
    #region IAppEnv
    public string AppDir => Environment.CurrentDirectory;
    public string AppFullPath => Process.GetCurrentProcess().MainModule?.FileName ?? "";
    public string AppName => Path.GetFileNameWithoutExtension(AppFullPath);
    public string AppNameWithExtension => Path.GetFileName(AppFullPath);

    public OSPlatform? OsPlatform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return OSPlatform.FreeBSD;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            else
            {
                return null;
            }
        }
    }

    public Architecture CpuArchitecture => RuntimeInformation.ProcessArchitecture;
    
    public string OsType => OsPlatform?.ToString().ToLower() + "_" + CpuArchitecture.ToString().ToLower();

    public UserInfo? User { get; set; }
    
    public string UserDataDir { get; private set; } = string.Empty;
    #endregion
    

    #region IPlugin
    public void OnStart() { }
    public void OnStop() { }
    public void OnLogin() { }

    public void OnLoggedIn()
    {
        UserDataDir = Path.Combine(SystemConfig.AppConf.UserDataDir, User?.Username ?? string.Empty);
        if (!Directory.Exists(UserDataDir))
        {
            Directory.CreateDirectory(UserDataDir);
        }
    }

    public void OnLoggedOut()
    {
        UserDataDir = string.Empty;
    }
    #endregion
    
}