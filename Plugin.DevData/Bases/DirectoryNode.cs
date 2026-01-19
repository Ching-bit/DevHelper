namespace Plugin.DevData;

public class DirectoryNode : IDirectoryNode
{
    public DirectoryNode(string name, string description, IDirectoryNode parent)
    {
        Name = name;
        Description = description;
        Parent = parent;
        LeafType = parent.LeafType;
    }

    public DirectoryNode(string directoryPath, Type leafType)
    {
        _directoryPath = directoryPath;

        string dirName = Path.GetFileNameWithoutExtension(directoryPath);
        Name = dirName.Split("@")[0];
        Description = dirName.Split("@").Length <= 1 ? string.Empty : dirName.Split("@")[1];
        
        LeafType = leafType;
    }
    
    #region Functions
    public void ReadFiles()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return;
        }
        
        Instances.Clear();
        SubDirectories.Clear();
        ReadFilesInner(DirectoryPath, this);
    }

    private void ReadFilesInner(string dir, DirectoryNode node)
    {
        string[] files = Directory.GetFiles(dir, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string name = fileName.Split("@")[0];
            string description = fileName.Split("@").Length <= 1 ? string.Empty : fileName.Split("@")[1];

            IFileNode? fileNode = (IFileNode?)Activator.CreateInstance(LeafType);
            if (null == fileNode)
            {
                throw new Exception($"Cannot create instance of type {LeafType}");
            }
            
            fileNode.Name = name;
            fileNode.Description = description;
            fileNode.Parent = node;
            fileNode.FromFile();
            node.Instances.Add(fileNode);
        }
        
        string[] subDirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        foreach (string subDir in subDirs)
        {
            string subDirName = Path.GetFileNameWithoutExtension(subDir);
            string name = subDirName.Split("@")[0];
            string description = subDirName.Split("@").Length <= 1 ? string.Empty : subDirName.Split("@")[1];
            DirectoryNode subNode = new(name, description, node);
            
            subNode.ReadFiles();
            node.SubDirectories.Add(subNode);
        }
    }
    #endregion
    

    #region Properties
    public string Name { get; set; }
    public string Description { get; set; }
    public IDirectoryNode? Parent { get; set; }
    public List<IFileNode> Instances { get; } = [];
    public List<IDirectoryNode> SubDirectories { get; } = [];
    public Type LeafType { get; set; }


    public string DirectoryName => Name + (string.IsNullOrEmpty(Description) ? "" : $"@{Description}");

    private readonly string? _directoryPath;
    public string DirectoryPath => _directoryPath ?? Path.Combine(Parent?.DirectoryPath ?? string.Empty, DirectoryName);

    public string MenuName => Name + (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");
    #endregion
    
}