using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Framework.Common;

public class UniViewModel : ObservableObject
{
    public Control? View { get; set; }
    
    public virtual void OnLoaded(object? sender, RoutedEventArgs e)
    {
        RefreshNotificationManager();
    }

    
    #region Lifecycle Functions for UniMenu
    public virtual void OnMenuInit() { }
    public virtual void OnMenuShown() { }
    public virtual void OnMenuHidden() { }
    public virtual void OnMenuClosing() { }
    public virtual void OnMenuClosed() { }
    #endregion
    
    
    protected void ShowNotification(string message, NotificationType notificationType)
    {
        Notification notification = new()
        {
            Message = ResourceHelper.FindStringResource(message)
        };
        _notificationManager?.Show(notification, type: notificationType);
    }

    public void RefreshNotificationManager(TopLevel? host = null)
    {
        host ??= TopLevel.GetTopLevel(View);

        if (_lastTopLevel != host)
        {
            _lastTopLevel = host;
            _notificationManager = new WindowNotificationManager(_lastTopLevel)
            {
                Position = NotificationPosition.TopCenter,
                MaxItems = 1
            };
        }
    }
    
    private WindowNotificationManager? _notificationManager;
    private TopLevel? _lastTopLevel;
}