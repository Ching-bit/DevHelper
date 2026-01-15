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
    
    public bool AddGroup<T>(DirectoryNode<T> directory, string groupName, string groupDescription, out DirectoryNode<T>? createdDirectory) where T : FileNode, new();
    public bool AddItem<T>(DirectoryNode<T> directory, string itemName, string itemDescription, out T? createdItem) where T : FileNode, new();
    public bool RemoveGroup<T>(DirectoryNode<T> parent, DirectoryNode<T> group) where T : FileNode, new();
    public bool RemoveItem<T>(DirectoryNode<T> parent, T item) where T : FileNode, new();
}