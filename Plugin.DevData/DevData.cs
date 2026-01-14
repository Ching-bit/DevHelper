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
    public bool AddTableGroup(DirectoryNode<TableInfo> directory, string groupName, string groupDescription, out DirectoryNode<TableInfo>? createdDirectory)
    {
        createdDirectory = null;
        if (directory.SubDirectories.Any(x => x.Name.Equals(groupName)))
        {
            return false;
        }

        string folderName = $"{groupName}@{groupDescription}";
        string groupDir = Path.Combine(directory.ConfigDirectory, folderName);
        try
        {
            Directory.CreateDirectory(groupDir);
        }
        catch
        {
            return false;
        }

        createdDirectory = new DirectoryNode<TableInfo>
        {
            ConfigDirectory = groupDir
        };
        directory.SubDirectories.Add(createdDirectory);
        return true;
    }

    public bool AddTable(DirectoryNode<TableInfo> directory, string tableName, string tableDescription, out TableInfo? createdTable)
    {
        createdTable = null;
        if (directory.Instances.Any(x => x.Name.Equals(tableName)))
        {
            return false;
        }

        string fileName = $"{tableName}@{tableDescription}.xml";
        string filePath = Path.Combine(directory.ConfigDirectory, fileName);

        // save an empty file
        TableInfo tableInfo = new()
        {
            ConfigFilePath = filePath
        };
        if (!tableInfo.ToFile())
        {
            return false;
        }
        
        directory.Instances.Add(tableInfo);
        createdTable = tableInfo;
        return true;
    }
    #endregion
    
    #endregion
}