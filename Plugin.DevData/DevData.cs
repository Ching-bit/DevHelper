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
    public DirectoryNode<TableInfo> TableRoot { get; } = new();

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
    public bool AddGroup<T>(DirectoryNode<T> directory, string groupName, string groupDescription, out DirectoryNode<T>? createdDirectory) where T : FileNode, new()
    {
        createdDirectory = null;
        if (directory.SubDirectories.Any(x => x.Name.Equals(groupName)))
        {
            return false;
        }

        string folderName = groupName + (string.IsNullOrEmpty(groupDescription) ? "" : $"@{groupDescription}");
        string groupDir = Path.Combine(directory.ConfigDirectory, folderName);
        try
        {
            Directory.CreateDirectory(groupDir);
        }
        catch
        {
            return false;
        }

        createdDirectory = new DirectoryNode<T>
        {
            ConfigDirectory = groupDir
        };
        directory.SubDirectories.Add(createdDirectory);
        return true;
    }

    public bool AddItem<T>(DirectoryNode<T> directory, string itemName, string itemDescription, out T? createdItem) where T : FileNode, new()
    {
        createdItem = null;
        if (directory.Instances.Any(x => x.Name.Equals(itemName)))
        {
            return false;
        }

        string fileName = itemName + (string.IsNullOrEmpty(itemDescription) ? "" : $"@{itemDescription}") + ".xml";
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

    public bool RemoveGroup<T>(DirectoryNode<T> parent, DirectoryNode<T> group) where T : FileNode, new()
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

    public bool RemoveItem<T>(DirectoryNode<T> parent, T item) where T : FileNode, new()
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
    #endregion
    
    #endregion
}