using System.Xml.Serialization;

namespace Framework.Common;

public class PluginInfo
{
    public PluginInfo()
    {
        Assembly = string.Empty;
        Name = string.Empty;
        IsAvailable = false;
    }

    public string Assembly { get; set; }

    public string Name { get; set; }

    [XmlIgnore]
    public bool IsAvailable { get; set; }

    [XmlIgnore]
    public Type? ClassType { get; set; }

    [XmlIgnore]
    public Type? InterfaceType { get; set; }

    [XmlIgnore]
    public IPlugin? Instance { get; set; }
}