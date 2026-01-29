using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Irihi.Avalonia.Shared.Helpers;

namespace Control.Basic;

[WithAttachedProperty(typeof(CheckBox), typeof(object), "CheckedContent", nullable: true)]
[WithAttachedProperty(typeof(CheckBox), typeof(object), "UncheckedContent", nullable: true)]
[WithAttachedProperty(typeof(CheckBox), typeof(bool), "InternalIsHooked", false)]
public partial class CheckBoxHelper
{
    static CheckBoxHelper()
    {
        CheckedContentProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is CheckBox cb) { EnsureHook(cb); }
        });

        UncheckedContentProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is CheckBox cb) { EnsureHook(cb); }
        });
    }
    
    private static void EnsureHook(CheckBox cb)
    {
        if (cb.GetValue(InternalIsHookedProperty))
            return;

        cb.SetValue(InternalIsHookedProperty, true);
        cb.GetObservable(ToggleButton.IsCheckedProperty).Subscribe(_ => UpdateContent(cb));
        
        cb.GetObservable(CheckedContentProperty).Subscribe(_ => UpdateContent(cb));
        cb.GetObservable(UncheckedContentProperty).Subscribe(_ => UpdateContent(cb));
        
        UpdateContent(cb);
    }

    private static void UpdateContent(CheckBox cb)
    {
        object? chosen = true == cb.IsChecked ? cb.GetValue(CheckedContentProperty) : cb.GetValue(UncheckedContentProperty);
        if (chosen != null)
        {
            cb.Content = chosen;
        }
    }
}