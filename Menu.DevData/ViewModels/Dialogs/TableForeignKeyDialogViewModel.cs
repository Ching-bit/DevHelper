using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableForeignKeyDialogViewModel : ConfirmDialogViewModel
{
    public TableForeignKeyDialogViewModel(List<ColumnInfoModel> columnList, List<TableInfoModel> tableList)
    {
        ColumnList = columnList;
        TableList = tableList;
        ReferenceColumnList = [];
        ForeignKeyInfoModel = new ForeignKeyInfoModel();

        ForeignKeyInfoModel.PropertyChanged -= OnForeignKeyChanged;
        ForeignKeyInfoModel.PropertyChanged += OnForeignKeyChanged;
    }

    #region Properties
    public List<ColumnInfoModel> ColumnList { get; }
    public List<TableInfoModel> TableList { get; }
    
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _referenceColumnList;
    [ObservableProperty] private ForeignKeyInfoModel _foreignKeyInfoModel;
    #endregion
    
    private void OnForeignKeyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (nameof(ForeignKeyInfoModel.ReferenceTable) == e.PropertyName && null != ForeignKeyInfoModel.ReferenceTable)
        {
            ReferenceColumnList.Clear();
            foreach (IndexInfoModel indexInfoModel in ForeignKeyInfoModel.ReferenceTable.IndexList)
            {
                if (indexInfoModel.Type is IndexType.Primary or IndexType.Unique &&
                    1 == indexInfoModel.ColumnList.Count &&
                    !ReferenceColumnList.Select(x => x.Id).Contains(indexInfoModel.ColumnList[0].Id))
                {
                    ReferenceColumnList.Add(indexInfoModel.ColumnList[0]);
                }
            }
        }
    }
}