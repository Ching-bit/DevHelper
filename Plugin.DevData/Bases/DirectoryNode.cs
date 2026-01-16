namespace Plugin.DevData;

public class DirectoryNode
{
    #region Functions
    public void ReadFiles()
    {
        if (!Directory.Exists(ConfigDirectory))
        {
            return;
        }
        
        Instances.Clear();
        SubDirectories.Clear();
        ReadFilesInner(ConfigDirectory, this);
    }

    private void ReadFilesInner(string dir, DirectoryNode node)
    {
        string[] fileNames = Directory.GetFiles(dir, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (string fileName in fileNames)
        {
            FileNode fileNode = new FileNode
            {
                ConfigFilePath = fileName
            };
            fileNode.FromFile();
            node.Instances.Add(fileNode);
        }
        
        string[] subDirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        foreach (string subDir in subDirs)
        {
            DirectoryNode subNode = new()
            {
                ConfigDirectory = subDir
            };
            subNode.ReadFiles();
            node.SubDirectories.Add(subNode);
        }
    }
    #endregion
    

    #region Properties
    public string ConfigDirectory { get; set; } = string.Empty;
    
    public DirectoryNode? Parent { get; set; } = null;
    public List<FileNode> Instances { get; } = [];
    public List<DirectoryNode> SubDirectories { get; } = [];

    public string Name
    {
        get
        {
            string fileName = Path.GetFileNameWithoutExtension(ConfigDirectory);
            return fileName.Split("@")[0];
        }
    }

    public string Description
    {
        get
        {
            string fileName = Path.GetFileNameWithoutExtension(ConfigDirectory);
            string[] arr = fileName.Split("@");
            return arr.Length >= 2 ? arr[1] : string.Empty;
        }
    }
    
    public string MenuName => Name + (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");
    #endregion
    
}