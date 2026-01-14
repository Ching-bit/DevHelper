using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ColumnDialogViewModel : ConfirmDialogViewModel
{
    public ColumnDialogViewModel(ColumnInfoModel? columnInfoModel, List<string> columnGroups, int id = 0)
    {
        ColumnGroups =
        [
            ResourceHelper.FindStringResource("R_STR_NEW_ADD", "New..."),
            ColumnInfo.DEFAULT_COLUMN_NAME
        ];
        ColumnGroups.AddRange(columnGroups);

        if (null == columnInfoModel)
        {
            // Add
            ColumnInfoModel = new ColumnInfoModel
            {
                Id = id,
                ModifyStatus = ModifyStatus.Added
            };
            SelectedGroupIndex = 1;
        }
        else
        {
            // Modify
            ColumnInfoModel = new ColumnInfoModel();
            ObjectHelper.Copy(ColumnInfoModel, columnInfoModel);
            if (ModifyStatus.Added != ColumnInfoModel.ModifyStatus)
            {
                ColumnInfoModel.ModifyStatus = ModifyStatus.Modified;
            }
            SelectedGroupIndex = ColumnGroups.IndexOf(ColumnInfoModel.Group);
        }
        
        GroupName = string.Empty;
        
        OnConfirmEvent -= OnConfirm;
        OnConfirmEvent += OnConfirm;
    }
    

    #region Item Sources
    public List<string> ColumnGroups { get; }
    #endregion
    
    
    [ObservableProperty] private ColumnInfoModel _columnInfoModel;
    [ObservableProperty] private int _selectedGroupIndex;
    [ObservableProperty] private string _groupName;

    private bool OnConfirm()
    {
        ColumnInfoModel.Group = 0 == SelectedGroupIndex ? GroupName : ColumnGroups[SelectedGroupIndex];
        return true;
    }
    
}