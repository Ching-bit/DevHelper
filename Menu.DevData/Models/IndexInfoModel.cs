using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class IndexInfoModel : UniModel
{
    public IndexInfoModel(string tableName)
    {
        _tableName = tableName;
        
        Name = string.Empty;
        ColumnList = [];

        ColumnList.CollectionChanged += (_, _) =>
        {
            Name = GenIndexName();
        };
    }
    
    private readonly string _tableName;
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private IndexType _type;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    
    [ObservableProperty] private ModifyStatus _modifyStatus;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Type) or nameof(ColumnList))
        {
            Name = GenIndexName();
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
            case IndexType.Foreign:
                sbName.Append("FK");
                break;
        }

        sbName.Append("_" + _tableName.ToUpper());
            
        foreach (ColumnInfoModel columnInfoModel in ColumnList)
        {
            sbName.Append("_" + columnInfoModel.Name.ToUpper());
        }
            
        return sbName.ToString();
    }
}