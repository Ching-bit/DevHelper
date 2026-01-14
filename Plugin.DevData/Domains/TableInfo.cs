using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class TableInfo : FileNode
{
    public List<int> ColumnIdList { get; set; } = [];
    public List<IndexInfo> IndexList { get; set; } = [];

    public override bool FromFile()
    {
        // TODO
        return true;
    }

    public override bool ToFile()
    {
        return ObjectHelper.ToXml(ConfigFilePath, this);
    }
}