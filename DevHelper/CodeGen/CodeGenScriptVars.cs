using System.Collections.Generic;
using Plugin.DevData;

namespace UniClient;

public class CodeGenScriptVars
{
    public List<ColumnInfo> Columns { get; set; } = [];
    public IDirectoryNode? TableRoot { get; set; }
    public Dictionary<DatabaseInfo, List<TableInfo>> Tables { get; set; } = [];
    public Dictionary<DatabaseInfo, List<TableInfo>> HistoryTables { get; set; } = [];
}