using System.Collections.Generic;
using System.IO;
using Plugin.DevData;

namespace UniClient;

public class GenTask
{
    public string TaskName { get; set; } = string.Empty;
    public RecursionLevel RecursionLevel { get; set; }
    public string TemplateFile { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public ProgramLanguage ProgramLanguage { get; set; }
    public bool IsUsingString { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public bool IsIncludingHistoryDatabases { get; set; }
    
    public string OutputDir { get; set; } = string.Empty;
    public List<string> TargetDatabases { get; set; } = [];

    public string TemplateDir { get; set; } = string.Empty;
    
    
    public string GetTemplatePath()
    {
        return Path.Combine(TemplateDir, TemplateFile);
    }
}