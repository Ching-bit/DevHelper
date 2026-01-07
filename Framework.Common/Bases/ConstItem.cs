namespace Framework.Common;

public class ConstItem
{
    public ConstItem(string key, string? resourceName, string? value)
    {
        Key = key;
        _resourceName = resourceName;
        _value = value;
    }

    public string Key { get; }
    private readonly string? _resourceName;
    private readonly string? _value;
    
    public override string ToString()
    {
        return (null != _resourceName ?
            $"{Key} {ResourceHelper.FindStringResource(_resourceName)}" :
            $"{Key} {_value ?? string.Empty}").Trim();
    }
}