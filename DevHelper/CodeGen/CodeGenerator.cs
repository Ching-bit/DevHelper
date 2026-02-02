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
    public static void GenFile(GenTask task)
    {
        if (Directory.Exists(task.OutputDir))
        {
            Directory.Delete(task.OutputDir, true);
        }
        Directory.CreateDirectory(task.OutputDir);

        switch (task.RecursionLevel)
        {
            case RecursionLevel.Database:
            {
                foreach (DatabaseInfo database in Global.Get<IDevData>().GetAllTables().Keys)
                {
                    _currentDatabase = database;
                    string outputFileName = task.OutputFile;
                    outputFileName = ReplaceMacro(outputFileName, "DatabaseName", _currentDatabase.Name);

                    string outputFilePath = Path.Combine(task.OutputDir, outputFileName);
                    StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                    string outputContent = GenFile_Database(Path.Combine(task.TemplateDir, task.TemplateFile), _currentDatabase);
                    sw.Write(outputContent);
                    sw.Close();
                }

                break;
            }
            case RecursionLevel.Table:
            {
                foreach (DatabaseInfo database in Global.Get<IDevData>().GetAllTables().Keys)
                {
                    _currentDatabase = database;
                    string outputDir = Path.Combine(task.OutputDir, database.Name);
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                
                    foreach (TableInfo tableInfo in Global.Get<IDevData>().GetAllTables()[database])
                    {
                        string outputFileName = task.OutputFile;
                        outputFileName = ReplaceMacro(outputFileName, "TableName", tableInfo.Name);
                        string outputFilePath = Path.Combine(outputDir, outputFileName);
                    
                        StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                        string outputContent = GenFile_Table(Path.Combine(task.TemplateDir, task.TemplateFile), tableInfo);
                        sw.Write(outputContent);
                        sw.Close();
                    }
                }

                break;
            }
        }
    }
    
    // database level template
    private static string GenFile_Database(string templatePath, DatabaseInfo databaseInfo)
    {
        return GenFileByTemplate(templatePath,
            new Dictionary<string, string>
            {
                { "DatabaseName", databaseInfo.Name },
                { "DatabaseDescription", databaseInfo.Description }
            },
            []);
    }
    
    // table level template
    private static string GenFile_Table(string templatePath, TableInfo tableInfo)
    {
        IndexInfo? primaryKeyInfo = tableInfo.IndexList.FirstOrDefault(x => IndexType.Primary == x.Type);
        
        List<ColumnInfo> columns = tableInfo.ColumnIdList.Select(x => Global.Get<IDevData>().Columns.First(y => y.Id == x)).ToList();
        List<ColumnInfo> primaryKeyColumns = columns.Where(x => primaryKeyInfo?.ColumnIdList.Contains(x.Id) ?? false).ToList();
        List<ColumnInfo> generalColumns = columns.Except(primaryKeyColumns).ToList();
        List<IndexInfo> indexes = tableInfo.IndexList.Where(x => x.Type is IndexType.Unique or IndexType.Index).ToList();
        
        return GenFileByTemplate(templatePath,
            new Dictionary<string, string>
            {
                { "DatabaseName", Global.Get<IDevData>().GetDatabaseInfoByTableId(tableInfo.Id)?.Name ?? string.Empty },
                { "TableName", tableInfo.Name },
                { "TableDescription", tableInfo.Description },
            },
            [
                // columns related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ColumnName", x => ((ColumnInfo)x).Name },
                        { "ColumnDescription", x => ((ColumnInfo)x).Description },
                        { "ColumnDbType", x => ToDbType((ColumnInfo)x) },
                        { "ColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "ColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "ColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "ColumnJavaType", x => ToJavaType((ColumnInfo)x) },
                        { "ColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," }
                    },
                    columns.ConvertAll<object>(x => x)),
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "GeneralColumnName", x => ((ColumnInfo)x).Name },
                        { "GeneralColumnDescription", x => ((ColumnInfo)x).Description },
                        { "GeneralColumnDbType", x => ToDbType((ColumnInfo)x) },
                        { "GeneralColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "GeneralColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "GeneralColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "GeneralColumnJavaType", x => ToJavaType((ColumnInfo)x) },
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
                        { "PrimaryKeyColumnDbType", x => ToDbType((ColumnInfo)x) },
                        { "PrimaryKeyColumnDbDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "PrimaryKeyColumnDbDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "PrimaryKeyColumnDbNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "PrimaryKeyColumnJavaType", x => ToJavaType((ColumnInfo)x) },
                        { "PrimaryKeyColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," },
                        { "PrimaryKeyColumnIndex", x => primaryKeyColumns.IndexOf((ColumnInfo)x) + "" }
                    },
                    primaryKeyColumns.ConvertAll<object>(x => x)),
                // indexes related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>()
                    {
                        { "IndexType", x => ((IndexInfo)x).Type.ToString().ToLower() },
                        { "IndexTypeWithKey", x => IndexType.Unique == ((IndexInfo)x).Type ? "unique key" : "index" },
                        { "IndexName", x => ((IndexInfo)x).Name },
                        { "IndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => Global.Get<IDevData>().Columns.First(z => y == z.Id).Name)) },
                        { "IndexColumnsWithBackQuota", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => "`" + Global.Get<IDevData>().Columns.First(z => y == z.Id).Name + "`")) },
                    },
                    indexes.ConvertAll<object>(y => y)),
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
    
    private static string ToJavaType(ColumnInfo columnInfo)
    {
        return columnInfo.Type switch
        {
            ColumnType.Int32 => "Int",
            ColumnType.Int64 => "Long",
            ColumnType.Number => "Double",
            ColumnType.Char or ColumnType.Varchar => "String",
            ColumnType.Bool => "Boolean",
            ColumnType.Datetime => "java.time.LocalDateTime",
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
    private static string GenFileByTemplate(string templatePath, Dictionary<string, string> globalMacros, List<Tuple<Dictionary<string, Func<object, string>>, List<object>>> repeatedMacros)
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
                while (ContainsMacro(line, key, out _, out _, out _))
                {
                    line = ReplaceMacro(line, key, globalMacros[key]);
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
                        string macroValue = macro.Item1[macroKey](column);
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
            {
                if (ContainsMacro(outputLine, "TEMPLATE", out int startIndex, out int endIndex, out string remark))
                {
                    string[] arr = remark.Split(",");
                    if (arr.Length == 2)
                    {
                        string subTemplatePath = Path.Combine(Path.GetDirectoryName(templatePath)!, arr[0]);
                        RecursionLevel recursionLevel = Enum.Parse<RecursionLevel>(arr[1]);
                        string subOutputContent = GenSubTemplate(subTemplatePath, recursionLevel);
                        outputLine = ReplaceRange(outputLine, startIndex, endIndex, subOutputContent);
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
        string ret = outputContent.ToString();
        // replace special micros
        // BACKSPACE
        {
            while (ContainsMacro(ret, "BACKSPACE", out int startIndex, out int endIndex, out _)) 
            {
                ret = ReplaceRange(ret, startIndex, endIndex, string.Empty);
                if (startIndex > 0)
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

    private static string GenSubTemplate(string templatePath, RecursionLevel recursionLevel)
    {
        switch (recursionLevel)
        {
            case RecursionLevel.Database:
                return null != _currentDatabase ? GenFile_Database(templatePath, _currentDatabase) : string.Empty;
            case RecursionLevel.Table when null == _currentDatabase:
                throw new ArgumentException("Null database info when recurse tables");
            case RecursionLevel.Table:
            {
                StringBuilder sb = new();
                List<TableInfo> tables = Global.Get<IDevData>().GetAllTables()[_currentDatabase];
                foreach (TableInfo tableInfo in tables)
                {
                    sb.Append(GenFile_Table(templatePath, tableInfo));
                }

                return sb.ToString();
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    
    #region Pointers
    private static DatabaseInfo? _currentDatabase;
    #endregion
}