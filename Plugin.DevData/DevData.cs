using System.Reflection;
using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class DevData : IDevData
{
    private static string DevDataDir => Path.Combine(SystemConfig.AppConf.UserDataDir, "DevData");
    private static string ColumnsFilePath => Path.Combine(DevDataDir, "Columns.xml");
    private const string TablesDirName = "Tables";
    private static string TablesDir => Path.Combine(DevDataDir, TablesDirName);
    
    #region IPlugin
    public void OnStart() { }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn()
    {
        Columns = ObjectHelper.FromXmlFile<List<ColumnInfo>>(ColumnsFilePath);
        TableRoot = new DirectoryNode(TablesDir);
        TableRoot.ReadFiles();
    }

    public void OnLoggedOut() { }
    #endregion


    #region IDevData
    public List<ColumnInfo> Columns { get; private set; } = [];
    public IDirectoryNode? TableRoot { get; private set; }

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
    public bool AddGroup(IDirectoryNode directory, string groupName, string groupDescription, out IDirectoryNode? createdDirectory)
    {
        createdDirectory = null;
        if (directory.SubDirectories.Any(x => x.Name.Equals(groupName)))
        {
            return false;
        }
        
        DirectoryNode newDirectoryNode = new(groupName, groupDescription, directory);
        try
        {
            Directory.CreateDirectory(newDirectoryNode.DirectoryPath);
        }
        catch
        {
            return false;
        }

        createdDirectory = newDirectoryNode;
        directory.SubDirectories.Add(createdDirectory);
        return true;
    }

    public bool AddItem(IDirectoryNode directory, string itemName, string itemDescription, out IFileNode? createdItem,
        Type itemType)
    {
        createdItem = null;
        MethodInfo? method = typeof(DevData).GetMethod(nameof(AddItemInner));
        if (null == method)
        {
            return false;
        }

        MethodInfo genericMethod = method.MakeGenericMethod(itemType);
        object?[] parameters = [directory, itemName, itemDescription, null];
        object? ret = genericMethod.Invoke(this, parameters);
        createdItem = (IFileNode?)parameters[3];
        return ret is true;
    }
    
    public bool AddItemInner<T>(IDirectoryNode directory, string itemName, string itemDescription, out T? createdItem)  where T : IFileNode, new()
    {
        createdItem = default;
        if (directory.Instances.Any(x => x.Name.Equals(itemName)))
        {
            return false;
        }

        T newFileNode = new()
        {
            Name = itemName,
            Description = itemDescription,
            Parent = directory
        };
        
        // save an empty file
        if (!newFileNode.ToFile())
        {
            return false;
        }

        createdItem = newFileNode;
        directory.Instances.Add(createdItem);
        return true;
    }

    public bool RemoveGroup(IDirectoryNode parent, IDirectoryNode group)
    {
        if (!parent.SubDirectories.Contains(group))
        {
            return false;
        }

        try
        {
            Directory.Delete(group.DirectoryPath, true);
        }
        catch
        {
            return false;
        }
        
        return parent.SubDirectories.Remove(group);
    }

    public bool RemoveItem(IDirectoryNode parent, IFileNode item)
    {
        if (!parent.Instances.Contains(item))
        {
            return false;
        }

        try
        {
            File.Delete(item.FilePath);
        }
        catch
        {
            return false;
        }
        
        return parent.Instances.Remove(item);
    }

    public bool ModifyGroup(IDirectoryNode directory, string newName, string newDescription)
    {
        IDirectoryNode tmp = new DirectoryNode(newName, newDescription, directory.Parent!);
        try
        {
            Directory.Move(directory.DirectoryPath, tmp.DirectoryPath);
        }
        catch
        {
            return false;
        }
        
        directory.Name = newName;
        directory.Description = newDescription;
        return true;
    }

    public bool ModifyItem(IFileNode item, string newName, string newDescription)
    {
        IFileNode tmp = new FileNode(newName, newDescription, item.Parent);
        try
        {
            File.Move(item.FilePath, tmp.FilePath);
        }
        catch
        {
            return false;
        }
        
        item.Name = newName;
        item.Description = newDescription;
        return true;
    }
    #endregion
    
    #endregion
    
}