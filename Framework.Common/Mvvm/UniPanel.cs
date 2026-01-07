using Avalonia.Controls;

namespace Framework.Common;

public class UniPanel : UserControl
{
    protected UniPanel()
    {
        _ = ViewModelLocator.SetViewModel(this);
    }
}