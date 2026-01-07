using Thrift;
using Thrift.Transport;

namespace Plugin.Thrift;

internal class PooledClient<T> where T : TBaseClient
{
    private readonly T _client;
    private readonly TTransport _transport;
    private readonly TimeSpan _idleTimeout;
    private DateTime _lastUsed;

    public PooledClient(T client, TTransport transport, TimeSpan idleTimeout)
    {
        _client = client;
        _transport = transport;
        _idleTimeout = idleTimeout;
        Touch();
    }

    public bool IsOpen()
    {
        if (DateTime.Now - _lastUsed > _idleTimeout)
        {
            try { _transport.Close(); } catch { }
            return false;
        }

        return _transport.IsOpen;
    }

    public async Task Open()
    {
        if (!_transport.IsOpen)
        {
            await _transport.OpenAsync();
        }
        
        if (_transport.IsOpen)
        {
            Touch();
        }
    }

    public void Close()
    {
        try { _transport.Close(); } catch { }
    }

    public T GetService()
    {
        Touch();
        return _client;
    }
    
    private void Touch()
    {
        _lastUsed = DateTime.Now;
    }
    
}
