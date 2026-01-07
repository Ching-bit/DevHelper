using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData.Dialogs;

public partial class ColumnDialogViewModel : ConfirmDialogViewModel
{
    public ColumnDialogViewModel()
    {
        ColumnInfoModel = new ColumnInfoModel();
        SelectedGroupIndex = 1;
        GroupName = string.Empty;
        
        OnConfirmEvent -= OnConfirm;
        OnConfirmEvent += OnConfirm;
    }
    

    #region Item Sources
    public List<string> ColumnGroups
    {
        get
        {
            List<string> columnGroups =
            [
                ResourceHelper.FindStringResource("R_STR_NEW_ADD", "New..."),
                "Default"
            ];

            foreach (ColumnInfo item in Global.Get<IDevData>().Columns)
            {
                if (!columnGroups.Contains(item.Group))
                {
                    columnGroups.Add(item.Group);
                }
            }
            
            return columnGroups;
        }
    }
    
    public static Array ColumnTypes => Enum.GetValues(typeof(ColumnType));
    #endregion
    
    
    [ObservableProperty] private ColumnInfoModel _columnInfoModel;
    [ObservableProperty] private int _selectedGroupIndex;
    [ObservableProperty] private string _groupName;

    private void OnConfirm()
    {
        ColumnInfoModel.Group = 0 == SelectedGroupIndex ? GroupName : ColumnGroups[SelectedGroupIndex];
    }
}