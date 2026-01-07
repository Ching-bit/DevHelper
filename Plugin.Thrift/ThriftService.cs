using Thrift;

namespace Plugin.Thrift;

public partial class ThriftService : IThriftService
{
    private readonly ThriftClientManager _manager = new();
    
    
    #region IThriftService
    public async Task DoServiceAsync<T>(Func<T, Task> func) where T : TBaseClient
    {
        ThriftClientPool<T>? clientPool = _manager.GetPool<T>();
        if (null == clientPool)
        {
            throw new NullReferenceException("Can't find Thrift client");
        }
        
        PooledClient<T> pooledClient = await clientPool.Get();
        T client = pooledClient.GetService();
        
        await func(client);
        
        clientPool.Return(pooledClient);
    }
    #endregion
    

    #region IPlugin
    public void OnStart() { }
    public void OnStop() { }

    public void OnLogin()
    {
        _manager.ClearPool();
        Init();
    }
    
    public void OnLoggedIn() { }

    public void OnLoggedOut()
    {
        _manager.ClearPool();
    }
    #endregion
    
}