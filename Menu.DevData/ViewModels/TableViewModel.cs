using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class TableViewModel : UniViewModel
{
    public TableViewModel()
    {
        AllColumnList = [];
        TableInfoModel = new();
    }

    public override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitData();
        
        if (View is not UniMenu uniMenu || uniMenu.MenuConf?.Entity is not TableInfo tableInfo)
        {
            throw new Exception("Cannot get table instance from the menu");
        }
        
        // TODO
        tableInfo.ColumnIdList = [1, 2];
        
        TableInfoModel = new TableInfoModel(tableInfo);
    }

    private void InitData()
    {
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            AllColumnList.Add(columnInfo);
        }
    }
    
    [ObservableProperty] private ObservableCollection<ColumnInfo> _allColumnList;
    [ObservableProperty] private TableInfoModel _tableInfoModel;
}