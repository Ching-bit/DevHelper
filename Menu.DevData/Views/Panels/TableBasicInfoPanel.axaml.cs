using Attributes.Avalonia;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

[WithDirectProperty(typeof(TableInfoModel), "TableInfoModel")]
public partial class TableBasicInfoPanel : UniPanel
{
    public TableBasicInfoPanel()
    {
        InitializeComponent();
        _tableInfoModel = new TableInfoModel();
    }
}