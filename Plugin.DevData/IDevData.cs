using Framework.Common;

namespace Plugin.DevData;

public interface IDevData : IPlugin
{
    public List<ColumnInfo> Columns { get; }
    public IDirectoryNode? TableRoot { get; }

    public bool UpdateColumns(List<ColumnInfo> columns);
    public TableInfo? FirstUsedTable(int columnId);
    
    public bool AddGroup(IDirectoryNode directory, string groupName, string groupDescription, out IDirectoryNode? createdDirectory);
    public bool AddItem(IDirectoryNode directory, string itemName, string itemDescription, out IFileNode? createdItem, Type itemType);
    public bool RemoveGroup(IDirectoryNode parent, IDirectoryNode group);
    public bool RemoveItem(IDirectoryNode parent, IFileNode item);
    public bool ModifyGroup(IDirectoryNode directory, string newName, string newDescription);
    public bool ModifyItem(IFileNode item, string newName, string newDescription);
    public bool IsItemNameExists(IDirectoryNode directoryNode, string itemName, string[]? exceptedItemNames = null);
    public IDirectoryNode? GetRootDirectory(IFileNode fileNode);
    public IDirectoryNode? GetRootDirectory(IDirectoryNode directoryNode);

    public List<DatabaseInfo> GetAllDatabases();
    public Dictionary<DatabaseInfo, List<TableInfo>> GetAllTables();
    public List<DatabaseInfo> GetAllHistoryDatabases();
    public Dictionary<DatabaseInfo, List<TableInfo>> GetAllHistoryTables();
    public TableInfo? GetTableById(int tableId);
    public DatabaseInfo? GetDatabaseInfoByTableId(int tableId);
    public bool UpdateTable(TableInfo tableInfo, List<int> columnIdList, List<IndexInfo> indexList, List<ForeignKeyInfo> foreignKeyList, bool hasHistoryTable, string remark, List<string> defaultValues);
}