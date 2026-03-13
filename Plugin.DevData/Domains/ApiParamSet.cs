namespace Plugin.DevData;

public class ApiParamSet
{
    public ApiParamSetType Type { get; set; }
    public ApiParamSetMode Mode { get; set; }
    public int TableId { get; set; }
    public List<int> ColumnIdList { get; set; } = [];
}