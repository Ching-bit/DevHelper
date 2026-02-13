using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Plugin.DevData.Messages;

public class ColumnsChangedMessage : ValueChangedMessage<List<ColumnInfo>>
{
    public ColumnsChangedMessage(List<ColumnInfo> value) : base(value)
    {
    }
}