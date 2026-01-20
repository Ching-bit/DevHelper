using System.Collections.ObjectModel;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        
        IndexList.Add(indexInfoModel);
        IsIndexChanged = true;
    }
    
    [RelayCommand]
    private void Modify()
    {
        
    }

    [RelayCommand]
    private void Delete()
    {
        
    }
    
}