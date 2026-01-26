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
[WithDirectProperty(typeof(string), "TableName", "")]
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
        List<ColumnInfoModel> sourceColumnList =
            ColumnList.Where(x => !ForeignKeyList.Select(y => y.Column?.Id).Contains(x.Id)).ToList();
        List<TableInfoModel> tableList = [];
        foreach (TableInfo tableInfo in Global.Get<IDevData>().GetTableList())
        {
            if (tableInfo.IndexList.Any(x => x.Type is IndexType.Primary or IndexType.Unique) &&
                tableInfo.Name != TableName)
            {
                tableList.Add(new TableInfoModel(tableInfo));
            }
        }
        
        TableForeignKeyDialogViewModel vm = new(sourceColumnList, tableList, TableName);
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
        
        List<ColumnInfoModel> sourceColumnList =
            ColumnList.Where(x => !ForeignKeyList.Where(z => z.Column?.Id != selecttedForeignKey.Column?.Id).Select(y => y.Column?.Id).Contains(x.Id)).ToList();
        List<TableInfoModel> tableList = [];
        foreach (TableInfo tableInfo in Global.Get<IDevData>().GetTableList())
        {
            if (tableInfo.IndexList.Any(x => x.Type is IndexType.Primary or IndexType.Unique) &&
                tableInfo.Name != TableName)
            {
                tableList.Add(new TableInfoModel(tableInfo));
            }
        }
        
        TableForeignKeyDialogViewModel vm = new(sourceColumnList, tableList, TableName)
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

        string confirmMessage = ResourceHelper.FindStringResource("R_STR_DELETE_CONFIRM_NOTICE")
            .Replace("#", string.Join(", ", selectedForeignKeys.Select(x => x.Name)));
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