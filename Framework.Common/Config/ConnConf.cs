using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Utils.Helpers;

namespace Framework.Common;

public enum ConnConfType
{
    Trade,
}

public partial class ConnConf : ObservableObject
{
    public ConnConf()
    {
        Name = string.Empty;
        Address = string.Empty;
    }

    [ObservableProperty] private ConnConfType _type;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _address;

    public string Host => Address.Split(":")[0];

    public int Port
    {
        get
        {
            string[] ipPort = Address.Split(":");
            if (ipPort.Length <= 1)
            {
                return 0;
            }

            _ = int.TryParse(ipPort[1], out int port);
            return port;
        }
    }

    public static Dictionary<ConnConfType, List<ConnConf>> FromXml(string xmlPath)
    {
        List<ConnConf> connConfList = ObjectHelper.FromXmlFile<List<ConnConf>>(xmlPath);
        
        return connConfList
            .GroupBy(item => item.Type)
            .ToDictionary(group => group.Key, group => group.ToList());
    }
}
