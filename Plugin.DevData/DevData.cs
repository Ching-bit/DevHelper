using System.Reflection;
using Framework.Common;
using Framework.Utils.Helpers;
using Plugin.AppEnv;

namespace Plugin.DevData;

public class DevData : IDevData
{
    private static string DevDataDir
    {
        get
        {
            string dir = SystemConfig.AppConf.UserDataDir;
            dir = Path.Combine(dir, Global.Get<IAppEnv>().User?.Username ?? string.Empty);
            dir = Path.Combine(dir, "DevData");
            return dir;
        }
    }
    
    private static string ColumnsFilePath => Path.Combine(DevDataDir, "Columns.xml");
    private const string TablesDirName = "Tables";
    private static string TablesDir => Path.Combine(DevDataDir, TablesDirName);
    
    #region IPlugin
    public void OnStart() { }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn()
    {
        if (!Directory.Exists(DevDataDir))
        {
            Directory.CreateDirectory(DevDataDir);
        }
        
        if (!File.Exists(ColumnsFilePath))
        {
            ObjectHelper.ToXml(ColumnsFilePath, new List<ColumnInfo>());
        }
        Columns = ObjectHelper.FromXmlFile<List<ColumnInfo>>(ColumnsFilePath);

        if (!Directory.Exists(TablesDir))
        {
            Directory.CreateDirectory(TablesDir);
        }
        TableRoot = new DirectoryNode(TablesDir, typeof(TableInfo));
        TableRoot.ReadFiles();
    }

    public void OnLoggedOut() { }
    #endregion


    #region IDevData
    public List<ColumnInfo> Columns { get; private set; } = [];
    public IDirectoryNode? TableRoot { get; private set; }
    

    #region Column Methods
    public bool UpdateColumns(List<ColumnInfo> columns)
    {
        if (!ObjectHelper.ToXml(ColumnsFilePath, columns))
        {
            return false;
        }

        Columns = columns;
        return true;
    }

    public TableInfo? FirstUsedTable(int columnId)
    {
        return null == TableRoot ? null : FirstUsedTableInner(columnId, TableRoot);
    }

    private TableInfo? FirstUsedTableInner(int columnId, IDirectoryNode directory)
    {
        foreach (IFileNode fileNode in directory.Instances)
        {
            if (fileNode is TableInfo tableInfo &&
                tableInfo.ColumnIdList.Contains(columnId))
            {
                return tableInfo;
            }
        }

        foreach (IDirectoryNode directoryNode in directory.SubDirectories)
        {
            TableInfo? tableInfo = FirstUsedTableInner(columnId, directoryNode);
            if (null != tableInfo)
            {
                return tableInfo;
            }
        }
        
        return null;
    }
    #endregion
    

    #region Menu Methods
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

    public bool AddItem(IDirectoryNode directory, string itemName, string itemDescription, out IFileNode? createdItem, Type itemType)
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

    public bool IsItemNameExists(IDirectoryNode directoryNode, string itemName, string[]? exceptedItemNames = null)
    {
        if (null != exceptedItemNames)
        {
            exceptedItemNames = exceptedItemNames.Select(name => name.ToLower()).ToArray();
        }
        
        foreach (IFileNode fileNode in directoryNode.Instances)
        {
            if (fileNode.Name.ToLower().Equals(itemName.ToLower()) &&
                (null == exceptedItemNames || !exceptedItemNames.Contains(fileNode.Name.ToLower())))
            {
                return true;
            }
        }

        foreach (IDirectoryNode subDirectory in directoryNode.SubDirectories)
        {
            if (IsItemNameExists(subDirectory, itemName, exceptedItemNames))
            {
                return true;
            }
        }
        
        return false;
    }

    public IDirectoryNode? GetRootDirectory(IFileNode fileNode)
    {
        IDirectoryNode? ret = fileNode.Parent;
        while (null != ret && null != ret.Parent)
        {
            ret = ret.Parent;
        }
        return ret;
    }

    public IDirectoryNode? GetRootDirectory(IDirectoryNode directoryNode)
    {
        IDirectoryNode? ret = directoryNode;
        while (null != ret && null != ret.Parent)
        {
            ret = ret.Parent;
        }
        return ret;
    }
    #endregion


    #region Table Methods
    public List<TableInfo> GetTableList()
    {
        return null == TableRoot ? [] : GetTableListInner(TableRoot);
    }

    private List<TableInfo> GetTableListInner(IDirectoryNode tableDirectory)
    {
        List<TableInfo> tableList = [];
        foreach (IFileNode fileNode in tableDirectory.Instances)
        {
            if (fileNode is TableInfo tableInfo)
            {
                tableList.Add(tableInfo);
            }
        }

        foreach (IDirectoryNode directoryNode in tableDirectory.SubDirectories)
        {
            tableList.AddRange(GetTableListInner(directoryNode));
        }
        
        return tableList;
    }
    
    public bool UpdateTable(TableInfo tableInfo, List<int> columnIdList, List<IndexInfo> indexList, List<ForeignKeyInfo> foreignKeyList, string remark)
    {
        TableInfo tmp = new(tableInfo.Name, tableInfo.Description, tableInfo.Parent)
        {
            ColumnIdList = columnIdList,
            IndexList = indexList,
            ForeignKeyList = foreignKeyList,
            Remark = remark
        };
        if (!tmp.ToFile())
        {
            return false;
        }
        
        tableInfo.ColumnIdList.Clear();
        tableInfo.ColumnIdList.AddRange(columnIdList);
        tableInfo.IndexList.Clear();
        tableInfo.IndexList.AddRange(indexList);
        tableInfo.ForeignKeyList.Clear();
        tableInfo.ForeignKeyList.AddRange(foreignKeyList);
        tableInfo.Remark = remark;
        return true;
    }
    #endregion
    
    #endregion
    
}