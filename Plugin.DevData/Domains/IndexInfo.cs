namespace Plugin.DevData;

public class IndexInfo
{
    public string Name { get; set; } = string.Empty;
    public IndexType Type { get; set; }
    public List<string> ColumnList { get; } = [];
}