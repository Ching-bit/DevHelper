using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;

namespace Control.Basic;

[WithDirectProperty(typeof(string), "Message")]
[WithDirectProperty(typeof(bool), "IsAutoClick", false)]
[WithDirectProperty(typeof(bool), "IsOkDefault", true)]
[WithDirectProperty(typeof(int), "AutoClickSeconds", 10)]
[WithDirectProperty(typeof(bool), "IsCancelButtonVisible", true)]
public partial class MessageDialog : UserControl
{
    public MessageDialog()
    {
        InitializeComponent();
    }
}