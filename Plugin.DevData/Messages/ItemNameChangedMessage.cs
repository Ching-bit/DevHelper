using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Plugin.DevData.Messages;

public class ItemNameChangedMessage : ValueChangedMessage<ItemNameChangedMessage.Args>
{
    public class Args
    {
        public Args(IFileNode item, string originalName, string originalDescription)
        {
            Item = item;
            OriginalName = originalName;
            OriginalDescription = originalDescription;
        }
        
        public IFileNode Item { get; }
        public string OriginalName { get; }
        public string OriginalDescription { get; }
    }

    public ItemNameChangedMessage(Args value) : base(value)
    {
    }
}