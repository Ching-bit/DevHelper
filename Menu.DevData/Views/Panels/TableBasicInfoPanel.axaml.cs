using Attributes.Avalonia;
using Framework.Common;

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