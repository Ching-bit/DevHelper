namespace Plugin.DevData;

public class DatabaseInfo
{
    public DatabaseInfo(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
    public string Description { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is DatabaseInfo databaseInfo)
        {
            return Name.Equals(databaseInfo.Name);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }
    
}