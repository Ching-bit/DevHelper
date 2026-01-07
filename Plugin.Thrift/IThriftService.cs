using Framework.Common;
using Thrift;

namespace Plugin.Thrift;

public interface IThriftService : IPlugin
{
    public Task DoServiceAsync<T>(Func<T, Task> func) where T : TBaseClient;
}