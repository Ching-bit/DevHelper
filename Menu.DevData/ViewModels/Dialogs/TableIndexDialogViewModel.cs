using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableIndexDialogViewModel : ConfirmDialogViewModel
{
    public TableIndexDialogViewModel(List<IndexInfoModel> currentIndexList, List<ColumnInfoModel> tableColumnList, string tableName)
    {
        TableName = tableName;
        IndexInfoModel = new IndexInfoModel(TableName);
        
        AllTypes = [IndexType.Unique, IndexType.Index];
        IndexInfoModel.Type = IndexType.Unique;
        if (currentIndexList.All(x => IndexType.Primary != x.Type))
        {
            AllTypes.Insert(0, IndexType.Primary);
            IndexInfoModel.Type = IndexType.Primary;
        }
        
        TableColumnList = [];
        TableColumnList.AddRange(tableColumnList);
    }
    
    [ObservableProperty] private IndexInfoModel _indexInfoModel;
    
    public List<IndexType> AllTypes { get; }
    public List<ColumnInfoModel> TableColumnList { get; }
    public string TableName { get; }
}