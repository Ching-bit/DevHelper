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
        ColumnList = [];
        IndexList = [];
    }

    public TableInfoModel(TableInfo tableInfo)
    {
        Name = tableInfo.Name;
        Description = tableInfo.Description;
        Remark = tableInfo.Remark;

        ColumnList = [];
        foreach (int columnId in tableInfo.ColumnIdList)
        {
            ColumnInfo? columnInfo = Global.Get<IDevData>().Columns.FirstOrDefault(x => x.Id == columnId);
            if (null != columnInfo)
            {
                ColumnList.Add(new ColumnInfoModel(columnInfo));
            }
        }
        
        // TODO
        IndexList = [];
        
        IndexInfo? primaryKey = tableInfo.IndexList.FirstOrDefault(x => IndexType.Primary == x.Type);
        if (null != primaryKey)
        {
            foreach (int colId in primaryKey.ColumnIdList)
            {
                ColumnInfoModel? primaryColumn = ColumnList.FirstOrDefault(x => x.Id == colId);
                if (null != primaryColumn)
                {
                    primaryColumn.IsPrimaryKey = true;
                }
            }
        }
        
        
    }

    public TableInfo GetTableInfo()
    {
        TableInfo tableInfo = new()
        {
            Name = Name,
            Description = Description,
            Remark = Remark,
            ColumnIdList = ColumnList.Select(x => x.Id).ToList(),
        };
        return tableInfo;
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _remark;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private ObservableCollection<IndexInfoModel> _indexList;
}