using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Framework.Common;
using Plugin.AppEnv;
using Plugin.DevData;

namespace UniClient;

public class CodeGenerator
{
    public static void GenerateTableDocument(string dir)
    {
        
    }
    
    public static void GenerateTableScripts(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        // initialize script
        string initializeScriptDir = Path.Combine(dir, "initialize");
        if (Directory.Exists(initializeScriptDir))
        {
            Directory.Delete(initializeScriptDir, true);
        }
        Directory.CreateDirectory(initializeScriptDir);
        
        foreach (string databaseName in Global.Get<IDevData>().GetAllTables().Keys)
        {
            List<TableInfo> tables = Global.Get<IDevData>().GetAllTables()[databaseName];
            string filePath = Path.Combine(initializeScriptDir, $"{databaseName}.sql");
            string databaseTemplatePath = string.Empty;
            string tableTemplatePath = string.Empty;
            if (Global.Get<IUserSetting>().DatabaseType == DatabaseTypeConst.MySQL)
            {
                databaseTemplatePath = Path.Combine(Global.Get<IAppEnv>().AppDir, "templates", "sys", "mysql_db_initialize.sql.template");
                tableTemplatePath = Path.Combine(Global.Get<IAppEnv>().AppDir, "templates", "sys", "mysql_table_initialize.sql.template");
            }
            string outputContent = GenFile(databaseTemplatePath,
                new Dictionary<string, string>
                {
                    { "DatabaseName", databaseName }
                },
                [
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "TableCreateScript", x => GenTableFile(tableTemplatePath, (TableInfo)x) },
                    },
                    tables.ConvertAll<object>(x => x))
                ]);
            StreamWriter sw = new(filePath, false, Encoding.UTF8);
            sw.Write(outputContent);
            sw.Close();
        }
    }
    
    public static void GenerateUserDefined(string dir)
    {
        
    }
    
    private static string GenTableFile(string templatePath, TableInfo tableInfo)
    {
        List<IndexInfo> primaryKeyList = tableInfo.IndexList.Where(x => IndexType.Primary == x.Type).ToList();
        string primaryKeys = "";
        IndexInfo? primaryKeyInfo = null;
        if (tableInfo.IndexList.Any(x => IndexType.Primary == x.Type))
        {
            primaryKeyInfo = tableInfo.IndexList.First(x => IndexType.Primary == x.Type);
            foreach (int columnId in primaryKeyInfo.ColumnIdList)
            {
                ColumnInfo columnInfo = Global.Get<IDevData>().Columns.First(x => x.Id == columnId);
                primaryKeys += (primaryKeys.Length > 0 ? "," : "") + columnInfo.Name;
            }
        }

        return GenFile(templatePath,
            new Dictionary<string, string>
            {
                { "DatabaseName", Global.Get<IDevData>().GetDatabaseNameByTableId(tableInfo.Id) },
                { "TableName", tableInfo.Name },
                { "TableDescription", tableInfo.Description },
                { "PrimaryKeys", primaryKeys },
                { "PrimaryKeyColumnCount", primaryKeyList.Count.ToString() }
            },
            [
                // columns related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ColumnName", x => ((ColumnInfo)x).Name },
                        { "ColumnDescription", x => ((ColumnInfo)x).Description },
                        { "ColumnDbType", x => ToDbType((ColumnInfo)x) },
                        { "ColumnDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? "default " : string.Empty},
                        { "ColumnDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "ColumnNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "ColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," }
                    },
                    tableInfo.ColumnIdList.ToList().ConvertAll<object>(x => Global.Get<IDevData>().Columns.First(y => y.Id == x))),
                // primary key related
                Tuple.Create
                (
                    null == primaryKeyInfo ? [] :
                    new Dictionary<string, Func<object, string>>
                    {
                        { "PrimaryKeyName", x => ((IndexInfo)x).Name },
                        { "PrimaryKeyColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => Global.Get<IDevData>().Columns.First(z => y == z.Id).Name)) },
                    },
                    primaryKeyList.ConvertAll<object>(x => x)),
                // indexes related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>()
                    {
                        { "IndexTypeString", x => IndexType.Unique == ((IndexInfo)x).Type ? "unique " : "" },
                        { "IndexName", x => ((IndexInfo)x).Name },
                        { "IndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => Global.Get<IDevData>().Columns.First(z => y == z.Id).Name)) },
                    },
                    tableInfo.IndexList.ToList().ConvertAll<object>(x => x.Type is IndexType.Unique or IndexType.Index)),
                ]);
    }

    private static string ToDbType(ColumnInfo columnInfo)
    {
        if (Global.Get<IUserSetting>().DatabaseType == DatabaseTypeConst.MySQL)
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
        if (Global.Get<IUserSetting>().DatabaseType == DatabaseTypeConst.Oracle)
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
        if (Global.Get<IUserSetting>().DatabaseType == DatabaseTypeConst.SQLServer)
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
    
    /// <summary>
    /// General method to generate a file
    /// </summary>
    /// <param name="templatePath"></param>
    /// <param name="globalMacros"></param>
    /// <param name="repeatedMacros"></param>
    /// <returns>file content</returns>
    private static string GenFile(string templatePath, Dictionary<string, string> globalMacros, List<Tuple<Dictionary<string, Func<object, string>>, List<object>>> repeatedMacros)
    {
        StringBuilder outputContent = new();
        StreamReader templateReader = new(templatePath, Encoding.UTF8);
        
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
                if (ContainsMacro(line, key, out int startIndex, out int endIndex, out NameStyle nameStyle))
                {
                    line = ReplaceRange(line, startIndex, endIndex, StringHelper.ToNameStyle(globalMacros[key], nameStyle));
                }
            }
            string outputLine = line;

            // replace repeated micros
            foreach (Tuple<Dictionary<string, Func<object, string>>, List<object>> macro in repeatedMacros)
            {
                bool isContainMacros = false;
                foreach (string macroKey in macro.Item1.Keys)
                {
                    if (ContainsMacro(line, macroKey, out _, out _, out _))
                    {
                        isContainMacros = true;
                        break;
                    }
                }
                
                if (!isContainMacros)
                {
                    continue;
                }
                
                IList<object> columns = macro.Item2;
                outputLine = "";
                foreach (object column in columns)
                {
                    string lineCopy = new(line);
                    foreach (string macroKey in macro.Item1.Keys)
                    {
                        if (ContainsMacro(lineCopy, macroKey, out int startIndex, out int endIndex,
                                out NameStyle nameStyle))
                        {
                            string macroValue = macro.Item1[macroKey](column);
                            lineCopy = ReplaceRange(lineCopy, startIndex, endIndex, StringHelper.ToNameStyle(macroValue, nameStyle));
                        }
                    }
                    outputLine += lineCopy + Environment.NewLine;
                }
            }
            
            // replace special micros
            {
                if (ContainsMacro(outputLine, "BACKSPACE", out int startIndex, out int endIndex, out _))
                {
                    ReplaceRange(outputLine, startIndex, endIndex, string.Empty);
                    if (startIndex > 0)
                    {
                        ReplaceRange(outputLine, startIndex - 1, startIndex - 1, string.Empty);
                    }
                }
            }
            
            if (!outputLine.EndsWith(Environment.NewLine))
            {
                outputLine += Environment.NewLine;
            }
            
            outputContent.Append(outputLine);
            line = templateReader.ReadLine();
        }

        templateReader.Close();
        return outputContent.ToString();
    }

    private static bool ContainsMacro(string line, string macro, out int startIndex, out int endIndex, out NameStyle nameStyle)
    {
        startIndex = -1;
        endIndex = -1;
        nameStyle = NameStyle.Original;
        
        string pattern = $@"\$\{{{Regex.Escape(macro)}(?:\((?<arg>[^\)]*)\))?\}}";

        var match = Regex.Match(line, pattern);

        if (match.Success)
        {
            startIndex = match.Index;
            endIndex = match.Index + match.Length - 1;
            nameStyle = match.Groups["arg"].Success ? Enum.Parse<NameStyle>(match.Groups["arg"].Value) : NameStyle.Original;
            return true;
        }

        return false;
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
    
}