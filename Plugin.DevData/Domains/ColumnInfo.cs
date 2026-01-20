namespace Plugin.DevData;

public class ColumnInfo
{
    public const string DEFAULT_COLUMN_NAME = "Default";
    
    public string Group { get; set; } = DEFAULT_COLUMN_NAME;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }  = string.Empty;
    public ColumnType Type { get; set; }
    public int Length { get; set; }
    public int Scale { get; set; }
    public string DataDict { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Id} {Name}";
    }
}