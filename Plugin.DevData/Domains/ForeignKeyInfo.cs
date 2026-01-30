namespace Plugin.DevData;

public class ForeignKeyInfo
{
    public int ColumnId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TableId { get; set; }
    public int ReferenceColumnId { get; set; }
}