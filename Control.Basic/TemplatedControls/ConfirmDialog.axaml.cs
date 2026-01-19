using System.Windows.Input;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Framework.Common;

namespace Control.Basic;

[WithDirectProperty(typeof(ICommand), "OkCommand", nullable: true)]
[WithDirectProperty(typeof(ICommand), "CancelCommand", nullable: true)]
[WithDirectProperty(typeof(object), "ReturnParameter", nullable: true)]
[WithDirectProperty(typeof(bool), "IsAutoClick", false)]
[WithDirectProperty(typeof(bool), "IsOkDefault", true)]
[WithDirectProperty(typeof(int), "AutoClickSeconds", 10)]
[WithDirectProperty(typeof(bool), "IsCancelButtonVisible", true)]
[WithStyledProperty(typeof(string), "OkButtonText", "")]
[WithStyledProperty(typeof(string), "CancelButtonText", "")]
public partial class ConfirmDialog : TemplatedControl
{
    private readonly DispatcherTimer _autoClickTimer;
    
    #region Constructors
    public ConfirmDialog()
    {
        _autoClickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        string okButtonText = ResourceHelper.FindStringResource("R_STR_OK", string.Empty);
        if (string.IsNullOrEmpty(okButtonText))
        {
            okButtonText = "OK";
        }
        OkButtonText = okButtonText;
        
        string cancelButtonText = ResourceHelper.FindStringResource("R_STR_CANCEL", string.Empty);
        if (string.IsNullOrEmpty(cancelButtonText))
        {
            cancelButtonText = "Cancel";
        }
        CancelButtonText = cancelButtonText;
            
        if (IsAutoClick)
        {
            AutoClickSeconds = Math.Max(0, AutoClickSeconds);
            
            _autoClickTimer.Tick += (_, _) =>
            {
                if (AutoClickSeconds > 0)
                {
                    AutoClickSeconds--;
                }

                if (AutoClickSeconds > 0)
                {
                    return;
                }
            
                ICommand? command = IsOkDefault ? OkCommand : CancelCommand;
                command?.Execute(null);
            };
        }
        _autoClickTimer.Start();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _autoClickTimer.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == DataContextProperty)
        {
            if (DataContext is ConfirmDialogViewModel viewModel)
            {
                viewModel.View = this;
                Bind(OkCommandProperty, new Binding("OkCommand"));
                Bind(CancelCommandProperty, new Binding("CancelCommand"));
            }
        }
    }
    #endregion
    

    #region Styled Properties
    [Content] public Avalonia.Controls.Control Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    public static readonly StyledProperty<Avalonia.Controls.Control> ContentProperty =
        AvaloniaProperty.Register<ConfirmDialog, Avalonia.Controls.Control>(nameof(Content));
    #endregion
}