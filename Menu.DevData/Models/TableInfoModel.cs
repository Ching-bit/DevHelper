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
        ColumnIdList = [];
        ColumnList = [];
    }

    public TableInfoModel(TableInfo tableInfo)
    {
        Name = tableInfo.Name;
        Description = tableInfo.Description;
        Remark = tableInfo.Remark;
        ColumnIdList = [];
        foreach (int columnId in tableInfo.ColumnIdList)
        {
            ColumnIdList.Add(columnId);
        }

        ColumnList = [];
        foreach (int columnId in ColumnIdList)
        {
            ColumnInfo? columnInfo = Global.Get<IDevData>().Columns.FirstOrDefault(x => x.Id == columnId);
            if (null != columnInfo)
            {
                ColumnList.Add(columnInfo);
            }
        }
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _remark;
    [ObservableProperty] private ObservableCollection<int> _columnIdList;
    [ObservableProperty] private ObservableCollection<ColumnInfo> _columnList;
}