namespace Plugin.DevData;

public class DirectoryNode<T> where T : FileNode, new()
{
    #region Constructors
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

    private void ReadFilesInner(string dir, DirectoryNode<T> node)
    {
        string[] fileNames = Directory.GetFiles(dir, "*.xml", SearchOption.TopDirectoryOnly);
        foreach (string fileName in fileNames)
        {
            T item = new T
            {
                ConfigFilePath = Path.Combine(dir, fileName)
            };
            item.FromFile();
            node.Instances.Add(item);
        }
        
        string[] subDirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        foreach (string subDir in subDirs)
        {
            DirectoryNode<T> subNode = new()
            {
                ConfigDirectory = Path.Combine(dir, subDir)
            };
            subNode.ReadFiles();
            node.SubDirectories.Add(subNode);
        }
    }
    #endregion
    

    #region Properties
    public string ConfigDirectory { get; set; } = string.Empty;
    public List<T> Instances { get; set; } = [];
    public List<DirectoryNode<T>> SubDirectories { get; set; } = [];

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
    #endregion
    
}