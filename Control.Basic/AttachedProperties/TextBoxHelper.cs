using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Control.Basic;

[WithAttachedProperty(typeof(TextBox), typeof(string), "BlacklistChars")]
[WithAttachedProperty(typeof(TextBox), typeof(string), "WhitelistChars", nullable: true)]
public partial class TextBoxHelper
{
    static TextBoxHelper()
    {
        BlacklistCharsProperty.Changed.AddClassHandler<TextBox>((tb, _) =>
        {
            tb.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        });
        WhitelistCharsProperty.Changed.AddClassHandler<TextBox>((tb, _) =>
        {
            tb.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
        });
    }
    
    private static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is not TextBox tb) { return; }
        
        string blacklistChars = GetBlacklistChars(tb);
        if (null != e.Text && e.Text.Any(c => blacklistChars.Contains(c)))
        {
            e.Handled = true;
        }

        string? whitelistChars = GetWhitelistChars(tb);
        if (!string.IsNullOrEmpty(whitelistChars) && null != e.Text && e.Text.Any(c => !whitelistChars.Contains(c)))
        {
            e.Handled = true;
        }
    }

}