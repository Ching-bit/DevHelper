using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;
using Plugin.DevData;

namespace Menu.DevData;

public partial class ColumnsViewModel : UniViewModel
{
    #region Constructors
    public ColumnsViewModel()
    {
        ColumnList = [];
        ColumnListView = new DataGridCollectionView(ColumnList);
        ColumnListView.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(ColumnInfo.Group)));
        IsColumnChanged = false;
        
        InitData();
    }

    private void InitData()
    {
        ColumnList.Clear();
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            ColumnList.Add(new ColumnInfoModel(columnInfo));
        }
        ColumnListView.Refresh();
        IsColumnChanged = false;
    }
    #endregion


    #region Properties
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private DataGridCollectionView _columnListView;
    [ObservableProperty] private ColumnInfoModel? _selectedColumn;
    [ObservableProperty] private bool _isColumnChanged;
    #endregion


    #region Commands

    [RelayCommand]
    private async Task Save()
    {
        // reserve the deleted columns which are being used
        List<ColumnInfoModel> deletedColumns = ColumnList.Where(x => ModifyStatus.Deleted == x.ModifyStatus).ToList();
        foreach (ColumnInfoModel deletedColumn in deletedColumns)
        {
            ErrorInfo errorInfo = Global.Get<IDevData>().CheckDeleteColumn(deletedColumn.Id);
            if (!errorInfo.IsSuccess)
            {
                await MessageDialog.Show(errorInfo.ErrorMessage);
                deletedColumn.ModifyStatus = ModifyStatus.Normal;
            }
        }
        
        // remove deleted columns
        List<ColumnInfoModel> columnModels = ColumnList.Where(x => ModifyStatus.Deleted != x.ModifyStatus).ToList();
        
        // sync to plugin and save
        if (Global.Get<IDevData>().UpdateColumns(columnModels.Select(columnInfoModel => columnInfoModel.GetColumnInfo()).ToList()))
        {
            ShowNotification("R_STR_SAVE_SUCCESS", NotificationType.Success);
            InitData();
        }
        else
        {
            ShowNotification("R_STR_SAVE_FAILED", NotificationType.Error);
        }
    }

    [RelayCommand]
    private void Restore()
    {
        InitData();
    }
    
    [RelayCommand]
    private async Task AddColumn()
    {
        ColumnDialogViewModel vm = new(null, GetAllColumnGroups(), ColumnList.Count <= 0 ? 1 : ColumnList.Max(x => x.Id) + 1);
        ConfirmDialogResult result = await ConfirmDialog.Show<ColumnDialog>(vm);
        if (!result.IsConfirmed)
        {
            return;
        }
        
        ColumnInfoModel columnInfoModel = (ColumnInfoModel)result.ReturnParameter!;
        ColumnList.Add(columnInfoModel);
        IsColumnChanged = true;
    }

    [RelayCommand]
    private async Task ModifyColumn()
    {
        // not select any column
        if (null == SelectedColumn)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_COLUMN_NOTICE", true);
            return;
        }
        
        // warning: the column is being used
        ErrorInfo errorInfo = Global.Get<IDevData>().CheckDeleteColumn(SelectedColumn.Id);
        if (!errorInfo.IsSuccess)
        {
            bool confirmResult = await MessageDialog.Show(errorInfo.ErrorMessage, isCancelButtonVisible: true);
            if (!confirmResult)
            {
                return;
            }
        }
        
        // modifying dialog
        ColumnDialogViewModel vm = new(SelectedColumn, GetAllColumnGroups());
        ConfirmDialogResult result = await ConfirmDialog.Show<ColumnDialog>(vm);
        if (!result.IsConfirmed)
        {
            return;
        }
        
        // modify data
        ColumnInfoModel modifiedColumn = (ColumnInfoModel)result.ReturnParameter!;
        if (ObjectHelper.Equals(SelectedColumn, modifiedColumn))
        {
            return;
        }
        
        bool groupChanged = !SelectedColumn.Group.Equals(modifiedColumn.Group);
        ObjectHelper.Copy(SelectedColumn, modifiedColumn);
        IsColumnChanged = true;
        if (groupChanged)
        {
            ColumnListView.Refresh();
        }
    }

    [RelayCommand]
    private async Task DeleteColumn()
    {
        if (View is not ColumnsView columnsView)
        {
            return;
        }

        List<ColumnInfoModel> selectedColumns = [];
        selectedColumns.AddRange(columnsView.DataGridColumns.SelectedItems.Cast<ColumnInfoModel>());

        // not select any column
        if (selectedColumns.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_COLUMN_NOTICE", true);
            return;
        }
        
        // if any column is being used, it is not allowed to delete
        foreach (ColumnInfoModel selectedColumn in selectedColumns)
        {
            ErrorInfo errorInfo = Global.Get<IDevData>().CheckDeleteColumn(selectedColumn.Id);
            if (!errorInfo.IsSuccess)
            {
                await MessageDialog.Show(errorInfo.ErrorMessage);
                return;
            }
        }
        
        // confirm
        bool isConfirmed = await MessageDialog.Show(
            ResourceHelper.FindResource<string>("R_STR_DELETE_COLUMNS_CONFIRM_NOTICE") +
            Environment.NewLine +
            string.Join(Environment.NewLine, selectedColumns.Select(x => $"{x.Id}: {x.Name}({x.Description})")),
            isCancelButtonVisible: true);
        if (!isConfirmed)
        {
            return;
        }

        // delete, only tag its ModifyStatus
        foreach (ColumnInfoModel columnInfoModel in selectedColumns)
        {
            columnInfoModel.ModifyStatus = ModifyStatus.Deleted;
        }
        IsColumnChanged = true;
    }
    
    [RelayCommand]
    private async Task EditColumnDataDictionary(ColumnInfoModel columnInfoModel)
    {
        ConfirmDialogResult result =
            await ConfirmDialog.Show<DataDictionaryDialog>(
                new DataDictionaryDialogViewModel(columnInfoModel.DataDict));
        if (!result.IsConfirmed)
        {
            return;
        }

        ObservableCollection<DataDictionaryItemModel> dataDictionaryItems = (ObservableCollection<DataDictionaryItemModel>)result.ReturnParameter!;
        string dataDictString = DataDictionaryDialogViewModel.DictToString(dataDictionaryItems.ToList());
        if (columnInfoModel.DataDict.Equals(dataDictString))
        {
            return;
        }

        columnInfoModel.DataDict = dataDictString;
        columnInfoModel.ModifyStatus = ModifyStatus.Modified;
        IsColumnChanged = true;
    }
    #endregion


    private List<string> GetAllColumnGroups()
    {
        return
            ColumnList 
                .Where(x => !x.Group.Equals(ColumnInfo.DefaultColumnGroup))
                .Select(x => x.Group)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
    }
}