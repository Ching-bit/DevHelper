namespace Framework.Common;

[AttributeUsage(AttributeTargets.Property)]
public class ResourceNameAttribute : Attribute
{
    public ResourceNameAttribute(string name)
    {
        Name = name;
    }
    
    public string Name { get; set; }
}