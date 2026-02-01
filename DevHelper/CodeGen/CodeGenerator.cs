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

        if (RecursionLevel.Database == task.RecursionLevel)
        {
            foreach (string databaseName in Global.Get<IDevData>().GetAllTables().Keys)
            {
                _currentDatabase = databaseName;
                string outputFileName = task.OutputFile;
                if (ContainsMacro(outputFileName, "DatabaseName", out int startIndex, out int endIndex, out string remark))
                {
                    outputFileName = ReplaceRange(outputFileName, startIndex, endIndex, ToNameStyle(_currentDatabase, remark));
                }

                string outputFilePath = Path.Combine(task.OutputDir, outputFileName);
                StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                string outputContent = GenFile_Database(Path.Combine(task.TemplateDir, task.TemplateFile), _currentDatabase);
                sw.Write(outputContent);
                sw.Close();
            }
        }
        else if (RecursionLevel.Table == task.RecursionLevel)
        {
            foreach (string databaseName in Global.Get<IDevData>().GetAllTables().Keys)
            {
                _currentDatabase = databaseName;
                string outputDir = Path.Combine(task.OutputDir, _currentDatabase);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                
                foreach (TableInfo tableInfo in Global.Get<IDevData>().GetAllTables()[databaseName])
                {
                    string outputFileName = task.OutputFile;
                    if (ContainsMacro(outputFileName, "TableName", out int startIndex, out int endIndex, out string remark))
                    {
                        outputFileName = ReplaceRange(outputFileName, startIndex, endIndex, ToNameStyle(_currentDatabase, remark));
                    }
                    string outputFilePath = Path.Combine(outputDir, outputFileName);
                    
                    StreamWriter sw = new(outputFilePath, false, Encoding.UTF8);
                    string outputContent = GenFile_Table(Path.Combine(task.TemplateDir, task.TemplateFile), tableInfo);
                    sw.Write(outputContent);
                    sw.Close();
                }
            }
        }
    }
    
    // database level template
    private static string GenFile_Database(string templatePath, string databaseName)
    {
        return GenFileByTemplate(templatePath,
            new Dictionary<string, string>
            {
                { "DatabaseName", databaseName }
            },
            []);
    }
    
    // table level template
    private static string GenFile_Table(string templatePath, TableInfo tableInfo)
    {
        IndexInfo? primaryKeyInfo = tableInfo.IndexList.FirstOrDefault(x => IndexType.Primary == x.Type);
        
        List<ColumnInfo> columns = tableInfo.ColumnIdList.Select(x => Global.Get<IDevData>().Columns.First(y => y.Id == x)).ToList();
        List<ColumnInfo> primaryKeyColumns = columns.Where(x => primaryKeyInfo?.ColumnIdList.Contains(x.Id) ?? false).ToList();
        
        return GenFileByTemplate(templatePath,
            new Dictionary<string, string>
            {
                { "DatabaseName", Global.Get<IDevData>().GetDatabaseNameByTableId(tableInfo.Id) },
                { "TableName", tableInfo.Name },
                { "TableDescription", tableInfo.Description },
                { "PrimaryKeyColumnCount", primaryKeyColumns.Count + "" },
                { "PrimaryKeyColumns", string.Join(", ", primaryKeyColumns.Select(x => x.Name)) },
                { "PrimaryKeyColumnsWithBackQuota", string.Join(", ", primaryKeyColumns.Select(x => $"`{x.Name}`")) },
            },
            [
                // columns related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>
                    {
                        { "ColumnName", x => ((ColumnInfo)x).Name },
                        { "ColumnDescription", x => ((ColumnInfo)x).Description },
                        { "ColumnDbType", x => ToDbType((ColumnInfo)x) },
                        { "ColumnDefaultString", x => ((ColumnInfo)x).HasDefaultValue ? " default " : string.Empty},
                        { "ColumnDefaultValue", x => ((ColumnInfo)x).HasDefaultValue ? ToDbDefaultValue((ColumnInfo)x) : string.Empty },
                        { "ColumnNullableFlag", x => ((ColumnInfo)x).IsNullable ? "" : " not null" },
                        { "ColumnComma", x => ((ColumnInfo)x).Id == tableInfo.ColumnIdList[^1] ? "" : "," }
                    },
                    columns.ConvertAll<object>(x => x)),
                // primary key related
                Tuple.Create
                (
                    new Dictionary<string, Func<object, string>>
                    {
                        { "PrimaryKeyName", x => ((IndexInfo)x).Name },
                        { "PrimaryKeyColumnName", x => ((ColumnInfo)x).Name },
                        { "PrimaryKeyColumnIndex", x => primaryKeyColumns.IndexOf((ColumnInfo)x) + "" }
                    },
                    primaryKeyColumns.ConvertAll<object>(x => x)),
                // indexes related
                Tuple.Create(
                    new Dictionary<string, Func<object, string>>()
                    {
                        { "IndexTypeString", x => IndexType.Unique == ((IndexInfo)x).Type ? "unique " : "" },
                        { "IndexName", x => ((IndexInfo)x).Name },
                        { "IndexColumns", x => string.Join(", ", ((IndexInfo)x).ColumnIdList.Select(y => Global.Get<IDevData>().Columns.First(z => y == z.Id).Name)) },
                    },
                    tableInfo.IndexList.Where(x => x.Type is IndexType.Unique or IndexType.Index).ToList().ConvertAll<object>(y => y)),
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
                if (ContainsMacro(line, key, out int startIndex, out int endIndex, out string remark))
                {
                    line = ReplaceRange(line, startIndex, endIndex, ToNameStyle(globalMacros[key], remark));
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
                                out string remark))
                        {
                            string macroValue = macro.Item1[macroKey](column);
                            lineCopy = ReplaceRange(lineCopy, startIndex, endIndex, ToNameStyle(macroValue, remark));
                        }
                    }
                    outputLine += lineCopy + Environment.NewLine;
                }
            }
            
            // replace special micros
            // BACKSPACE
            {
                if (ContainsMacro(outputLine, "BACKSPACE", out int startIndex, out int endIndex, out _)) 
                {
                    outputLine = ReplaceRange(outputLine, startIndex, endIndex, string.Empty);
                    if (startIndex > 0)
                    {
                        outputLine = ReplaceRange(outputLine, startIndex - 1, startIndex - 1, string.Empty);
                    }
                }
            }
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
        return outputContent.ToString();
    }

    private static bool ContainsMacro(string line, string macro, out int startIndex, out int endIndex, out string remark)
    {
        startIndex = -1;
        endIndex = -1;
        remark = string.Empty;
        
        string pattern = $@"\$\{{{Regex.Escape(macro)}(?:\((?<arg>[^\)]*)\))?\}}";

        var match = Regex.Match(line, pattern);

        if (match.Success)
        {
            startIndex = match.Index;
            endIndex = match.Index + match.Length - 1;
            remark = match.Groups["arg"].Success ? match.Groups["arg"].Value : string.Empty;
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

    private static string GenSubTemplate(string templatePath, RecursionLevel recursionLevel)
    {
        if (RecursionLevel.Database == recursionLevel)
        {
            StringBuilder sb = new();
            foreach (string databaseName in Global.Get<IDevData>().GetAllTables().Keys)
            {
                _currentDatabase = databaseName;
                sb.Append(GenFile_Database(templatePath, databaseName));
            }
            return sb.ToString();
        }
        
        if (RecursionLevel.Table == recursionLevel)
        {
            StringBuilder sb = new();
            List<TableInfo> tables = Global.Get<IDevData>().GetAllTables()[_currentDatabase];
            foreach (TableInfo tableInfo in tables)
            {
                sb.Append(GenFile_Table(templatePath, tableInfo));
            }

            return sb.ToString();
        }

        throw new ArgumentOutOfRangeException();
    }

    #region Helper Functions

    private static string ToNameStyle(string input, string nameStyle)
    {
        _ = Enum.TryParse(nameStyle, out NameStyle ns);
        return StringHelper.ToNameStyle(input, ns);
    }

    #endregion

    #region Pointers
    private static string _currentDatabase = string.Empty;
    #endregion
}