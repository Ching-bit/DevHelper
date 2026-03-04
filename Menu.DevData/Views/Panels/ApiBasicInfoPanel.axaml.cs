using Attributes.Avalonia;
using Avalonia;
using Framework.Common;

namespace Menu.DevData;

[WithDirectProperty(typeof(string), "ApiName", nullable: true)]
[WithDirectProperty(typeof(string), "ApiDescription", nullable: true)]
[WithDirectProperty(typeof(string), "Remark", "")]
[WithDirectProperty(typeof(bool), "IsBasicInfoChanged")]
[WithDirectProperty(typeof(ModifyStatus), "InternalRemarkModifyStatus")]
public partial class ApiBasicInfoPanel : UniPanel
{
    public ApiBasicInfoPanel()
    {
        InitializeComponent();
    }
    
    private string _remarkOriginalValue = "";
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == RemarkProperty)
        {
            InternalRemarkModifyStatus =
                _remarkOriginalValue == Remark ? ModifyStatus.Normal : ModifyStatus.Modified;
            IsBasicInfoChanged = InternalRemarkModifyStatus != ModifyStatus.Normal;
        }
        else if (e.Property == IsBasicInfoChangedProperty)
        {
            if (!IsBasicInfoChanged)
            {
                _remarkOriginalValue = Remark;
                InternalRemarkModifyStatus = ModifyStatus.Normal;
            }
        }
    }
}