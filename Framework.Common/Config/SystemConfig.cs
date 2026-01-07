using System.Collections.ObjectModel;
using Framework.Utils.Helpers;

namespace Framework.Common;

public static class SystemConfig
{
    private static readonly string SysConfDir = Path.Combine(Environment.CurrentDirectory, "sys");
    public static AppConf AppConf => ObjectHelper.FromXmlDir<AppConf>(SysConfDir);

    public static Dictionary<ConnConfType, List<ConnConf>> ConnConfDict => ConnConf.FromXml(AppConf.ConnConfPath);
    public static ObservableCollection<MenuConf> MenuConfList => MenuConf.FromXml(AppConf.MenuConfPath);
}