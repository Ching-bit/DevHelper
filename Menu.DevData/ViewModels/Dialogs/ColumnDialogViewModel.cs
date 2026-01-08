using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ColumnDialogViewModel : ConfirmDialogViewModel
{
    public ColumnDialogViewModel()
    {
        ColumnInfoModel = new ColumnInfoModel();
        SelectedGroupIndex = 1;
        GroupName = string.Empty;

        ColumnGroups =
        [
            ResourceHelper.FindStringResource("R_STR_NEW_ADD", "New..."),
            ColumnInfo.DEFAULT_COLUMN_NAME
        ];
        
        OnConfirmEvent -= OnConfirm;
        OnConfirmEvent += OnConfirm;
    }
    

    #region Item Sources
    public List<string> ColumnGroups { get; }
    #endregion
    
    
    [ObservableProperty] private ColumnInfoModel _columnInfoModel;
    [ObservableProperty] private int _selectedGroupIndex;
    [ObservableProperty] private string _groupName;

    private void OnConfirm()
    {
        ColumnInfoModel.Group = 0 == SelectedGroupIndex ? GroupName : ColumnGroups[SelectedGroupIndex];
    }


}