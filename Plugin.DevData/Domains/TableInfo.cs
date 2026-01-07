namespace Plugin.DevData;

public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }  = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = [];
    public List<IndexInfo> Indexes { get; set; } = [];
}