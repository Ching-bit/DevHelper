using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class TableInfo : FileNode
{
    #region Constructors
    public TableInfo() : base() { }

    public TableInfo(string name, string description, IDirectoryNode? parent)
        : base(name, description, parent)
    {
        // max Id + 1 to this table
        List<TableInfo> allTableList = Global.Get<IDevData>().GetAllTables().Values.SelectMany(list => list).ToList();
        Id = allTableList.Count > 0 ? allTableList.Max(x => x.Id) + 1 : 1;
    }
    #endregion
    
    
    #region Properties
    public int Id { get; set; }
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
            Id = fileContent.Id;
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