using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class IndexInfoModel : UniModel
{
    #region Constructors
    public IndexInfoModel(string tableName)
    {
        _tableName = tableName;
        
        Name = string.Empty;
        ColumnList = [];
        ColumnListString = string.Empty;

        ColumnList.CollectionChanged += (_, _) =>
        {
            Name = GenIndexName();
            RefreshColumnListString();
        };
    }

    public IndexInfoModel(string tableName, IndexInfo indexInfo, List<ColumnInfoModel> allColumns) : this(tableName)
    {
        Name = indexInfo.Name;
        Type = indexInfo.Type;
        foreach (int columnId in indexInfo.ColumnIdList)
        {
            ColumnInfoModel? columnInfoModel = allColumns.FirstOrDefault(x => x.Id == columnId);
            if (null != columnInfoModel)
            {
                ColumnList.Add(columnInfoModel);
            }
        }
        AutoIncrementColumn = allColumns.FirstOrDefault(x => x.Id == indexInfo.AutoIncrementColumnId);
    }
    #endregion


    #region Properties
    private readonly string _tableName;
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private IndexType _type;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private ColumnInfoModel? _autoIncrementColumn;
    
    [ObservableProperty] private string _columnListString;
    [ObservableProperty] private ModifyStatus _modifyStatus;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Type) or nameof(ColumnList))
        {
            Name = GenIndexName();
            RefreshColumnListString();
        }
        else if (e.PropertyName == nameof(AutoIncrementColumn))
        {
            RefreshColumnListString();
        }
    }

    private string GenIndexName()
    {
        StringBuilder sbName = new();
        switch (Type)
        {
            case IndexType.Primary:
                return $"PK_{_tableName.ToUpper()}";
            case IndexType.Unique:
                sbName.Append("UNI");
                break;
            case IndexType.Index:
                sbName.Append("IDX");
                break;
        }

        sbName.Append("_" + _tableName.ToUpper());
            
        foreach (ColumnInfoModel columnInfoModel in ColumnList)
        {
            sbName.Append("_" + columnInfoModel.Name.ToUpper());
        }
            
        return sbName.ToString();
    }
    
    private void RefreshColumnListString()
    {
        ColumnListString = string.Join(", ", ColumnList.Select(x => x.Id == AutoIncrementColumn?.Id ? $"{x.Name}※" : x.Name));
    }
    #endregion


    #region Public Functions
    public IndexInfo GetIndexInfo()
    {
        IndexInfo indexInfo = new()
        {
            Name = Name,
            Type = Type,
            AutoIncrementColumnId = IndexType.Primary == Type ? AutoIncrementColumn?.Id : null
        };
        indexInfo.ColumnIdList.AddRange(ColumnList.Select(x => x.Id));
        return indexInfo;
    }

    public void CopyFrom(IndexInfoModel source, List<ColumnInfoModel> tableColumnList)
    {
        Name = source.Name;
        Type = source.Type;
        
        ColumnList.Clear();
        foreach (ColumnInfoModel sourceColumn in source.ColumnList)
        {
            ColumnInfoModel? columnInfoModel = tableColumnList.FirstOrDefault(x => x.Id == sourceColumn.Id);
            if (null != columnInfoModel)
            {
                ColumnList.Add(columnInfoModel);
            }
        }

        AutoIncrementColumn = source.AutoIncrementColumn;
    }

    public override bool Equals(object? obj)
    {
        if (obj is IndexInfoModel indexInfoModel)
        {
            return
                Name.Equals(indexInfoModel.Name) &&
                Type == indexInfoModel.Type &&
                new HashSet<int>(ColumnList.Select(x => x.Id)).SetEquals(indexInfoModel.ColumnList.Select(x => x.Id)) &&
                AutoIncrementColumn?.Id == indexInfoModel.AutoIncrementColumn?.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Name);
        hash.Add(Type);
        foreach (ColumnInfoModel columnInfoModel in ColumnList)
        {
            hash.Add(columnInfoModel.Id);
        }
        return hash.ToHashCode();
    }
    #endregion

    
    
}