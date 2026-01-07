using System.Collections.ObjectModel;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Menu.DevData.Dialogs;
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
        
        InitData();
    }

    private void InitData()
    {
        foreach (ColumnInfo columnInfo in Global.Get<IDevData>().Columns)
        {
            ColumnList.Add(new ColumnInfoModel(columnInfo));
        }
    }
    #endregion


    #region Properties
    [ObservableProperty] private ObservableCollection<ColumnInfoModel> _columnList;
    [ObservableProperty] private DataGridCollectionView _columnListView;
    #endregion


    #region Commands
    [RelayCommand]
    private async Task AddColumn()
    {
        ConfirmDialogResult? result = await Dialog.ShowCustomModal<ColumnDialog, ColumnDialogViewModel, ConfirmDialogResult>(new ColumnDialogViewModel());
        if (true != result?.IsConfirmed)
        {
            return;
        }
        
        ColumnInfoModel columnInfoModel = (ColumnInfoModel)result.ReturnParameter!;
        columnInfoModel.Id = ColumnList.Max(x => x.Id) + 1;
        ColumnList.Add(columnInfoModel);
    }
    

    #endregion
}