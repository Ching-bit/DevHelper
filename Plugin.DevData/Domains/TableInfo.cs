namespace Plugin.DevData;

public class TableInfo : FileNode
{
    public List<int> ColumnIdList { get; set; } = [];
    public List<IndexInfo> IndexList { get; set; } = [];

    public override void FromFile()
    {
        // TODO
    }

    public override void ToFile()
    {
        // TODO
    }
}