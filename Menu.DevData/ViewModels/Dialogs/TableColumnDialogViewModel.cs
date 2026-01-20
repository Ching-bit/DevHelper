using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableColumnDialogViewModel : ConfirmDialogViewModel
{
    public TableColumnDialogViewModel(List<ColumnInfoModel> columnList)
    {
        AllColumns = [];

        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns.Where(x => !columnList.Select(y => y.Id).Contains(x.Id)))
        {
            AllColumns.Add(new ColumnInfoModel(columnInfo));
        }
    }
    
    [ObservableProperty] private ColumnInfoModel? _selectedColumn;
    
    public List<ColumnInfoModel> AllColumns { get; }
}