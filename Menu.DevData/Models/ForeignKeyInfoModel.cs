using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ForeignKeyInfoModel : UniModel
{
    #region Constructors
    public ForeignKeyInfoModel()
    {
        
    }
    
    public ForeignKeyInfoModel(ForeignKeyInfo foreignKeyInfo, List<ColumnInfoModel> columnList)
    {
        Column = columnList.FirstOrDefault(x => x.Id == foreignKeyInfo.ColumnId);

        TableInfo? tableInfo = Global.Get<IDevData>().GetTableList()
            .FirstOrDefault(x => x.Name.Equals(foreignKeyInfo.TableName));
        if (null != tableInfo)
        {
            ReferenceTable = new TableInfoModel(tableInfo);
            ReferenceColumn = ReferenceTable.ColumnList.FirstOrDefault(x => x.Id == foreignKeyInfo.ReferenceColumnId);
        }
    }
    #endregion
    
    
    #region Properties
    [ObservableProperty] private ColumnInfoModel? _column;
    [ObservableProperty] private TableInfoModel? _referenceTable;
    [ObservableProperty] private ColumnInfoModel? _referenceColumn;
    
    [ObservableProperty] private ModifyStatus _modifyStatus;
    #endregion
    

    public ForeignKeyInfo GetForeignKeyInfo()
    {
        return new ForeignKeyInfo()
        {
            ColumnId = Column?.Id ?? 0,
            TableName = ReferenceTable?.Name ?? string.Empty,
            ReferenceColumnId = ReferenceColumn?.Id ?? 0
        };
    }
}