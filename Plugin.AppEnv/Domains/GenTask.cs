namespace Plugin.AppEnv;

public class GenTask
{
    public GenTaskType TaskType { get; set; }
    public string TaskNameResource { get; set; } = string.Empty;
    public string OutputDir { get; set; } = string.Empty;
}