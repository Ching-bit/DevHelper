namespace Plugin.AppEnv;

public class GenTaskConf
{
    public string TemplateDir { get; set; } = string.Empty;
    
    public string OutputDir { get; set; } = string.Empty;
    public List<string> TargetDatabases { get; set; } = [];
}