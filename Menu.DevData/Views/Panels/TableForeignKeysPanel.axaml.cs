using System.Collections.ObjectModel;
using Attributes.Avalonia;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ForeignKeyInfoModel>), "ForeignKeyList")]
[WithDirectProperty(typeof(ObservableCollection<ColumnInfoModel>), "ColumnList")]
[WithDirectProperty(typeof(bool), "IsForeignKeyChanged")]
[WithDirectProperty(typeof(int), "TableId")]
public partial class TableForeignKeysPanel : UniPanel
{
    public TableForeignKeysPanel()
    {
        InitializeComponent();
        _foreignKeyList = [];
        _columnList = [];
    }

    [RelayCommand]
    private async Task Add()
    {
        TableInfo? tableInfo = Global.Get<IDevData>().GetTableById(TableId);
        DatabaseInfo? database = Global.Get<IDevData>().GetDatabaseInfoByTableId(TableId);
        if (null == database)
        {
            throw new ArgumentException("Null database info when add a foreign key");
        }
        
        List<ColumnInfoModel> sourceColumnList =
            ColumnList.Where(x => !ForeignKeyList.Select(y => y.Column?.Id).Contains(x.Id)).ToList();
        List<TableInfoModel> tableList = [];
        
        foreach (TableInfo item in Global.Get<IDevData>().GetAllTables()[database])
        {
            if (item.IndexList.Any(x => x.Type is IndexType.Primary or IndexType.Unique) &&
                item.Id != TableId)
            {
                tableList.Add(new TableInfoModel(item));
            }
        }
        
        TableForeignKeyDialogViewModel vm = new(sourceColumnList, tableList, tableInfo?.Name ?? string.Empty);
        ConfirmDialogResult result = await ConfirmDialog.Show<TableForeignKeyDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not ForeignKeyInfoModel foreignKeyInfoModel)
        {
            return;
        }

        foreignKeyInfoModel.ModifyStatus = ModifyStatus.Added;
        ForeignKeyList.Add(foreignKeyInfoModel);
        IsForeignKeyChanged = true;
    }

    [RelayCommand]
    private async Task Modify()
    {
        if (DataGridForeignKeys.SelectedItem is not ForeignKeyInfoModel selecttedForeignKey)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_INDEX_NOTICE", true);
            return;
        }
        
        TableInfo? tableInfo = Global.Get<IDevData>().GetTableById(TableId);
        DatabaseInfo? database = Global.Get<IDevData>().GetDatabaseInfoByTableId(TableId);
        if (null == database)
        {
            throw new ArgumentException("Null database info when modify a foreign key");
        }
        
        List<ColumnInfoModel> sourceColumnList =
            ColumnList.Where(x => !ForeignKeyList.Where(z => z.Column?.Id != selecttedForeignKey.Column?.Id).Select(y => y.Column?.Id).Contains(x.Id)).ToList();
        List<TableInfoModel> tableList = [];
        foreach (TableInfo item in Global.Get<IDevData>().GetAllTables()[database])
        {
            if (item.IndexList.Any(x => x.Type is IndexType.Primary or IndexType.Unique) &&
                item.Id != TableId)
            {
                tableList.Add(new TableInfoModel(item));
            }
        }
        
        TableForeignKeyDialogViewModel vm = new(sourceColumnList, tableList, tableInfo?.Name ?? string.Empty)
        {
            ForeignKeyInfoModel =
            {
                Column = selecttedForeignKey.Column,
                ReferenceTable = selecttedForeignKey.ReferenceTable,
                ReferenceColumn = selecttedForeignKey.ReferenceColumn
            }
        };
        ConfirmDialogResult result = await ConfirmDialog.Show<TableForeignKeyDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not ForeignKeyInfoModel foreignKeyInfoModel)
        {
            return;
        }

        selecttedForeignKey.CopyFrom(foreignKeyInfoModel);
        selecttedForeignKey.ModifyStatus = ModifyStatus.Modified;
        IsForeignKeyChanged = true;
    }

    [RelayCommand]
    private async Task Delete()
    {
        List<ForeignKeyInfoModel> selectedForeignKeys = DataGridForeignKeys.SelectedItems.Cast<ForeignKeyInfoModel>().ToList();
        if (selectedForeignKeys.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_INDEX_NOTICE", true);
            return;
        }

        string confirmMessage = ResourceHelper.FindResource<string>("R_STR_DELETE_CONFIRM_NOTICE")
            .Replace("#1", string.Join(", ", selectedForeignKeys.Select(x => x.Name)));
        if (!await MessageDialog.Show(confirmMessage, isCancelButtonVisible: true))
        {
            return;
        }

        foreach (ForeignKeyInfoModel foreignKeyInfoModel in selectedForeignKeys)
        {
            foreignKeyInfoModel.ModifyStatus = ModifyStatus.Deleted;
        }
        IsForeignKeyChanged = true;
    }
    
}