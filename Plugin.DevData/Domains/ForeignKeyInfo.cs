namespace Plugin.DevData;

public class ForeignKeyInfo
{
    public int ColumnId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int ReferenceColumnId { get; set; }
}