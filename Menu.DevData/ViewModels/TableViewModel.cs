using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableViewModel : UniViewModel
{
    public override void OnMenuInit()
    {
        base.OnMenuInit();
        InitData();
    }

    [RelayCommand]
    private void Save()
    {
        if (null == TableInfoModel || View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not TableInfo tableInfo)
        {
            throw new Exception("Cannot get table instance from the menu");
        }

        foreach (ColumnInfoModel deletedColumn in TableInfoModel.ColumnList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList())
        {
            TableInfoModel.ColumnList.Remove(deletedColumn);
        }

        foreach (IndexInfoModel deletedIndex in TableInfoModel.IndexList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList())
        {
            TableInfoModel.IndexList.Remove(deletedIndex);
        }

        if (!Global.Get<IDevData>().UpdateTable(tableInfo,
                TableInfoModel.ColumnList.Select(x => x.Id).ToList(),
                TableInfoModel.IndexList.Select(x => x.GetIndexInfo()).ToList(),
                TableInfoModel.Remark) ||
            !tableInfo.ToFile())
        {
            ShowNotification("R_STR_SAVE_FAILED", NotificationType.Error);
            return;
        }

        InitData();
    }

    [RelayCommand]
    private void Restore()
    {
        InitData();
    }

    private void InitData()
    {
        if (View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not TableInfo tableInfo)
        {
            throw new Exception("Cannot get table instance from the menu");
        }
        
        TableInfoModel = new TableInfoModel(tableInfo);
        IsTableChanged = false;
    }
    
    [ObservableProperty] private TableInfoModel? _tableInfoModel;
    [ObservableProperty] private bool _isTableChanged;
}