using System.Collections.ObjectModel;
using Attributes.Avalonia;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ColumnInfoModel>), "ColumnList")]
[WithDirectProperty(typeof(bool), "IsColumnChanged", false)]
[WithDirectProperty(typeof(ObservableCollection<IndexInfoModel>), "IndexList")]
public partial class TableColumnsPanel : UniPanel
{
    public TableColumnsPanel()
    {
        InitializeComponent();
        _columnList = [];
        _indexList = [];
    }
    
    [RelayCommand]
    private async Task Add()
    {
        TableColumnDialogViewModel vm = new(ColumnList.ToList());
        vm.OnConfirmEvent += () =>
        {
            if (ColumnList.Any(x => x.Name.Equals(vm.SelectedColumn?.Name)))
            {
                return false;
            }
            return true;
        };
        
        ConfirmDialogResult result = await ConfirmDialog.Show<TableColumnDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not ColumnInfoModel columnInfoModel)
        {
            return;
        }

        columnInfoModel.ModifyStatus = ModifyStatus.Added;
        ColumnList.Add(columnInfoModel);
        IsColumnChanged = true;
    }

    [RelayCommand]
    private async Task Modify()
    {
        if (DataGridColumns.SelectedItem is not ColumnInfoModel selectedColumn)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_COLUMN_NOTICE", true);
            return;
        }
        
        // if it is used by any index, it is not allowed to modify
        foreach (IndexInfoModel indexInfoModel in IndexList)
        {
            if (indexInfoModel.ColumnList.Any(x => x.Id.Equals(selectedColumn.Id)))
            {
                string errMsg = ResourceHelper.FindStringResource("R_STR_COLUMN_IS_USED_BY_INDEX_NOTICE")
                    .Replace("#1", selectedColumn.Name)
                    .Replace("#2", indexInfoModel.Name);
                await MessageDialog.Show(errMsg);
                return;
            }
        }
        
        // modify dialog
        TableColumnDialogViewModel vm = new(ColumnList.Where(x => x.Id != selectedColumn.Id).ToList());
        ConfirmDialogResult result = await ConfirmDialog.Show<TableColumnDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not ColumnInfoModel columnInfoModel)
        {
            return;
        }
        
        ObjectHelper.Copy(selectedColumn, columnInfoModel);
        selectedColumn.ModifyStatus = ModifyStatus.Modified;
        IsColumnChanged = true;
    }

    [RelayCommand]
    private async Task Delete()
    {
        List<ColumnInfoModel> selectedColumns = DataGridColumns.SelectedItems.Cast<ColumnInfoModel>().ToList();
        if (selectedColumns.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_COLUMN_NOTICE", true);
            return;
        }

        // if any column is used by any index, it is not allowed to delete
        foreach (ColumnInfoModel selectedColumn in selectedColumns)
        {
            foreach (IndexInfoModel indexInfoModel in IndexList)
            {
                if (indexInfoModel.ColumnList.Any(x => x.Id.Equals(selectedColumn.Id)))
                {
                    string errMsg = ResourceHelper.FindStringResource("R_STR_COLUMN_IS_USED_BY_INDEX_NOTICE")
                        .Replace("#1", selectedColumn.Name)
                        .Replace("#2", indexInfoModel.Name);
                    await MessageDialog.Show(errMsg);
                    return;
                }
            }
        }
        
        foreach (ColumnInfoModel selectedColumn in selectedColumns)
        {
            selectedColumn.ModifyStatus = ModifyStatus.Deleted;
        }
        IsColumnChanged = true;
    }
}