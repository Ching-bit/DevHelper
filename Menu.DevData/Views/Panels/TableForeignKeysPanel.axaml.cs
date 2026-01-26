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
        
        TableForeignKeyDialogViewModel vm = new(sourceColumnList, tableList);
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
    private void Modify()
    {
        
    }

    [RelayCommand]
    private void Delete()
    {
        
    }
    
}