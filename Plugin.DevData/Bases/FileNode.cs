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
    
    public string Name { get; set; }
    public string Description { get; set; }
    public IDirectoryNode? Parent { get; set; }
    
    
    public string FileName => Name + (string.IsNullOrEmpty(Description) ? "" : $"@{Description}") + ".xml";
    public string FilePath => Path.Combine(Parent.DirectoryPath, FileName);
    public string MenuName => Name + (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");

    public virtual bool FromFile() => true;
    public virtual bool ToFile() => true;
}