using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class TableInfo : FileNode
{
    #region Constructors
    public TableInfo() : base() { }
    public TableInfo(string name, string description, IDirectoryNode? parent)
        : base(name, description, parent) { }
    #endregion
    
    
    #region Properties
    public List<int> ColumnIdList { get; set; } = [];
    public List<IndexInfo> IndexList { get; set; } = [];
    public List<ForeignKeyInfo> ForeignKeyList { get; set; } = [];
    public bool HasHistoryTable { get; set; }
    public string Remark { get; set; } = string.Empty;
    #endregion
    
    public override bool FromFile()
    {
        try
        {
            TableInfo fileContent = ObjectHelper.FromXmlFile<TableInfo>(FilePath);
            ColumnIdList.Clear();
            ColumnIdList.AddRange(fileContent.ColumnIdList);
            IndexList.Clear();
            IndexList.AddRange(fileContent.IndexList);
            ForeignKeyList.Clear();
            ForeignKeyList.AddRange(fileContent.ForeignKeyList);
            HasHistoryTable = fileContent.HasHistoryTable;
            Remark = fileContent.Remark;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool ToFile()
    {
        return ObjectHelper.ToXml(FilePath, this);
    }
}