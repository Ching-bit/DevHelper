using Avalonia.Controls.Notifications;
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
            ColumnInfo.DefaultColumnGroup
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
        if (string.IsNullOrWhiteSpace(ColumnInfoModel.Name))
        {
            ShowNotification("R_STR_EMPTY_NAME_NOTICE", NotificationType.Error);
            return false;
        }

        if (!ColumnInfoModel.HasDefaultValue)
        {
            ColumnInfoModel.DefaultValue = string.Empty;
        }
        else
        {
            if (ColumnInfoModel.Type is ColumnType.Int32 && !int.TryParse(ColumnInfoModel.DefaultValue, out int _) ||
                ColumnInfoModel.Type is ColumnType.Int64 && !long.TryParse(ColumnInfoModel.DefaultValue, out long _) ||
                ColumnInfoModel.Type is ColumnType.Number && !double.TryParse(ColumnInfoModel.DefaultValue, out double _))
            {
                ShowNotification("R_STR_COLUMN_DEFAULT_VALUE_NOT_MATCH_TYPE", NotificationType.Error);
                return false;
            }
        }
        
        ColumnInfoModel.Group = 0 == SelectedGroupIndex ? GroupName : ColumnGroups[SelectedGroupIndex];
        return true;
    }
    
}