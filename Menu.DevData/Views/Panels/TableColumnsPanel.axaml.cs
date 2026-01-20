using System.Collections.ObjectModel;
using Attributes.Avalonia;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

[WithDirectProperty(typeof(ObservableCollection<ColumnInfo>), "AllColumnList")]
[WithDirectProperty(typeof(ObservableCollection<ColumnInfo>), "ColumnList")]
public partial class TableColumnsPanel : UniPanel
{
    public TableColumnsPanel()
    {
        InitializeComponent();
        _allColumnList = [];
        _columnList = [];
    }
}