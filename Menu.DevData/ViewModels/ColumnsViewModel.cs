using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;
using Plugin.DevData;
using Ursa.Controls;

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
    }

    public override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitData();
    }

    private void InitData()
    {
        ColumnList.Clear();
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            ColumnList.Add(new ColumnInfoModel(columnInfo));
        }
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
        // remove deleted columns
        List<ColumnInfoModel> columnModels = ColumnList.Where(x => ModifyStatus.Deleted != x.ModifyStatus).ToList();
        
        // sync to plugin and save
        List<ColumnInfo> columns = [];
        columns.AddRange(columnModels.Select(columnInfoModel => columnInfoModel.GetColumnInfo()));
        try
        {
            Global.Get<IDevData>().Columns = columns;
            await Dialog.ShowCustomModal<ConfirmDialogResult>(
                new MessageDialog
                {
                    Message = ResourceHelper.FindStringResource("R_STR_SAVE_SUCCESS"),
                    IsAutoClick = true,
                    IsCancelButtonVisible = false
                },
                new ConfirmDialogViewModel(),
                options: new DialogOptions
                {
                    Mode = DialogMode.Warning,
                    CanDragMove = true,
                    IsCloseButtonVisible = true,
                    CanResize = false
                });
            
            InitData();
        }
        catch (Exception)
        {
            await Dialog.ShowCustomModal<ConfirmDialogResult>(
                new MessageDialog
                {
                    Message = ResourceHelper.FindStringResource("R_STR_SAVE_FAILED"),
                    IsCancelButtonVisible = false
                },
                new ConfirmDialogViewModel(),
                options: new DialogOptions
                {
                    Mode = DialogMode.Warning,
                    CanDragMove = true,
                    IsCloseButtonVisible = true,
                    CanResize = false
                });
        }
        

        
    }

    [RelayCommand]
    private void Restore()
    {
        InitData();
        IsColumnChanged = false;
    }
    
    [RelayCommand]
    private async Task AddColumn()
    {
        ColumnDialogViewModel vm = new();
        vm.ColumnGroups.AddRange(GetAllColumnGroups());
        vm.ColumnInfoModel.ModifyStatus = ModifyStatus.Added;
        vm.ColumnInfoModel.Id = ColumnList.Max(x => x.Id) + 1;
        
        ConfirmDialogResult? result = await Dialog.ShowCustomModal<ColumnDialog, ColumnDialogViewModel, ConfirmDialogResult>(vm);
        if (true != result?.IsConfirmed)
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
            await Dialog.ShowCustomModal<ConfirmDialogResult>(
                new MessageDialog
                {
                    Message = ResourceHelper.FindStringResource("R_STR_SELECT_EMPTY_COLUMNS_NOTICE"),
                    IsAutoClick = true,
                    IsCancelButtonVisible = false
                },
                new ConfirmDialogViewModel(),
                options: new DialogOptions
                {
                    Mode = DialogMode.Warning,
                    CanDragMove = true,
                    IsCloseButtonVisible = true,
                    CanResize = false
                });
            return;
        }
        
        // modifying dialog
        ColumnDialogViewModel vm = new();
        vm.ColumnGroups.AddRange(GetAllColumnGroups());
        ObjectHelper.Copy(vm.ColumnInfoModel, SelectedColumn);
        vm.ColumnInfoModel.ModifyStatus = ModifyStatus.Modified;
        ConfirmDialogResult? result = await Dialog.ShowCustomModal<ColumnDialog, ColumnDialogViewModel, ConfirmDialogResult>(vm);
        if (true != result?.IsConfirmed)
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
            await Dialog.ShowCustomModal<ConfirmDialogResult>(
                new MessageDialog
                {
                    Message = ResourceHelper.FindStringResource("R_STR_SELECT_EMPTY_COLUMNS_NOTICE"),
                    IsAutoClick = true,
                    IsCancelButtonVisible = false
                },
                new ConfirmDialogViewModel(),
                options: new DialogOptions
                {
                    Mode = DialogMode.Warning,
                    CanDragMove = true,
                    IsCloseButtonVisible = true,
                    CanResize = false
                });
            return;
        }
        
        // confirm
        ConfirmDialogResult? confirmResult = await Dialog.ShowCustomModal<ConfirmDialogResult>(
            new MessageDialog
            {
                Message = ResourceHelper.FindStringResource("R_STR_DELETE_COLUMNS_CONFIRM_NOTICE") +
                          Environment.NewLine +
                          string.Join(Environment.NewLine, selectedColumns.Select(x => $"{x.Id}: {x.Name}({x.Description})"))
            },
            new ConfirmDialogViewModel(),
            options: new DialogOptions
            {
                Mode = DialogMode.Warning,
                CanDragMove = true,
                IsCloseButtonVisible = true,
                CanResize = false
            });
        if (true != confirmResult?.IsConfirmed)
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
    #endregion


    private List<string> GetAllColumnGroups()
    {
        return
            ColumnList 
                .Where(x => !x.Group.Equals(ColumnInfo.DEFAULT_COLUMN_NAME))
                .Select(x => x.Group)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
    }
}