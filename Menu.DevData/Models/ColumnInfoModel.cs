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
        
        Group = ColumnInfo.DEFAULT_COLUMN_NAME;
        Name = string.Empty;
        Description = string.Empty;
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
                    Length = Math.Min(Length, 38);                  // Number(m, n), m <= 38
                    Scale = Math.Min(Scale, Math.Min(Length, 30));  // n <= 30, n <= m
                }
                else if (ColumnType.Char == Type)
                {
                    Length = Math.Min(Length, 255);     // Char(m), m <= 255
                }
                else if (ColumnType.String == Type)
                {
                    Length = Math.Min(Length, 4000);    // String(m), m <= 4000
                }

                break;
            }
            case nameof(Scale):
            {
                if (ColumnType.Number == Type)
                {
                    Scale = Math.Min(Scale, Math.Min(Length, 30));   // Number(m, n), n <= 30, n <= m
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
                    Length = 255;
                    Scale = 0;
                }
                else if (ColumnType.String == Type)
                {
                    Length = 4000;
                    Scale = 0;
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
            DataDict = DataDict,
            Remark = Remark
        };

        switch (columnInfo.Type)
        {
            case ColumnType.Number:
                // do nothing
                break;
            case ColumnType.Char or ColumnType.String:
                columnInfo.Scale = 0;
                break;
            default:
                columnInfo.Length = 0;
                columnInfo.Scale = 0;
                break;
        }

        return columnInfo;
    }
    
    public override string ToString()
    {
        return $"{Id} {Name}";
    }
    
}