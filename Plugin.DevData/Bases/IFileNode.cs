namespace Plugin.DevData;

public interface IFileNode
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IDirectoryNode? Parent { get; set; }
    
    public string FileName { get; }
    public string FilePath { get; }
    public string MenuName { get; }

    public bool FromFile();
    public bool ToFile();
}