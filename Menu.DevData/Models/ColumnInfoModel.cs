using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ColumnInfoModel : UniModel
{
    #region Constructors

    public ColumnInfoModel()
    {
        Group = "Default";
        Name = string.Empty;
        Description = string.Empty;
        DataDict = string.Empty;
        Remark = string.Empty;
    }
    
    public ColumnInfoModel(ColumnInfo columnInfo)
    {
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
    
    

    [ObservableProperty] private string _group;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private ColumnType _type;
    [ObservableProperty] private int _length;
    [ObservableProperty] private bool _lengthEnabled;
    [ObservableProperty] private int _scale;
    [ObservableProperty] private bool _scaleEnabled;
    [ObservableProperty] private string _dataDict;
    [ObservableProperty] private string _remark;
}