using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Framework.Common;

public class LanguageChangedMessage : ValueChangedMessage<string>
{
    public LanguageChangedMessage(string value) : base(value)
    {
    }
}