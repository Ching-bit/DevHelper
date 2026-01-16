using Framework.Common;

namespace Plugin.DevData;

public interface IDevData : IPlugin
{
    public List<ColumnInfo> Columns { get; }
    public DirectoryNode TableRoot { get; }

    public bool AddColumn(ColumnInfo columnInfo);
    public bool RemoveColumn(int id);
    public bool ModifyColumn(ColumnInfo columnInfo);
    public bool SaveColumns();
    
    public bool AddGroup(DirectoryNode directory, string groupName, string groupDescription, out DirectoryNode? createdDirectory);
    public bool AddItem<T>(DirectoryNode directory, string itemName, string itemDescription, out T? createdItem) where T : FileNode, new();
    public bool RemoveGroup(DirectoryNode parent, DirectoryNode group);
    public bool RemoveItem(DirectoryNode parent, FileNode item);
    public bool ModifyGroup(DirectoryNode directory, string newName, string newDescription);
    public bool ModifyItem(FileNode item, string newName, string newDescription);
}