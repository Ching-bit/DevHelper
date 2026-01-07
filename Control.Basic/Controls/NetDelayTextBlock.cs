using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace Control.Basic;

public class NetDelayTextBlock : TextBlock
{
    #region Constructors
    public NetDelayTextBlock()
    {
        Bind(
            TextProperty,
            new Binding("Delay")
            {
                Source = this,
                Converter = new NetDelayToTextConverter()
            });

        MultiBinding multiBinding = new() { Converter = new NetDelayToColorConverter() };
        multiBinding.Bindings.Add(new Binding("Delay") { Source = this });
        multiBinding.Bindings.Add(new Binding("WarnThreshold") { Source = this });
        multiBinding.Bindings.Add(new Binding("ErrorThreshold") { Source = this });
        Bind(ForegroundProperty, multiBinding);
    }
    #endregion


    #region Dependency Properties
    
    public long? Delay
    {
        get => GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }
    public static readonly StyledProperty<long?> DelayProperty =
        AvaloniaProperty.Register<NetDelayTextBlock, long?>(nameof(Delay));
    
    public long? WarnThreshold
    {
        get => GetValue(WarnThresholdProperty);
        set => SetValue(WarnThresholdProperty, value);
    }
    public static readonly StyledProperty<long?> WarnThresholdProperty =
        AvaloniaProperty.Register<NetDelayTextBlock, long?>(nameof(WarnThreshold));
    
    public long? ErrorThreshold
    {
        get => GetValue(ErrorThresholdProperty);
        set => SetValue(ErrorThresholdProperty, value);
    }
    public static readonly StyledProperty<long?> ErrorThresholdProperty =
        AvaloniaProperty.Register<NetDelayTextBlock, long?>(nameof(ErrorThreshold));
    #endregion


    #region Hide Properties
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public new string? Text
    {
        get => base.Text;
        set => base.Text = value;
    }

    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public new IBrush? Foreground
    {
        get => base.Foreground;
        set => base.Foreground = value;
    }
    #endregion
}