using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Framework.Common;
using Framework.Utils;
using Plugin.DevData;
using Plugin.DevData.Messages;

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
        ForeignKeyList = [];

        AllColumns = [];
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            AllColumns.Add(new ColumnInfoModel(columnInfo));
        }

        DefaultValues = [];
        
        // register table name changed
        WeakReferenceMessenger.Default.Register<ItemNameChangedMessage>(this, (_, message) =>
        {
            if (message.Value.Item is TableInfo tableInfo &&
                tableInfo.Id == Id)
            {
                Name = tableInfo.Name;
                Description = tableInfo.Description;
            }
        });
    }

    public TableInfoModel(TableInfo tableInfo) : this()
    {
        Id = tableInfo.Id;
        Name = tableInfo.Name;
        Description = tableInfo.Description;
        HasHistoryTable = tableInfo.HasHistoryTable;
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

        foreach (ForeignKeyInfo foreignKeyInfo in tableInfo.ForeignKeyList)
        {
            ForeignKeyList.Add(new ForeignKeyInfoModel(foreignKeyInfo, ColumnList.ToList(), Name));
        }

        foreach (string defaultValue in tableInfo.DefaultValues)
        {
            DynamicRow row = new();
            string[] defaultValueArray = defaultValue.Split(",");
            for (int i = 0; i < ColumnList.Count; i++)
            {
                string value = i < defaultValueArray.Length ? defaultValueArray[i] : string.Empty;
                row[ColumnList[i].Name] = value;
            }
            DefaultValues.Add(row);
        }
    }
    
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private ObservableCollection<IndexInfoModel> _indexList;
    [ObservableProperty] private ObservableCollection<ForeignKeyInfoModel> _foreignKeyList;
    [ObservableProperty] private bool _hasHistoryTable;
    [ObservableProperty] private string _remark;
    [ObservableProperty] private ObservableCollection<DynamicRow> _defaultValues;
    
    private List<ColumnInfoModel> AllColumns { get; }

    public override string ToString()
    {
        return $"{Name}{(string.IsNullOrEmpty(Description) ? "" : $" ({Description})")}";
    }
}