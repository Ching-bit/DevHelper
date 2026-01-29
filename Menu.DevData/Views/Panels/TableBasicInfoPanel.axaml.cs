using Attributes.Avalonia;
using Avalonia;
using Avalonia.Interactivity;
using Framework.Common;

namespace Menu.DevData;

[WithDirectProperty(typeof(string), "TableName", nullable: true)]
[WithDirectProperty(typeof(string), "TableDescription", nullable: true)]
[WithDirectProperty(typeof(bool), "HasHistoryTable")]
[WithDirectProperty(typeof(string), "Remark", nullable: true)]
[WithDirectProperty(typeof(bool), "IsBasicInfoChanged")]
[WithDirectProperty(typeof(ModifyStatus), "InternalHasHistoryTableModifyStatus")]
[WithDirectProperty(typeof(ModifyStatus), "InternalRemarkModifyStatus")]
public partial class TableBasicInfoPanel : UniPanel
{
    public TableBasicInfoPanel()
    {
        InitializeComponent();
    }
    
    private bool _hasHistoryTableOriginalValue;
    private string? _remarkOriginalValue;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == HasHistoryTableProperty)
        {
            InternalHasHistoryTableModifyStatus =
                _hasHistoryTableOriginalValue == HasHistoryTable ? ModifyStatus.Normal : ModifyStatus.Modified;
            IsBasicInfoChanged =
                InternalHasHistoryTableModifyStatus != ModifyStatus.Normal ||
                InternalRemarkModifyStatus != ModifyStatus.Normal;
        }
        else if (e.Property == RemarkProperty)
        {
            InternalRemarkModifyStatus =
                _remarkOriginalValue == Remark ? ModifyStatus.Normal : ModifyStatus.Modified;
            IsBasicInfoChanged =
                InternalHasHistoryTableModifyStatus != ModifyStatus.Normal ||
                InternalRemarkModifyStatus != ModifyStatus.Normal;
        }
        else if (e.Property == IsBasicInfoChangedProperty)
        {
            if (!IsBasicInfoChanged)
            {
                _hasHistoryTableOriginalValue = HasHistoryTable;
                InternalHasHistoryTableModifyStatus = ModifyStatus.Normal;
                _remarkOriginalValue = Remark;
                InternalRemarkModifyStatus = ModifyStatus.Normal;
            }
        }
    }
}