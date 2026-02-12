using Avalonia.Controls;
using Avalonia.Data;

namespace Control.Basic;

public class DataGridBindableTextColumn : DataGridTextColumn
{
    public IBinding? ForegroundBinding { get; set; }
    
    protected override Avalonia.Controls.Control GenerateElement(DataGridCell cell, object dataItem)
    {
        TextBlock tb = (TextBlock)base.GenerateElement(cell, dataItem);
        if (ForegroundBinding != null)
        {
            tb.Bind(TextBlock.ForegroundProperty, ForegroundBinding);
        }
        
        return tb;
    }
}
