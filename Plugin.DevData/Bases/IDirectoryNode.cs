namespace Plugin.DevData;

public interface IDirectoryNode
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IDirectoryNode? Parent { get; set; }
    public List<IFileNode> Instances { get; }
    public List<IDirectoryNode> SubDirectories { get; }
    
    
    public string DirectoryName { get; }
    public string DirectoryPath { get; }
    public string MenuName { get; }

    public void ReadFiles();
}