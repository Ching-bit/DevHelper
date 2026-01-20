using System.Collections.ObjectModel;
using Attributes.Avalonia;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ColumnInfoModel>), "ColumnList")]
[WithDirectProperty(typeof(bool), "IsColumnChanged", false)]
public partial class TableColumnsPanel : UniPanel
{
    public TableColumnsPanel()
    {
        InitializeComponent();
        _columnList = [];
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
            return;
        }
        
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
    private void Delete()
    {
        List<ColumnInfoModel> selectedColumns = DataGridColumns.SelectedItems.Cast<ColumnInfoModel>().ToList();
        if (selectedColumns.Count <= 0)
        {
            return;
        }
        
        foreach (ColumnInfoModel selectedColumn in selectedColumns)
        {
            selectedColumn.ModifyStatus = ModifyStatus.Deleted;
        }
        IsColumnChanged = true;
    }
}