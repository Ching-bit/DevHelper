using System.Collections.Generic;
using Plugin.DevData;

namespace UniClient;

public class CodeGenScriptVars
{
    public List<ColumnInfo> Columns { get; set; } = [];
    public IDirectoryNode? TableRoot { get; set; }
}