using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ApiParamSetModel : UniModel
{
    #region Constructors
    public ApiParamSetModel(ApiParamSetType type)
    {
        Type = type;
    }
    
    public ApiParamSetModel(ApiParamSet apiParamSet)
    {
        Type = apiParamSet.Type;
        IsRepeated = apiParamSet.IsRepeated;
        Mode = apiParamSet.Mode;
        TableInfo? tableInfo = Global.Get<IDevData>().GetTableById(apiParamSet.TableId);
        if (null != tableInfo)
        {
            AssociatedTable = new TableInfoModel(tableInfo);
        }
        
        foreach (int columnId in apiParamSet.ColumnIdList)
        {
            ColumnInfo? columnInfo = Global.Get<IDevData>().Columns.FirstOrDefault(x => x.Id == columnId);
            if (null != columnInfo)
            {
                ColumnList.Add(new ColumnInfoModel(columnInfo));
            }
        }
        RecordOriginalValues();
    }
    #endregion
    
    
    #region Properties
    [ObservableProperty] private ApiParamSetType _type;
    [ObservableProperty] private bool _isRepeated;
    [ObservableProperty] private ApiParamSetMode _mode;
    [ObservableProperty] private TableInfoModel? _associatedTable;
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList = [];
    [ObservableProperty] private bool _isParamSetChanged = false;
    
    
    private ApiParamSetMode _originalMode;
    private bool _originalIsRepeated;
    private int _originalTableId;
    private List<int> _originalColumnIdList = [];
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(IsParamSetChanged))
        {
            if (!IsParamSetChanged)
            {
                RecordOriginalValues();
            }
        }
        else
        {
            IsParamSetChanged =
                _originalIsRepeated != IsRepeated ||
                _originalMode != Mode ||
                _originalTableId != (AssociatedTable?.Id ?? 0) ||
                !_originalColumnIdList.SequenceEqual(ColumnList.Select(x => x.Id));
            OnPropertyChanged(nameof(IsParamSetChanged));
        }
    }
    
    private void RecordOriginalValues()
    {
        _originalIsRepeated = IsRepeated;
        _originalMode = Mode;
        _originalTableId = AssociatedTable?.Id ?? 0;
        _originalColumnIdList = ColumnList.Select(x => x.Id).ToList();
    }
    #endregion
    
    
    #region Public Methods
    public ApiParamSet GetApiParamSet()
    {
        return new ApiParamSet
        {
            Type = Type,
            IsRepeated = IsRepeated,
            Mode = Mode,
            TableId = AssociatedTable?.Id ?? 0,
            ColumnIdList = ColumnList.Select(x => x.Id).ToList()
        };
    }
    #endregion
}