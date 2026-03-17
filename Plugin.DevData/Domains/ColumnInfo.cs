using Framework.Common;
using Plugin.AppEnv;

namespace Plugin.DevData;

public class ColumnInfo
{
    public const string DefaultColumnGroup = "Default";
    public const int ArchiveDateColumnId = -1;
    
    public string Group { get; set; } = DefaultColumnGroup;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; }  = string.Empty;
    public ColumnType Type { get; set; }
    public int Length { get; set; }
    public int Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool HasDefaultValue { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public string DataDict { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;

    public static ColumnInfo GetArchiveDateColumn()
    {
        return new ColumnInfo
        {
            Id = ArchiveDateColumnId,
            Name = Global.Get<IUserSetting>().ArchiveDateColumnName,
            Type = ColumnType.Int32,
            IsNullable = false,
        };
    }
    
    public string GetDbType(DatabaseType databaseType)
    {
        if (DatabaseType.MySQL == databaseType)
        {
            return Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "bigint",
                ColumnType.Number =>
                    $"decimal({Length}{(Scale > 0 ? $", {Scale}" : string.Empty)})",
                ColumnType.Char => $"char({Length})",
                ColumnType.Varchar => $"varchar({Length})",
                ColumnType.Bool => "tinyint",
                ColumnType.Datetime => "datetime",
                _ => string.Empty
            };
        }
        if (DatabaseType.Oracle == databaseType)
        {
            return Type switch
            {
                ColumnType.Int32 => "number(10)",
                ColumnType.Int64 => "number(19)",
                ColumnType.Number =>
                    $"number({Length}{(Scale > 0 ? $", {Scale}" : string.Empty)})",
                ColumnType.Char => $"char({Length})",
                ColumnType.Varchar => $"varchar2({Length})",
                ColumnType.Bool => "number(1)",
                ColumnType.Datetime => "date",
                _ => string.Empty
            };
        }
        if (DatabaseType.SQLServer == databaseType)
        {
            return Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "bigint",
                ColumnType.Number =>
                    $"decimal({Length}{(Scale > 0 ? $", {Scale}" : string.Empty)})",
                ColumnType.Char => $"char({Length})",
                ColumnType.Varchar => $"varchar({Length})",
                ColumnType.Bool => "bit",
                ColumnType.Datetime => "datetime",
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    public string GetDbDefaultValue()
    {
        return Type switch
        {
            ColumnType.Int32 or ColumnType.Int64 or ColumnType.Number => DefaultValue,
            ColumnType.Char or ColumnType.Varchar or ColumnType.Datetime => $"'{DefaultValue}'",
            ColumnType.Bool => DefaultValue.ToLower() switch
            {
                "false" => "0",
                "true" => "1",
                _ => DefaultValue
            },
            _ => string.Empty
        };
    }

    public string GetProgramType(ProgramLanguage programLanguage, bool isUsingString)
    {
        return programLanguage switch
        {
            ProgramLanguage.Cpp => Type switch
            {
                ColumnType.Int32 => "int32_t",
                ColumnType.Int64 => "int64_t",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar => isUsingString
                    ? "std::string"
                    : $"char{Length * Scale + 1}",
                ColumnType.Bool => "bool",
                ColumnType.Datetime => "std::chrono",
                _ => string.Empty
            },
            ProgramLanguage.CSharp => Type switch
            {
                ColumnType.Int32 => "int",
                ColumnType.Int64 => "long",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar => isUsingString
                    ? "string"
                    : $"byte[{Length * Scale + 1}]",
                ColumnType.Bool => "bool",
                ColumnType.Datetime => "DateTime",
                _ => string.Empty
            },
            ProgramLanguage.Java => Type switch
            {
                ColumnType.Int32 => "Int",
                ColumnType.Int64 => "Long",
                ColumnType.Number => "Double",
                ColumnType.Char or ColumnType.Varchar => isUsingString
                    ? "String"
                    : $"Byte[{Length * Scale + 1}]",
                ColumnType.Bool => "Boolean",
                ColumnType.Datetime => "LocalDateTime",
                _ => string.Empty
            },
            _ => string.Empty
        };
    }

    public string GetHungarianPrefix()
    {
        return Type switch
        {
            ColumnType.Int32 => "n",
            ColumnType.Int64 => "l",
            ColumnType.Number => "d",
            ColumnType.Char or ColumnType.Varchar => "sz",
            ColumnType.Bool => "b",
            ColumnType.Datetime => "dt",
            _ => string.Empty
        };
    }

    public string GetRpcType(RpcType rpcType)
    {
        return rpcType switch
        {
            RpcType.Thrift => Type switch
            {
                ColumnType.Int32 => "i32",
                ColumnType.Int64 => "i64",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar or ColumnType.Datetime => "string",
                ColumnType.Bool => "bool",
                _ => string.Empty
            },
            RpcType.Grpc => Type switch
            {
                ColumnType.Int32 => "int32",
                ColumnType.Int64 => "int64",
                ColumnType.Number => "double",
                ColumnType.Char or ColumnType.Varchar or ColumnType.Datetime => "string",
                ColumnType.Bool => "bool",
                _ => string.Empty
            },
            _ => string.Empty
        };
    }
}