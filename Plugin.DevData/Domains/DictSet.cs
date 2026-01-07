namespace Plugin.DevData;

public class DictSet
{
    public string Id { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public List<DictItem> DictItems { get; set; } = [];
}