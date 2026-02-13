using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ColumnInfoModel : UniModel
{
    #region Constructors
    public ColumnInfoModel()
    {
        ModifyStatus = ModifyStatus.Normal;
        Group = ColumnInfo.DefaultColumnGroup;
    }
    
    public ColumnInfoModel(ColumnInfo columnInfo)
    {
        ModifyStatus = ModifyStatus.Normal;
        CopyFromColumnInfo(columnInfo);
    }

    public void CopyFromColumnInfo(ColumnInfo columnInfo)
    {
        Group = columnInfo.Group;
        Id = columnInfo.Id;
        Name = columnInfo.Name;
        Description = columnInfo.Description;
        Type = columnInfo.Type;
        Length = columnInfo.Length;
        Scale = columnInfo.Scale;
        IsNullable = columnInfo.IsNullable;
        HasDefaultValue = columnInfo.HasDefaultValue;
        DefaultValue = columnInfo.DefaultValue;
        DataDict = columnInfo.DataDict;
        Remark = columnInfo.Remark;
    }
    #endregion


    #region Properties
    [ObservableProperty] private ModifyStatus _modifyStatus;
    [ObservableProperty] private bool _isPrimaryKey;
    
    [ObservableProperty] private string _group = string.Empty;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private ColumnType _type;
    [ObservableProperty] private int _length;
    [ObservableProperty] private int _scale;
    [ObservableProperty] private bool _isNullable;
    [ObservableProperty] private bool _hasDefaultValue;
    [ObservableProperty] private string _defaultValue = string.Empty;
    [ObservableProperty] private string _dataDict = string.Empty;
    [ObservableProperty] private string _remark = string.Empty;
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(Length):
            {
                if (ColumnType.Number == Type)
                {
                    Scale = Math.Min(Scale, Length);
                }

                break;
            }
            case nameof(Scale):
            {
                if (ColumnType.Number == Type)
                {
                    Scale = Math.Min(Scale, Length);
                }

                break;
            }
            case nameof(Type):
            {
                // init value
                if (ColumnType.Number == Type)
                {
                    Length = 38;
                    Scale = 0;
                }
                else if (ColumnType.Char == Type)
                {
                    Length = 16;
                    Scale = 1;
                }
                else if (ColumnType.Varchar == Type)
                {
                    Length = 255;
                    Scale = 1;
                }

                break;
            }
        }
    }
    #endregion
    

    public ColumnInfo GetColumnInfo()
    {
        ColumnInfo columnInfo = new()
        {
            Group = Group,
            Id = Id,
            Name = Name,
            Description = Description,
            Type = Type,
            Length = Length,
            Scale = Scale,
            IsNullable = IsNullable,
            HasDefaultValue = HasDefaultValue,
            DefaultValue = DefaultValue,
            DataDict = DataDict,
            Remark = Remark
        };

        switch (columnInfo.Type)
        {
            case ColumnType.Number or ColumnType.Char or ColumnType.Varchar:
                // do nothing
                break;
            default:
                columnInfo.Length = 0;
                columnInfo.Scale = 0;
                break;
        }

        return columnInfo;
    }

    public string GetTypeString()
    {
        return Type switch
        {
            ColumnType.Char or ColumnType.Varchar => $"{Type}({Length})",
            ColumnType.Number => $"{Type}({Length}, {Scale})",
            _ => Type.ToString()
        };
    }

    public bool IsSameType(ColumnInfoModel columnInfoModel)
    {
        return GetTypeString().Equals(columnInfoModel.GetTypeString());
    }
    
    public override string ToString()
    {
        return $"{Id}: {Name}{(string.IsNullOrEmpty(Description) ? string.Empty : $" ({Description})")}";
    }
    
}