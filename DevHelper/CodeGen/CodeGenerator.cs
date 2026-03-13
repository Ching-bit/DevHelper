using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        List<ColumnInfo> columns = [];
        columns.AddRange(Global.Get<IDevData>().Columns);
        columns.Add(ColumnInfo.GetArchiveDateColumn());
        
        CodeGenScriptVars scriptVars = new()
        {
            GenTask = task,
            Columns = columns,
            TableRoot = Global.Get<IDevData>().TableRoot,
            ApiRoot = Global.Get<IDevData>().ApiRoot,
            Tables = Global.Get<IDevData>().GetAllTables(),
            HistoryTables = Global.Get<IDevData>().GetAllHistoryTables(),
            Apis = Global.Get<IDevData>().GetAllApis(false)
        };
        
        string scriptContent = await File.ReadAllTextAsync(task.GetTemplatePath());
        await CSharpScript.EvaluateAsync(scriptContent,
            globals: scriptVars,
            options: Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))));
    }

    private static async Task GenFileUsingTemplate(GenTask task)
    {
        _allTables = Global.Get<IDevData>().GetAllTables();
        if (task.IsIncludingHistoryDatabases)
        {
            _allTables = _allTables.Concat(Global.Get<IDevData>().GetAllHistoryTables()).ToDictionary();
        }
        
        switch (task.RecursionLevel)
        {
            case RecursionLevel.Database:
            {
                foreach (DatabaseInfo database in _allTables.Keys)
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
                foreach (DatabaseInfo database in _allTables.Keys)
                {
                    if (task.TargetDatabases.Count > 0 && !task.TargetDatabases.Contains(database.Name))
                    {
                        continue;
                    }
                    _currentDatabase = database;
                    string outputDir =
                        task.TargetDatabases.Count > 1 ?
                        Path.Combine(task.OutputDir, _currentDatabase.Name) :
                        task.OutputDir;
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    
                    foreach (TableInfo tableInfo in TableListSorter.Sort(_allTables[_currentDatabase]))
                    {
                        _currentTable = tableInfo;
                        string outputFileName = GenFile_Table(task.OutputFile, _currentTable, null);
                        string outputFilePath = Path.Combine(outputDir, outputFileName);
                    
                        StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                        string templateText = await File.ReadAllTextAsync(task.GetTemplatePath());
                        string outputContent = GenFile_Table(templateText, _currentTable, task);
                        await sw.WriteAsync(outputContent);
                        sw.Close();
                    }
                }
                
                break;
            }
            case RecursionLevel.Api:
            {
                _allApis = Global.Get<IDevData>().GetAllApis(false);
                foreach (ApiInfo apiInfo in _allApis)
                {
                    _currentApi = apiInfo;
                    string outputFileName = GenFile_Api(task.OutputFile, _currentApi, null);
                    string outputFilePath = Path.Combine(task.OutputDir, outputFileName);
                    
                    StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                    string templateText = await File.ReadAllTextAsync(task.GetTemplatePath());
                    string outputContent = GenFile_Api(templateText, _currentApi, task);
                    await sw.WriteAsync(outputContent);
                    sw.Close();
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

        List<ColumnInfo> allColumns = [];
        allColumns.AddRange(Global.Get<IDevData>().Columns);
        allColumns.Add(ColumnInfo.GetArchiveDateColumn());
        
        List<ColumnInfo> columns = tableInfo.ColumnIdList.Select(x => allColumns.First(y => y.Id == x)).ToList();
        List<ColumnInfo> primaryKeyColumns = columns.Where(x => primaryKeyInfo?.ColumnIdList.Contains(x.Id) ?? false).ToList();
        List<ColumnInfo> generalColumns = columns.Except(primaryKeyColumns).ToList();
        List<IndexInfo> indexes = tableInfo.IndexList.Where(x => x.Type is IndexType.Unique or IndexType.Index).ToList();
        List<IndexInfo> uniqueIndexes = tableInfo.IndexList.Where(x => x.Type is IndexType.Unique).ToList();
        List<IndexInfo> nonUniqueIndexes = tableInfo.IndexList.Where(x => x.Type is IndexType.Index).ToList();
        List<ForeignKeyInfo> foreignKeys = tableInfo.ForeignKeyList;
        ColumnInfo? autoIncColumn = null == primaryKeyInfo?.AutoIncrementColumnId ? null : allColumns.FirstOrDefault(x => x.Id == primaryKeyInfo.AutoIncrementColumnId);
        
        return GenFile_Template(templateText,
            new Dictionary<string, string>
            {
                { "DatabaseName", Global.Get<IDevData>().GetDatabaseInfoByTableId(tableInfo.Id)?.Name ?? string.Empty },
                { "TableName", tableInfo.Name },
                { "TableDescription", tableInfo.Description },
                { "PrimaryKeyName", primaryKeyInfo?.Name ?? string.Empty },
                { "PrimaryKeyColumnCount", primaryKeyColumns.Count + "" },
                { "PrimaryKeyColumns", string.Join(", ", primaryKeyColumns.Select(x => x.Name)) },
                { "PrimaryKeyColumnsWithBackQuota", string.Join(", ", primaryKeyColumns.Select(x => $"`{x.Name}`")) },
            },
            null == task ? [] :
            [
                // columns related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ColumnName", x => ((ColumnInfo)x).Name },
                        { "ColumnDescription", x => ((ColumnInfo)x).Description },
                        { "ColumnDbType", x => ((ColumnInfo)x).GetDbType(task.DatabaseType) },
                        { "ColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? "default" : string.Empty},
                        { "ColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ((ColumnInfo)x).GetDbDefaultValue() : string.Empty },
                        { "ColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : "not null" },
                        { "ColumnProgramType", x => ((ColumnInfo)x).GetProgramType(task.ProgramLanguage, task.IsUsingString) },
                        { "ColumnHungarianPrefix", x => ((ColumnInfo)x).GetHungarianPrefix() },
                        { "ColumnComma", x => ((ColumnInfo)x).Id != tableInfo.ColumnIdList[^1] ? "," : string.Empty }
                    },
                    columns.ConvertAll<object>(x => x)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "GeneralColumnName", x => ((ColumnInfo)x).Name },
                        { "GeneralColumnDescription", x => ((ColumnInfo)x).Description },
                        { "GeneralColumnDbType", x => ((ColumnInfo)x).GetDbType(task.DatabaseType) },
                        { "GeneralColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? "default" : string.Empty},
                        { "GeneralColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ((ColumnInfo)x).GetDbDefaultValue() : string.Empty },
                        { "GeneralColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : "not null" },
                        { "GeneralColumnProgramType", x => ((ColumnInfo)x).GetProgramType(task.ProgramLanguage, task.IsUsingString) },
                        { "GeneralColumnHungarianPrefix", x => ((ColumnInfo)x).GetHungarianPrefix() },
                        { "GeneralColumnComma", x => ((ColumnInfo)x).Id != tableInfo.ColumnIdList[^1] ? "," : string.Empty }
                    },
                    generalColumns.ConvertAll<object>(x => x)),
                // primary key related
                Tuple.Create
                (
                    new Dictionary<string, Func<object, string>>
                    {
                        { "PrimaryKeyColumnName", x => ((ColumnInfo)x).Name },
                        { "PrimaryKeyColumnDescription", x => ((ColumnInfo)x).Description },
                        { "PrimaryKeyColumnDbType", x => ((ColumnInfo)x).GetDbType(task.DatabaseType) },
                        { "PrimaryKeyColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? "default" : string.Empty},
                        { "PrimaryKeyColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ((ColumnInfo)x).GetDbDefaultValue() : string.Empty },
                        { "PrimaryKeyColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : "not null" },
                        { "PrimaryKeyColumnProgramType", x => ((ColumnInfo)x).GetProgramType(task.ProgramLanguage, task.IsUsingString) },
                        { "PrimaryKeyColumnHungarianPrefix", x => ((ColumnInfo)x).GetHungarianPrefix() },
                        { "PrimaryKeyColumnComma", x => ((ColumnInfo)x).Id != tableInfo.ColumnIdList[^1] ? "," : string.Empty },
                        { "PrimaryKeyColumnIndex", x => primaryKeyColumns.IndexOf((ColumnInfo)x) + "" },
                        { "PrimaryKeyColumnAutoIncrement", x => primaryKeyInfo?.AutoIncrementColumnId == ((ColumnInfo)x).Id ? "auto_increment" : string.Empty},
                    },
                    primaryKeyColumns.ConvertAll<object>(x => x)),
                // indexes related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "IndexType", x => ((IndexInfo)x).Type.ToString().ToLower() },
                        { "IndexTypeWithKey", x => IndexType.Unique == ((IndexInfo)x).Type ? "unique key" : "index" },
                        { "IndexName", x => ((IndexInfo)x).Name },
                        { "IndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => allColumns.First(z => y == z.Id).Name)) },
                        { "IndexColumnsWithBackQuota", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => "`" + allColumns.First(z => y == z.Id).Name + "`")) },
                    },
                    indexes.ConvertAll<object>(y => y)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "UniqueIndexName", x => ((IndexInfo)x).Name },
                        { "UniqueIndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => allColumns.First(z => y == z.Id).Name)) },
                        { "UniqueIndexColumnsWithBackQuota", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => "`" + allColumns.First(z => y == z.Id).Name + "`")) },
                    },
                    uniqueIndexes.ConvertAll<object>(y => y)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "NonUniqueIndexName", x => ((IndexInfo)x).Name },
                        { "NonUniqueIndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => allColumns.First(z => y == z.Id).Name)) },
                        { "NonUniqueIndexColumnsWithBackQuota", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => "`" + allColumns.First(z => y == z.Id).Name + "`")) },
                    },
                    nonUniqueIndexes.ConvertAll<object>(y => y)),
                // foreign keys related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ForeignKeyName", x => ((ForeignKeyInfo)x).Name },
                        { "ForeignKeyColumnName", x => allColumns.FirstOrDefault(y => y.Id == ((ForeignKeyInfo)x).ColumnId)?.Name ?? string.Empty},
                        { "ForeignKeyReferenceTableName", x => Global.Get<IDevData>().GetTableById(((ForeignKeyInfo)x).TableId)?.Name ?? string.Empty},
                        { "ForeignKeyReferenceColumnName", x => allColumns.FirstOrDefault(y => y.Id == ((ForeignKeyInfo)x).ReferenceColumnId)?.Name ?? string.Empty},
                    },
                    foreignKeys.ConvertAll<object>(y => y)),
                // auto increment column
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "AutoIncColumnName", x => ((ColumnInfo)x).Name },
                        { "AutoIncColumnDescription", x => ((ColumnInfo)x).Description },
                        { "AutoIncColumnDbType", x => ((ColumnInfo)x).GetDbType(task.DatabaseType) },
                        { "AutoIncColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? "default" : string.Empty},
                        { "AutoIncColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ((ColumnInfo)x).GetDbDefaultValue() : string.Empty },
                        { "AutoIncColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : "not null" },
                        { "AutoIncColumnProgramType", x => ((ColumnInfo)x).GetProgramType(task.ProgramLanguage, task.IsUsingString) },
                        { "AutoIncColumnHungarianPrefix", x => ((ColumnInfo)x).GetHungarianPrefix() },
                    },
                    null == autoIncColumn ? new List<object>() : [ autoIncColumn ]),
                // default values
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "DefaultValue", x => x + string.Empty },
                    },
                    tableInfo.GetDbDefaultValues().ConvertAll<object>(y => y)),
                ],
            task);
    }

    // API level template
    private static string GenFile_Api(string templateText, ApiInfo apiInfo, GenTask? task)
    {
        return GenFile_Template(templateText,
            new Dictionary<string, string>()
            {
                { "ApiName", apiInfo.Name },
                { "ApiDescription", apiInfo.Description }
            },
            [
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "InputParamSetIndex", x => apiInfo.InputParamSets.IndexOf((ApiParamSet)x) + 1 + "" },
                        { "InputParamSetRepeated0", x => ((ApiParamSet)x).IsRepeated ? (RpcType.Thrift == task!.RpcType ? "list<" : "repeated ") : string.Empty},
                        { "InputParamSetRepeated1", x => ((ApiParamSet)x).IsRepeated ? (RpcType.Thrift == task!.RpcType ? ">" : "") : string.Empty}
                    },
                    apiInfo.InputParamSets.ConvertAll<object>(y => y)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "OutputParamSetIndex", x => apiInfo.OutputParamSets.IndexOf((ApiParamSet)x) + 1 + "" },
                        { "OutputParamSetRepeated0", x => ((ApiParamSet)x).IsRepeated ? (RpcType.Thrift == task!.RpcType ? "list<" : "repeated ") : string.Empty},
                        { "OutputParamSetRepeated1", x => ((ApiParamSet)x).IsRepeated ? (RpcType.Thrift == task!.RpcType ? ">" : "") : string.Empty}
                    },
                    apiInfo.OutputParamSets.ConvertAll<object>(y => y))
            ],
            task);
    }

    // API param set level template
    private static string GenFile_ApiParamSet(string templateText, ApiParamSet paramSet, GenTask? task)
    {
        List<ColumnInfo> columns = paramSet.ColumnIdList.Select(x => Global.Get<IDevData>().Columns.First(y => y.Id == x)).ToList();
        
        return GenFile_Template(templateText,
            new Dictionary<string, string>()
            {
                { "ApiName", _currentApi?.Name + "" },
                { "ParamSetType", paramSet.Type.ToString() },
                { "ParamSetIndex", ApiParamSetType.Input == paramSet.Type ? _currentApi?.InputParamSets.IndexOf(paramSet) + 1 + "" : _currentApi?.OutputParamSets.IndexOf(paramSet) + 1 + "" }
            },
            null == task ? [] :
                [
                    Tuple.Create(
                        new Dictionary<string, Func<object, string>>
                        {
                            { "ParamIndex", x => columns.IndexOf((ColumnInfo)x) + 1 + "" },
                            { "ParamName", x => ((ColumnInfo)x).Name },
                            { "ParamRpcType", x => ((ColumnInfo)x).GetRpcType(task.RpcType) },
                        },
                        columns.ConvertAll<object>(y => y)),
                ],
            task);
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
                while (StringHelper.ContainsMacro(line, key, out _, out _, out _))
                {
                    line = StringHelper.ReplaceMacro(line, key, globalMacros[key]);
                }
            }
            string outputLine = line;

            // replace repeated micros
            Tuple<Dictionary<string, Func<object, string>>, List<object>>? repeatedMacro = null;
            foreach (Tuple<Dictionary<string, Func<object, string>>, List<object>> macro in repeatedMacros)
            {
                foreach (string macroKey in macro.Item1.Keys)
                {
                    if (StringHelper.ContainsMacro(line, macroKey, out _, out _, out _))
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
                        while (StringHelper.ContainsMacro(lineCopy, macroKey, out _, out _, out _))
                        {
                            lineCopy = StringHelper.ReplaceMacro(lineCopy, macroKey, macroValue);
                        }
                    }
                    outputLine += lineCopy + Environment.NewLine;
                }
            }
            
            // replace special micros
            // TEMPLATE
            if (null != task)
            {
                if (StringHelper.ContainsMacro(outputLine, "TEMPLATE", out int startIndex, out int endIndex, out string remark))
                {
                    string[] arr = remark.Split(",");
                    if (arr.Length == 2)
                    {
                        string subTemplatePath = Path.Combine(task.TemplateDir, arr[0]);
                        string subTemplateText = File.ReadAllText(subTemplatePath);
                        RecursionLevel recursionLevel = Enum.Parse<RecursionLevel>(arr[1]);
                        string subOutputContent = GenFile_SubTemplate(subTemplateText, recursionLevel, task);
                        outputLine = StringHelper.ReplaceRange(outputLine, startIndex, endIndex, subOutputContent);
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
            while (StringHelper.ContainsMacro(ret, "BACKSPACE", out int startIndex, out int endIndex, out _)) 
            {
                ret = StringHelper.ReplaceRange(ret, startIndex, endIndex, string.Empty);
                if (startIndex <= 0) { continue; }
                
                if (ret[..startIndex].EndsWith(Environment.NewLine))
                {
                    ret = StringHelper.ReplaceRange(ret, startIndex - Environment.NewLine.Length, startIndex - 1, string.Empty);
                }
                else
                {
                    ret = StringHelper.ReplaceRange(ret, startIndex - 1, startIndex - 1, string.Empty);
                }
            }
        }
        // CONDITIONAL_SPACE
        {
            while (StringHelper.ContainsMacro(ret, "CONDITIONAL_SPACE", out int startIndex, out int endIndex, out _)) 
            {
                ret = StringHelper.ReplaceRange(ret, startIndex, endIndex, string.Empty);
                if (startIndex <= 0 || startIndex >= ret.Length - 1) { continue; }
                
                while (startIndex > 0 && startIndex - 1 < ret.Length && ret[startIndex - 1] == ' ')
                {
                    ret = StringHelper.ReplaceRange(ret, startIndex - 1, startIndex - 1, string.Empty);
                    startIndex--;
                }
                while (startIndex < ret.Length && ret[startIndex] == ' ')
                {
                    ret = StringHelper.ReplaceRange(ret, startIndex, startIndex, string.Empty);
                }
                
                ret = ret.Insert(startIndex, " ");
            }
        }
        return ret;
    }
    
    private static string GenFile_SubTemplate(string templateText, RecursionLevel recursionLevel, GenTask task)
    {
        switch (recursionLevel)
        {
            case RecursionLevel.Database:
                return null != _currentDatabase ? GenFile_Database(templateText, _currentDatabase, task) : string.Empty;
            case RecursionLevel.Table:
            {
                if (null == _currentDatabase) { throw new ArgumentException("Null database info when recurse tables"); }
                StringBuilder sb = new();
                List<TableInfo> tables = _allTables[_currentDatabase];
                tables = TableListSorter.Sort(tables);
                foreach (TableInfo tableInfo in tables)
                {
                    _currentTable = tableInfo;
                    sb.Append(GenFile_Table(templateText, _currentTable, task));
                }
                return sb.ToString();
            }
            case RecursionLevel.ApiParamSet:
            {
                if (null == _currentApi) { throw new ArgumentException("Null API info when recurse API parameter sets"); }
                StringBuilder sb = new();
                foreach (ApiParamSet apiParamSet in _currentApi.InputParamSets)
                {
                    _currentApiParamSet = apiParamSet;
                    sb.Append(GenFile_ApiParamSet(templateText, _currentApiParamSet, task));
                }
                foreach (ApiParamSet apiParamSet in _currentApi.OutputParamSets)
                {
                    _currentApiParamSet = apiParamSet;
                    sb.Append(GenFile_ApiParamSet(templateText, _currentApiParamSet, task));
                }
                return sb.ToString();
            }
            default:
                throw new ArgumentException("Invalid recursion level");
        }
    }

    
    #region Pointers
    private static Dictionary<DatabaseInfo, List<TableInfo>> _allTables = [];
    private static DatabaseInfo? _currentDatabase;
    private static TableInfo? _currentTable;
    
    private static List<ApiInfo> _allApis = [];
    private static ApiInfo? _currentApi;
    private static ApiParamSet? _currentApiParamSet;
    #endregion
}