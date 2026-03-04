namespace Plugin.DevData;

public class ApiParamSet
{
    public ApiParamSetMode Mode { get; set; }
    public int TableId { get; set; }
    public List<int> ColumnIdList { get; set; } = [];
}