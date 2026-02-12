using Framework.Common;
using Plugin.AppEnv;

namespace Plugin.DevData;

public class ColumnInfo
{
    public const string DefaultColumnGroup = "Default";
    public const int ArchiveDateColumnId = -1;
    
    public string Group { get; set; } = DefaultColumnGroup;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }  = string.Empty;
    public ColumnType Type { get; set; }
    public int Length { get; set; }
    public int Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool HasDefaultValue { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public string DataDict { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;

    public static ColumnInfo GetArchiveDateColumn()
    {
        return new ColumnInfo
        {
            Id = ArchiveDateColumnId,
            Name = Global.Get<IUserSetting>().ArchiveDateColumnName,
            Type = ColumnType.Int32,
            IsNullable = false,
        };
    }
}