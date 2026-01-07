using System.Reflection;

namespace Framework.Common;

public class ConstBase : List<ConstItem>
{
    protected ConstBase()
    {
        Type type = GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        Array.ForEach(fields, field =>
        {
            ResourceNameAttribute? resourceNameAttribute = field.GetCustomAttribute<ResourceNameAttribute>();
            string key = field.GetRawConstantValue()?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(key))
            {
                RemoveAll(x => x.Key == key);
                Add(new ConstItem(key, resourceNameAttribute?.Name, field.Name));
            }
            else
            {
                Add(new ConstItem(field.Name, resourceNameAttribute?.Name, null));
            }
        });
    }

    public List<string> Keys()
    {
        return this.Select(x => x.Key).ToList();
    }
}