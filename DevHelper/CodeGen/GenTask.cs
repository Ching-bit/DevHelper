namespace UniClient;

public class GenTask
{
    public string TaskName { get; set; } = string.Empty;
    public RecursionLevel RecursionLevel { get; set; }
    public string TemplateFile { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public string OutputDir { get; set; } = string.Empty;

    public string TemplateDir { get; set; } = string.Empty;
}