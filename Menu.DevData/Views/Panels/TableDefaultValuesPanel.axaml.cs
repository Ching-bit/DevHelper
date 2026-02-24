using System.Collections;
using System.Collections.ObjectModel;
using Attributes.Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ColumnInfoModel>), "ColumnList")]
[WithDirectProperty(typeof(ObservableCollection<DynamicRow>), "DefaultValues")]
[WithDirectProperty(typeof(bool), "IsDefaultValueChanged")]
public partial class TableDefaultValuesPanel : UniPanel
{
    #region Constructor
    public TableDefaultValuesPanel()
    {
        InitializeComponent();
        _columnList = [];
        _defaultValues = [];

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitDataGridHeaders();
    }

    private void InitDataGridHeaders()
    {
        DataGridDefaultValues.Columns.Clear();
        foreach (ColumnInfoModel columnInfoModel in ColumnList)
        {
            DataGridDefaultValues.Columns.Add(new DataGridTextColumn
            {
                Header = columnInfoModel.Name,
                Binding = new Binding($"[{columnInfoModel.Name}]", mode: BindingMode.TwoWay),
                IsReadOnly = false
            });
        }
    }
    #endregion


    [RelayCommand]
    private void AddRow()
    {
        DynamicRow newRow = new();
        foreach (ColumnInfoModel columnInfoModel in ColumnList)
        {
            newRow[columnInfoModel.Name] = ResourceHelper.FindResource<string>("R_STR_INPUT_VALUE");
        }
        
        DefaultValues.Add(newRow);
        IsDefaultValueChanged = true;
    }

    [RelayCommand]
    private async Task DeleteRow()
    {
        IList selectedRows = DataGridDefaultValues.SelectedItems;
        if (selectedRows.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_ROW_NOTICE", true);
            return;
        }

        foreach (object selectedRow in selectedRows)
        {
            if (selectedRow is DynamicRow dynamicRow)
            {
                DefaultValues.Remove(dynamicRow);
            }
        }

        IsDefaultValueChanged = true;
    }
}