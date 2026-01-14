using Framework.Common;

namespace Plugin.DevData;

public interface IDevData : IPlugin
{
    public List<ColumnInfo> Columns { get; }
    public DirectoryNode<TableInfo> TableRoot { get; }

    public bool AddColumn(ColumnInfo columnInfo);
    public bool RemoveColumn(int id);
    public bool ModifyColumn(ColumnInfo columnInfo);
    public bool SaveColumns();
    
    public bool AddTableGroup(DirectoryNode<TableInfo> directory, string groupName, string groupDescription, out DirectoryNode<TableInfo>? createdDirectory);
    public bool AddTable(DirectoryNode<TableInfo> directory, string tableName, string tableDescription, out TableInfo? createdTable);
}