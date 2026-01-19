using System.Xml.Serialization;

namespace Plugin.DevData;

public class FileNode : IFileNode
{
    public FileNode()
    {
        Name = string.Empty;
        Description = string.Empty;
        Parent = null;
    }

    public FileNode(string name, string description, IDirectoryNode? parent)
    {
        Name = name;
        Description = description;
        Parent = parent;
    }
    
    [XmlIgnore] public string Name { get; set; }
    [XmlIgnore] public string Description { get; set; }
    [XmlIgnore] public IDirectoryNode? Parent { get; set; }
    
    [XmlIgnore] public string FileName => Name + (string.IsNullOrEmpty(Description) ? "" : $"@{Description}") + ".xml";
    [XmlIgnore] public string FilePath => Path.Combine(Parent?.DirectoryPath ?? string.Empty, FileName);
    [XmlIgnore] public string MenuName => Name + (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");

    public virtual bool FromFile() => true;
    public virtual bool ToFile() => true;
}