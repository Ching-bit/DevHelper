using Framework.Common;

namespace Plugin.DevData;

public interface IDevData : IPlugin
{
    public Dictionary<string, DictSet> DictSets { get; }
    public List<ColumnInfo> Columns { get; }
    public Dictionary<string, List<TableInfo>> Tables { get; }
}