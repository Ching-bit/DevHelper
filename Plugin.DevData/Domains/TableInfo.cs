using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class TableInfo : FileNode
{
    public List<int> ColumnIdList { get; set; } = [];
    public List<IndexInfo> IndexList { get; set; } = [];
    public string Remark { get; set; } = string.Empty;
    
    public override bool FromFile()
    {
        try
        {
            TableInfo fileContent = ObjectHelper.FromXmlFile<TableInfo>(FilePath);
            ColumnIdList.Clear();
            ColumnIdList.AddRange(fileContent.ColumnIdList);
            IndexList.Clear();
            IndexList.AddRange(fileContent.IndexList);
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