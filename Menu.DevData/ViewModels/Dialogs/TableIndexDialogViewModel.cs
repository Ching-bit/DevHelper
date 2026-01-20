using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableIndexDialogViewModel : ConfirmDialogViewModel
{
    public TableIndexDialogViewModel(List<IndexInfoModel> indexList, List<ColumnInfoModel> columnList, string tableName)
    {
        AllTypes = [IndexType.Unique, IndexType.Index];
        if (indexList.All(x => IndexType.Primary != x.Type))
        {
            AllTypes.Insert(0, IndexType.Primary);
        }
        
        AllColumns = [];
        AllColumns.AddRange(columnList);

        TableName = tableName;
        
        IndexInfoModel = new IndexInfoModel(TableName);
    }
    
    [ObservableProperty] private IndexInfoModel _indexInfoModel;
    
    public List<IndexType> AllTypes { get; }
    public List<ColumnInfoModel> AllColumns { get; }
    public string TableName { get; }
}