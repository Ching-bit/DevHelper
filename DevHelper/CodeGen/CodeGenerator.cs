using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Framework.Common;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Plugin.DevData;
using Plugin.DevData.Helpers;

namespace UniClient;

public class CodeGenerator
{
    public static async Task GenFile(GenTask task)
    {
        if (!Directory.Exists(task.OutputDir))
        {
            Directory.CreateDirectory(task.OutputDir);
        }
        
        if (task.TemplateFile.EndsWith(".cs"))
        {
            await GenFileUsingCSharpScript(task);
        }
        else if (task.TemplateFile.EndsWith(".template"))
        {
            await GenFileUsingTemplate(task);
        }
    }
    
    private static async Task GenFileUsingCSharpScript(GenTask task)
    {
        CodeGenScriptVars scriptVars = new()
        {
            Columns = Global.Get<IDevData>().Columns,
            TableRoot = Global.Get<IDevData>().TableRoot
        };
        string scriptContent = await File.ReadAllTextAsync(task.GetTemplatePath());
        await CSharpScript.EvaluateAsync(scriptContent,
            globals: scriptVars,
            options: Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location)))
                .WithImports("System", "System.IO", "System.Text", "System.Collections.Generic",
                    "Framework.Common", "Plugin.DevData", "UniClient"));
    }

    private static async Task GenFileUsingTemplate(GenTask task)
    {
        switch (task.RecursionLevel)
        {
            case RecursionLevel.Database:
            {
                foreach (DatabaseInfo database in Global.Get<IDevData>().GetAllTables().Keys)
                {
                    if (task.TargetDatabases.Count > 0 && !task.TargetDatabases.Contains(database.Name))
                    {
                        continue;
                    }
                    _currentDatabase = database;
                    string outputFileName = GenFile_Database(task.OutputFile, _currentDatabase, null);
                    string outputFilePath = Path.Combine(task.OutputDir, outputFileName);
                    
                    StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                    string templateText = await File.ReadAllTextAsync(task.GetTemplatePath());
                    string outputContent = GenFile_Database(templateText, _currentDatabase, task);
                    await sw.WriteAsync(outputContent);
                    sw.Close();
                }
                
                break;
            }
            case RecursionLevel.Table:
            {
                foreach (DatabaseInfo database in Global.Get<IDevData>().GetAllTables().Keys)
                {
                    if (task.TargetDatabases.Count > 0 && !task.TargetDatabases.Contains(database.Name))
                    {
                        continue;
                    }
                    _currentDatabase = database;
                    string outputDir =
                        task.TargetDatabases.Count > 1 ?
                        Path.Combine(task.OutputDir, database.Name) :
                        task.OutputDir;
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    
                    List<TableInfo> tableList = TableListSorter.Sort(Global.Get<IDevData>().GetAllTables()[database]);
                    foreach (TableInfo tableInfo in tableList)
                    {
                        _currentTable = tableInfo;
                        string outputFileName = GenFile_Table(task.OutputFile, tableInfo, null);
                        string outputFilePath = Path.Combine(outputDir, outputFileName);
                    
                        StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                        string templateText = await File.ReadAllTextAsync(task.GetTemplatePath());
                        string outputContent = GenFile_Table(templateText, tableInfo, task);
                        await sw.WriteAsync(outputContent);
                        sw.Close();
                    }
                }
                
                break;
            }
        }
    }
    
    // database level template
    private static string GenFile_Database(string templateText, DatabaseInfo databaseInfo, GenTask? task)
    {
        return GenFile_Template(templateText,
            new Dictionary<string, string>
            {
                { "DatabaseName", databaseInfo.Name },
                { "DatabaseDescription", databaseInfo.Description }
            },
            [],
            task);
    }
    
    // table level template
    private static string GenFile_Table(string templateText, TableInfo tableInfo, GenTask? task)
    {
        IndexInfo? primaryKeyInfo = tableInfo.IndexList.FirstOrDefault(x => IndexType.Primary == x.Type);
        
        List<ColumnInfo> columns = tableInfo.ColumnIdList.Select(x => Global.Get<IDevData>().Columns.First(y => y.Id == x)).ToList();
        List<ColumnInfo> primaryKeyColumns = columns.Where(x => primaryKeyInfo?.ColumnIdList.Contains(x.Id) ?? false).ToList();
        List<ColumnInfo> generalColumns = columns.Except(primaryKeyColumns).ToList();
        List<IndexInfo> indexes = tableInfo.IndexList.Where(x => x.Type is IndexType.Unique or IndexType.Index).ToList();
        List<ForeignKeyInfo> foreignKeys = tableInfo.ForeignKeyList;
        
        return GenFile_Template(templateText,
            new Dictionary<string, string>
            {
                { "DatabaseName", Global.Get<IDevData>().GetDatabaseInfoByTableId(tableInfo.Id)?.Name ?? string.Empty },
                { "TableName", tableInfo.Name },
                { "TableDescription", tableInfo.Description },
            },
            null == task ? [] :
            [
                // columns related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ColumnName", x => ((ColumnInfo)x).Name },
                        { "ColumnDescription", x => ((ColumnInfo)x).Description },
                        { "ColumnDbType", x => ToDbType((ColumnInfo)x, task.DatabaseType) },
                        { "ColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "ColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "ColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "ColumnProgramType", x => ToProgramType((ColumnInfo)x, task) },
                        { "ColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," }
                    },
                    columns.ConvertAll<object>(x => x)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "GeneralColumnName", x => ((ColumnInfo)x).Name },
                        { "GeneralColumnDescription", x => ((ColumnInfo)x).Description },
                        { "GeneralColumnDbType", x => ToDbType((ColumnInfo)x, task.DatabaseType) },
                        { "GeneralColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "GeneralColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "GeneralColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "GeneralColumnProgramType", x => ToProgramType((ColumnInfo)x, task) },
                        { "GeneralColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," }
                    },
                    generalColumns.ConvertAll<object>(x => x)),
                // primary key related
                Tuple.Create
                (
                    new Dictionary<string, Func<object, string>>
                    {
                        { "PrimaryKeyName", _ => primaryKeyInfo?.Name ?? string.Empty },
                        { "PrimaryKeyColumnCount", _ => primaryKeyColumns.Count + "" },
                        { "PrimaryKeyColumns", _ => string.Join(", ", primaryKeyColumns.Select(x => x.Name)) },
                        { "PrimaryKeyColumnsWithBackQuota", _ => string.Join(", ", primaryKeyColumns.Select(x => $"`{x.Name}`")) },
                        { "PrimaryKeyColumnName", x => ((ColumnInfo)x).Name },
                        { "PrimaryKeyColumnDescription", x => ((ColumnInfo)x).Description },
                        { "PrimaryKeyColumnDbType", x => ToDbType((ColumnInfo)x, task.DatabaseType) },
                        { "PrimaryKeyColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "PrimaryKeyColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "PrimaryKeyColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "PrimaryKeyColumnProgramType", x => ToProgramType((ColumnInfo)x, task) },
                        { "PrimaryKeyColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," },
                        { "PrimaryKeyColumnIndex", x => primaryKeyColumns.IndexOf((ColumnInfo)x) + "" }
                    },
                    primaryKeyColumns.ConvertAll<object>(x => x)),
                // indexes related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "IndexType", x => ((IndexInfo)x).Type.ToString().ToLower() },
                        { "IndexTypeWithKey", x => IndexType.Unique == ((IndexInfo)x).Type ? "unique key" : "index" },
                        { "IndexName", x => ((IndexInfo)x).Name },
                        { "IndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => Global.Get<IDevData>().Columns.First(z => y == z.Id).Name)) },
                        { "IndexColumnsWithBackQuota", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => "`" + Global.Get<IDevData>().Columns.First(z => y == z.Id).Name + "`")) },
                    },
                    indexes.ConvertAll<object>(y => y)),
                // foreign keys related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ForeignKeyName", x => ((ForeignKeyInfo)x).Name },
                        { "ForeignKeyColumn", x => Global.Get<IDevData>().Columns.FirstOrDefault(y => y.Id == ((ForeignKeyInfo)x).ColumnId)?.Name ?? string.Empty},
                        { "ForeignKeyReferenceTableName", x => Global.Get<IDevData>().GetTableById(((ForeignKeyInfo)x).TableId)?.Name ?? string.Empty},
                        { "ForeignKeyReferenceColumnName", x => Global.Get<IDevData>().Columns.FirstOrDefault(y => y.Id == ((ForeignKeyInfo)x).ReferenceColumnId)?.Name ?? string.Empty},
                    },
                    foreignKeys.ConvertAll<object>(y => y)),
                ],
            task);
    }

    private static string ToDbType(ColumnInfo columnInfo, DatabaseType databaseType)
    {
        if (DatabaseType.MySQL == databaseType)
        {
            return columnInfo.Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "bigint",
                ColumnType.Number =>
                    $"number({columnInfo.Length}{(columnInfo.Scale > 0 ? $", {columnInfo.Scale}" : string.Empty)})",
                ColumnType.Char => $"char({columnInfo.Length})",
                ColumnType.Varchar => $"varchar({columnInfo.Length})",
                ColumnType.Bool => "tinyint",
                ColumnType.Datetime => "datetime",
                _ => string.Empty
            };
        }
        if (DatabaseType.Oracle == databaseType)
        {
            return columnInfo.Type switch
            {
                ColumnType.Int32 => "number(10)",
                ColumnType.Int64 => "number(19)",
                ColumnType.Number =>
                    $"number({columnInfo.Length}{(columnInfo.Scale > 0 ? $", {columnInfo.Scale}" : string.Empty)})",
                ColumnType.Char => $"char({columnInfo.Length})",
                ColumnType.Varchar => $"varchar2({columnInfo.Length})",
                ColumnType.Bool => "number(1)",
                ColumnType.Datetime => "date",
                _ => string.Empty
            };
        }
        if (DatabaseType.SQLServer == databaseType)
        {
            return columnInfo.Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "bigint",
                ColumnType.Number =>
                    $"decimal({columnInfo.Length}{(columnInfo.Scale > 0 ? $", {columnInfo.Scale}" : string.Empty)})",
                ColumnType.Char => $"char({columnInfo.Length})",
                ColumnType.Varchar => $"varchar({columnInfo.Length})",
                ColumnType.Bool => "bit",
                ColumnType.Datetime => "datetime",
                _ => string.Empty
            };
        }

        return string.Empty;
    }
    
    private static string ToDbDefaultValue(ColumnInfo columnInfo)
    {
        return columnInfo.Type switch
        {
            ColumnType.Int32 or ColumnType.Int64 or ColumnType.Number => columnInfo.DefaultValue,
            ColumnType.Char or ColumnType.Varchar or ColumnType.Datetime => $"'{columnInfo.DefaultValue}'",
            ColumnType.Bool => columnInfo.DefaultValue.ToLower() switch
            {
                "false" => "0",
                "true" => "1",
                _ => columnInfo.DefaultValue
            },
            _ => string.Empty
        };
    }
    
    private static string ToProgramType(ColumnInfo columnInfo, GenTask task)
    {
        return task.ProgramLanguage switch
        {
            ProgramLanguage.Cpp => columnInfo.Type switch
            {
                ColumnType.Int32 => "int32_t",
                ColumnType.Int64 => "int64_t",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar => task.IsUsingString
                    ? "std::string"
                    : $"char[{columnInfo.Length * columnInfo.Scale + 1}]",
                ColumnType.Bool => "bool",
                ColumnType.Datetime => "std::chrono",
                _ => string.Empty
            },
            ProgramLanguage.CSharp => columnInfo.Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "long",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar => task.IsUsingString
                    ? "string"
                    : $"byte[{columnInfo.Length * columnInfo.Scale + 1}]",
                ColumnType.Bool => "bool",
                ColumnType.Datetime => "DateTime",
                _ => string.Empty
            },
            ProgramLanguage.Java => columnInfo.Type switch
            {
                ColumnType.Int32 => "Int",
                ColumnType.Int64 => "Long",
                ColumnType.Number => "Double",
                ColumnType.Char or ColumnType.Varchar => task.IsUsingString
                    ? "String"
                    : $"Byte[{columnInfo.Length * columnInfo.Scale + 1}]",
                ColumnType.Bool => "Boolean",
                ColumnType.Datetime => "LocalDateTime",
                _ => string.Empty
            },
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// General method to generate a file
    /// </summary>
    /// <param name="templateText"></param>
    /// <param name="globalMacros"></param>
    /// <param name="repeatedMacros"></param>
    /// <param name="task"></param>
    /// <returns>file content</returns>
    private static string GenFile_Template(string templateText, Dictionary<string, string> globalMacros, List<Tuple<Dictionary<string, Func<object, string>>, List<object>>> repeatedMacros, GenTask? task)
    {
        StringBuilder outputContent = new();
        StringReader templateReader = new(templateText);
        
        Dictionary<string, string> defaultGlobalMacros = new()
        {
            { "NEWLINE", Environment.NewLine },
        };
        
        globalMacros = globalMacros.Concat(defaultGlobalMacros).ToDictionary();

        string? line = templateReader.ReadLine();
        while (line != null)
        {
            // replace global micros
            foreach (string key in globalMacros.Keys)
            {
                while (ContainsMacro(line, key, out _, out _, out _))
                {
                    line = ReplaceMacro(line, key, globalMacros[key]);
                }
            }
            string outputLine = line;

            // replace repeated micros
            Tuple<Dictionary<string, Func<object, string>>, List<object>>? repeatedMacro = null;
            foreach (Tuple<Dictionary<string, Func<object, string>>, List<object>> macro in repeatedMacros)
            {
                foreach (string macroKey in macro.Item1.Keys)
                {
                    if (ContainsMacro(line, macroKey, out _, out _, out _))
                    {
                        repeatedMacro = macro;
                        break;
                    }
                }
            }

            if (null != repeatedMacro)
            {
                IList<object> columns = repeatedMacro.Item2;
                outputLine = "";
                foreach (object column in columns)
                {
                    string lineCopy = new(line);
                    foreach (string macroKey in repeatedMacro.Item1.Keys)
                    {
                        string macroValue = repeatedMacro.Item1[macroKey](column);
                        while (ContainsMacro(lineCopy, macroKey, out _, out _, out _))
                        {
                            lineCopy = ReplaceMacro(lineCopy, macroKey, macroValue);
                        }
                    }
                    outputLine += lineCopy + Environment.NewLine;
                }
            }
            
            // replace special micros
            // TEMPLATE
            if (null != task)
            {
                if (ContainsMacro(outputLine, "TEMPLATE", out int startIndex, out int endIndex, out string remark))
                {
                    string[] arr = remark.Split(",");
                    if (arr.Length == 2)
                    {
                        string subTemplatePath = Path.Combine(task.TemplateDir, arr[0]);
                        string subTemplateText = File.ReadAllText(subTemplatePath);
                        RecursionLevel recursionLevel = Enum.Parse<RecursionLevel>(arr[1]);
                        string subOutputContent = GenFile_SubTemplate(subTemplateText, recursionLevel, task);
                        outputLine = ReplaceRange(outputLine, startIndex, endIndex, subOutputContent);
                    }
                }
            }
            
            line = templateReader.ReadLine();
            if (outputLine.Length > 0 || null == repeatedMacro)
            {
                if (null != line && !outputLine.EndsWith(Environment.NewLine))
                {
                    outputLine += Environment.NewLine;
                }
                outputContent.Append(outputLine);
            }
        }

        templateReader.Close();
        string ret = outputContent.ToString();
        
        // replace special micros
        // BACKSPACE
        {
            while (ContainsMacro(ret, "BACKSPACE", out int startIndex, out int endIndex, out _)) 
            {
                ret = ReplaceRange(ret, startIndex, endIndex, string.Empty);
                if (startIndex <= 0) { continue; }
                
                if (ret[..startIndex].EndsWith(Environment.NewLine))
                {
                    ret = ReplaceRange(ret, startIndex - Environment.NewLine.Length, startIndex - 1, string.Empty);
                }
                else
                {
                    ret = ReplaceRange(ret, startIndex - 1, startIndex - 1, string.Empty);
                }
            }
        }
        return ret;
    }

    private static string ReplaceMacro(string input, string macro, string macroValue)
    {
        if (ContainsMacro(input, macro, out int startIndex, out int endIndex, out string remark))
        {
            _ = Enum.TryParse(remark, out NameStyle nameStyle);
            input = ReplaceRange(input, startIndex, endIndex, StringHelper.ToNameStyle(macroValue, nameStyle));
        }
        return input;
    }

    private static bool ContainsMacro(string line, string macro, out int startIndex, out int endIndex, out string remark)
    {
        startIndex = -1;
        endIndex = -1;
        remark = string.Empty;
        
        string pattern = $@"\$\{{{Regex.Escape(macro)}(?:\((?<arg>[^\)]*)\))?\}}";

        var match = Regex.Match(line, pattern);

        if (!match.Success) { return false; }
        
        startIndex = match.Index;
        endIndex = match.Index + match.Length - 1;
        remark = match.Groups["arg"].Success ? match.Groups["arg"].Value : string.Empty;
        return true;
    }
    
    private static string ReplaceRange(string input, int startIndex, int endIndex, string replacement)
    {
        if (startIndex < 0 || endIndex >= input.Length || startIndex > endIndex)
        {
            throw new ArgumentOutOfRangeException();
        }
        
        string before = input[..startIndex];
        string after = input[(endIndex + 1)..];

        return before + replacement + after;
    }

    private static string GenFile_SubTemplate(string templateText, RecursionLevel recursionLevel, GenTask task)
    {
        switch (recursionLevel)
        {
            case RecursionLevel.Database:
                return null != _currentDatabase ? GenFile_Database(templateText, _currentDatabase, task) : string.Empty;
            case RecursionLevel.Table when null == _currentDatabase:
                throw new ArgumentException("Null database info when recurse tables");
            case RecursionLevel.Table:
            {
                StringBuilder sb = new();
                List<TableInfo> tables = Global.Get<IDevData>().GetAllTables()[_currentDatabase];
                tables = TableListSorter.Sort(tables);
                foreach (TableInfo tableInfo in tables)
                {
                    _currentTable = tableInfo;
                    sb.Append(GenFile_Table(templateText, tableInfo, task));
                }
                
                return sb.ToString();
            }
            case RecursionLevel.HistoryDatabase:
            case RecursionLevel.HistoryTable:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    
    #region Pointers
    private static DatabaseInfo? _currentDatabase;
    private static TableInfo? _currentTable;
    #endregion
}