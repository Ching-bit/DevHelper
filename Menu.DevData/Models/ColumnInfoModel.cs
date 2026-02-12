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
        Name = string.Empty;
        Description = string.Empty;
        DefaultValue = string.Empty;
        DataDict = string.Empty;
        Remark = string.Empty;
    }
    
    public ColumnInfoModel(ColumnInfo columnInfo)
    {
        ModifyStatus = ModifyStatus.Normal;
        
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
    
    [ObservableProperty] private string _group;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private ColumnType _type;
    [ObservableProperty] private int _length;
    [ObservableProperty] private int _scale;
    [ObservableProperty] private bool _isNullable;
    [ObservableProperty] private bool _hasDefaultValue;
    [ObservableProperty] private string _defaultValue;
    [ObservableProperty] private string _dataDict;
    [ObservableProperty] private string _remark;
    
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