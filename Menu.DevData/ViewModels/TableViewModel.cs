using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
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
        
        tableInfo.ColumnIdList.Clear();
        tableInfo.ColumnIdList.AddRange(TableInfoModel.ColumnList.Select(x => x.Id));
        // TODO

        if (!tableInfo.ToFile())
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