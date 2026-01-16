using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class DevData : IDevData
{
    private static string DevDataDir => Path.Combine(SystemConfig.AppConf.UserDataDir, "DevData");
    private static string ColumnsFilePath => Path.Combine(DevDataDir, "Columns.xml");
    private static string TableDataDir => Path.Combine(DevDataDir, "Tables");
    
    #region IPlugin
    public void OnStart() { }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn()
    {
        Columns = ObjectHelper.FromXmlFile<List<ColumnInfo>>(ColumnsFilePath);
        TableRoot.ConfigDirectory = TableDataDir;
        TableRoot.ReadFiles();
    }

    public void OnLoggedOut() { }
    #endregion


    #region IDevData
    public List<ColumnInfo> Columns { get; private set; } = [];
    public DirectoryNode TableRoot { get; } = new();

    #region Column Methods
    public bool AddColumn(ColumnInfo columnInfo)
    {
        if (Columns.Any(x => x.Id == columnInfo.Id || x.Name == columnInfo.Name))
        {
            return false;
        }
        
        Columns.Add(columnInfo);
        return true;
    }

    public bool RemoveColumn(int id)
    {
        ColumnInfo? columnInfo = Columns.FirstOrDefault(x => x.Id == id);
        if (null == columnInfo)
        {
            return false;
        }
        
        Columns.Remove(columnInfo);
        return true;
    }

    public bool ModifyColumn(ColumnInfo columnInfo)
    {
        ColumnInfo? target = Columns.FirstOrDefault(x => x.Id == columnInfo.Id);
        if (null == target)
        {
            return false;
        }
        
        ObjectHelper.Copy(target, columnInfo);
        return true;
    }
    
    public bool SaveColumns()
    {
        return ObjectHelper.ToXml(ColumnsFilePath, Columns);
    }
    #endregion
    

    #region Table Methods
    public bool AddGroup(DirectoryNode directory, string groupName, string groupDescription, out DirectoryNode? createdDirectory)
    {
        createdDirectory = null;
        if (directory.SubDirectories.Any(x => x.Name.Equals(groupName)))
        {
            return false;
        }

        string folderName = AssembleFileName(groupName, groupDescription);
        string groupDir = Path.Combine(directory.ConfigDirectory, folderName);
        try
        {
            Directory.CreateDirectory(groupDir);
        }
        catch
        {
            return false;
        }

        createdDirectory = new DirectoryNode
        {
            ConfigDirectory = groupDir
        };
        directory.SubDirectories.Add(createdDirectory);
        return true;
    }

    public bool AddItem<T>(DirectoryNode directory, string itemName, string itemDescription, out T? createdItem) where T : FileNode, new()
    {
        createdItem = null;
        if (directory.Instances.Any(x => x.Name.Equals(itemName)))
        {
            return false;
        }

        string fileName = $"{AssembleFileName(itemName, itemDescription)}.xml";
        string filePath = Path.Combine(directory.ConfigDirectory, fileName);

        // save an empty file
        T item = new()
        {
            ConfigFilePath = filePath
        };
        if (!item.ToFile())
        {
            return false;
        }
        
        directory.Instances.Add(item);
        createdItem = item;
        return true;
    }

    public bool RemoveGroup(DirectoryNode parent, DirectoryNode group)
    {
        if (!parent.SubDirectories.Contains(group))
        {
            return false;
        }

        try
        {
            Directory.Delete(group.ConfigDirectory, true);
        }
        catch
        {
            return false;
        }
        
        return parent.SubDirectories.Remove(group);
    }

    public bool RemoveItem(DirectoryNode parent, FileNode item)
    {
        if (!parent.Instances.Contains(item))
        {
            return false;
        }

        try
        {
            File.Delete(item.ConfigFilePath);
        }
        catch
        {
            return false;
        }
        
        return parent.Instances.Remove(item);
    }

    public bool ModifyGroup(DirectoryNode directory, string newName, string newDescription)
    {
        string newFolderName = AssembleFileName(newName, newDescription);
        string newDir = Path.Combine(Path.GetDirectoryName(directory.ConfigDirectory) ?? string.Empty, newFolderName);
        try
        {
            Directory.Move(directory.ConfigDirectory, newDir);
        }
        catch
        {
            return false;
        }
        
        directory.ConfigDirectory = newDir;
        // TODO
        // update all paths of sub directories and files
        
        return true;
    }

    public bool ModifyItem(FileNode item, string newName, string newDescription)
    {
        string newFileName = $"{AssembleFileName(newName, newDescription)}.xml";
        string newPath = Path.Combine(Path.GetDirectoryName(item.ConfigFilePath) ?? string.Empty, newFileName);
        try
        {
            File.Move(item.ConfigFilePath, newPath);
        }
        catch
        {
            return false;
        }
        
        item.ConfigFilePath = newPath;
        return true;
    }
    #endregion
    
    #endregion


    private string AssembleFileName(string name, string description)
    {
        return name + (string.IsNullOrEmpty(description) ? "" : $"@{description}");
    }
}