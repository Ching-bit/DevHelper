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

        AllColumns = [];
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            AllColumns.Add(new ColumnInfoModel(columnInfo));
        }
    }

    public TableInfoModel(TableInfo tableInfo) : this()
    {
        Name = tableInfo.Name;
        Description = tableInfo.Description;
        Remark = tableInfo.Remark;
        
        foreach (int columnId in tableInfo.ColumnIdList)
        {
            ColumnInfoModel? columnInfoModel = AllColumns.FirstOrDefault(x => x.Id == columnId);
            if (null != columnInfoModel)
            {
                ColumnList.Add(columnInfoModel);
            }
        }
        
        foreach (IndexInfo indexInfo in tableInfo.IndexList)
        {
            IndexList.Add(new IndexInfoModel(Name, indexInfo, AllColumns));
        }
        
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
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private string _remark;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private ObservableCollection<IndexInfoModel> _indexList;
    
    private List<ColumnInfoModel> AllColumns { get; }
}