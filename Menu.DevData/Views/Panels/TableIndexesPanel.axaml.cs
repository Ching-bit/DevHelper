using System.Collections.ObjectModel;
using Attributes.Avalonia;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<IndexInfoModel>), "IndexList")]
[WithDirectProperty(typeof(ObservableCollection<ColumnInfoModel>), "ColumnList")]
[WithDirectProperty(typeof(string), "TableName", "")]
[WithDirectProperty(typeof(bool), "IsIndexChanged")]
public partial class TableIndexesPanel : UniPanel
{
    public TableIndexesPanel()
    {
        InitializeComponent();
        _indexList = [];
        _columnList = [];
    }

    [RelayCommand]
    private async Task Add()
    {
        TableIndexDialogViewModel vm = new(IndexList.ToList(), ColumnList.ToList(), TableName);
        ConfirmDialogResult result = await ConfirmDialog.Show<TableIndexDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not IndexInfoModel indexInfoModel)
        {
            return;
        }

        indexInfoModel.ModifyStatus = ModifyStatus.Added;
        IndexList.Add(indexInfoModel);
        IsIndexChanged = true;
    }
    
    [RelayCommand]
    private async Task Modify()
    {
        if (DataGridIndexes.SelectedItem is not IndexInfoModel selectedIndex)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_INDEX_NOTICE", true);
            return;
        }

        TableIndexDialogViewModel vm = new(IndexList.Where(x => !x.Equals(selectedIndex)).ToList(), ColumnList.ToList(), TableName);
        foreach (ColumnInfoModel columnInfoModel in selectedIndex.ColumnList)
        {
            vm.IndexInfoModel.ColumnList.Add(columnInfoModel);
        }
        vm.IndexInfoModel.Name = selectedIndex.Name;
        vm.IndexInfoModel.Type = selectedIndex.Type;
        
        ConfirmDialogResult result = await ConfirmDialog.Show<TableIndexDialog>(vm);
        if (!result.IsConfirmed || result.ReturnParameter is not IndexInfoModel indexInfoModel || selectedIndex.Equals(indexInfoModel))
        {
            return;
        }
        
        selectedIndex.CopyFrom(indexInfoModel, ColumnList.ToList());
        selectedIndex.ModifyStatus = ModifyStatus.Modified;
        IsIndexChanged = true;
    }

    [RelayCommand]
    private async Task Delete()
    {
        List<IndexInfoModel> selectedIndexes = DataGridIndexes.SelectedItems.Cast<IndexInfoModel>().ToList();
        if (selectedIndexes.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_INDEX_NOTICE", true);
            return;
        }

        foreach (IndexInfoModel indexInfoModel in selectedIndexes)
        {
            indexInfoModel.ModifyStatus = ModifyStatus.Deleted;
        }
        IsIndexChanged = true;
    }
    
}