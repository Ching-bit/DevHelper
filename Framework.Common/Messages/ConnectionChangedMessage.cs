using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Framework.Common;

public class ConnectionChangedMessage : ValueChangedMessage<ConnConf>
{
    public ConnectionChangedMessage(ConnConf value) : base(value)
    {
    }
}