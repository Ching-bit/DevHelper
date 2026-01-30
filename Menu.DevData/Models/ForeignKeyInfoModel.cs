using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ForeignKeyInfoModel : UniModel
{
    #region Constructors
    public ForeignKeyInfoModel(string tableName)
    {
        TableName = tableName;
        Name = string.Empty;
    }
    
    public ForeignKeyInfoModel(ForeignKeyInfo foreignKeyInfo, List<ColumnInfoModel> columnList, string tableName) : this(tableName)
    {
        Column = columnList.FirstOrDefault(x => x.Id == foreignKeyInfo.ColumnId);

        TableInfo? tableInfo = Global.Get<IDevData>().GetTableById(foreignKeyInfo.TableId);
        if (null != tableInfo)
        {
            ReferenceTable = new TableInfoModel(tableInfo);
            ReferenceColumn = ReferenceTable.ColumnList.FirstOrDefault(x => x.Id == foreignKeyInfo.ReferenceColumnId);
        }
    }
    #endregion
    
    
    #region Properties
    [ObservableProperty] private ColumnInfoModel? _column;
    [ObservableProperty] private string _name;
    [ObservableProperty] private TableInfoModel? _referenceTable;
    [ObservableProperty] private ColumnInfoModel? _referenceColumn;
    
    [ObservableProperty] private ModifyStatus _modifyStatus;

    private string TableName { get; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Column))
        {
            Name = "FK_" + $"{TableName.ToUpper()}_{Column?.Name.ToUpper() ?? string.Empty}";
        }
    }

    #endregion
    

    public ForeignKeyInfo GetForeignKeyInfo()
    {
        return new ForeignKeyInfo()
        {
            ColumnId = Column?.Id ?? 0,
            Name = Name,
            TableId = ReferenceTable?.Id ?? 0,
            ReferenceColumnId = ReferenceColumn?.Id ?? 0
        };
    }

    public void CopyFrom(ForeignKeyInfoModel source)
    {
        Column = source.Column;
        Name = source.Name;
        ReferenceTable = source.ReferenceTable;
        ReferenceColumn = source.ReferenceColumn;
    }
}