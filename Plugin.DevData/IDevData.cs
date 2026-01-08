using Framework.Common;

namespace Plugin.DevData;

public interface IDevData : IPlugin
{
    public List<ColumnInfo> Columns { get; set; }
    public Dictionary<string, List<TableInfo>> Tables { get; }
}