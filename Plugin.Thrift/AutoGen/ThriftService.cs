namespace Plugin.Thrift;

public partial class ThriftService : IThriftService
{
    private void Init()
    {
        // UserService
        _manager.RegisterPool(protocol => new UserService.Client(protocol));
    }
}