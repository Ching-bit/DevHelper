using System.Collections.Generic;
using Plugin.DevData;

namespace UniClient;

public class CodeGenScriptVars
{
    public GenTask? GenTask { get; set; }
    public List<ColumnInfo> Columns { get; set; } = [];
    public IDirectoryNode? TableRoot { get; set; }
    public IDirectoryNode? ApiRoot { get; set; }
    public Dictionary<DatabaseInfo, List<TableInfo>> Tables { get; set; } = [];
    public Dictionary<DatabaseInfo, List<TableInfo>> HistoryTables { get; set; } = [];
    public List<ApiInfo> Apis { get; set; } = [];
}