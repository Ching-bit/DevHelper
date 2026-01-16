namespace Plugin.DevData;

public class FileNode
{
    public string ConfigFilePath { get; set; } = string.Empty;
    
    public string Name
    {
        get
        {
            string fileName = Path.GetFileNameWithoutExtension(ConfigFilePath);
            return fileName.Split("@")[0];
        }
    }

    public string Description
    {
        get
        {
            string fileName = Path.GetFileNameWithoutExtension(ConfigFilePath);
            string[] arr = fileName.Split("@");
            return arr.Length >= 2 ? arr[1] : string.Empty;
        }
    }

    public string MenuName => Name + (string.IsNullOrEmpty(Description) ? "" : $" ({Description})");

    public virtual bool FromFile() => true;
    public virtual bool ToFile() => true;
}