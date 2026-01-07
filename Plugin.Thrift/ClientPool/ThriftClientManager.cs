using Framework.Common;
using Plugin.AppEnv;
using Thrift;
using Thrift.Protocol;

namespace Plugin.Thrift;

internal class ThriftClientManager
{
    private readonly Dictionary<Type, IThriftClientPool> _pools = [];

    public void RegisterPool<T>(Func<TProtocol, T> clientFactory) where T : TBaseClient
    {
        if (_pools.ContainsKey(typeof(T)))
        {
            return;
        }
        
        if (!SystemConfig.ConnConfDict.TryGetValue(ConnConfType.Trade, out List<ConnConf>? connConfs))
        {
            return;
        }
        ConnConf? connConf = connConfs.FirstOrDefault(x => x.Name == Global.Get<IGlobalSetting>().SelectedConn);
        if (null == connConf)
        {
            return;
        }

        string serviceName = typeof(T).DeclaringType?.Name ?? typeof(T).Name;
        _pools.Add(typeof(T), new ThriftClientPool<T>(
            connConf.Host,
            connConf.Port,
            serviceName,
            clientFactory,
            SystemConfig.AppConf.ThriftPoolMaxSize,
            SystemConfig.AppConf.ThriftClientIdleTimeout)
        );
    }

    public void ClearPool()
    {
        foreach (IThriftClientPool pool in _pools.Values)
        {
            pool.Close();
        }
        _pools.Clear();
    }

    public ThriftClientPool<T>? GetPool<T>() where T : TBaseClient
    {
        if (!_pools.TryGetValue(typeof(T), out IThriftClientPool? pool))
        {
            return null;
        }
        
        return (ThriftClientPool<T>)pool;
    }
}
