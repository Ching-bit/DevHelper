using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableInfoModel : UniModel
{
    public TableInfoModel()
    {
        Name = string.Empty;
        Description = string.Empty;
        Remark = string.Empty;
        ColumnIds = [];
        AllColumns = [];
    }

    public TableInfoModel(TableInfo tableInfo)
    {
        Name = tableInfo.Name;
        Description = tableInfo.Description;
        Remark = tableInfo.Remark;
        ColumnIds = [];
        foreach (int columnId in tableInfo.ColumnIdList)
        {
            ColumnIds.Add(columnId);
        }

        AllColumns = [];
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            AllColumns.Add(columnInfo);
        }
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _remark;
    [ObservableProperty] private ObservableCollection<int> _columnIds;
    [ObservableProperty] private ObservableCollection<ColumnInfo> _allColumns;
}