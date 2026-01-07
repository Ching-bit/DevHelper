using System.Collections.Concurrent;
using System.Net.Sockets;
using Framework.Common;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;
using Thrift.Transport.Client;

namespace Plugin.Thrift;

internal class ThriftClientPool<T> : IThriftClientPool where T : TBaseClient
{
    private readonly ConcurrentBag<PooledClient<T>> _clients = [];
    private readonly string _host;
    private readonly int _port;
    private readonly string _serviceName;
    private readonly Func<TProtocol, T> _clientFactory;
    private readonly int _maxSize;
    private readonly TimeSpan _idleTimeout;

    public ThriftClientPool(string host, int port, string serviceName, Func<TProtocol, T> clientFactory, int maxSize = 10, int idleSeconds = 60)
    {
        _host = host;
        _port = port;
        _serviceName = serviceName;
        _clientFactory = clientFactory;
        _maxSize = maxSize;
        _idleTimeout = TimeSpan.FromSeconds(idleSeconds);
    }

    private async Task<PooledClient<T>> CreateClient()
    {
        TSocketTransport transport = new(new TcpClient(_host, _port), new TConfiguration());
        TBufferedTransport bufferedTransport = new(transport);

        if (!Enum.TryParse(SystemConfig.AppConf.ThriftProtocol, out ThriftProtocol thriftProtocol))
        {
            thriftProtocol = ThriftProtocol.Binary;
        }
        TProtocol protocol = thriftProtocol switch
        {
            ThriftProtocol.Binary => new TBinaryProtocol(bufferedTransport),
            ThriftProtocol.Compact => new TCompactProtocol(bufferedTransport),
            ThriftProtocol.Json => new TJsonProtocol(bufferedTransport),
            _ => new TBinaryProtocol(bufferedTransport)
        };

        TMultiplexedProtocol multiplexedProtocol = new(protocol, _serviceName);
        T client = _clientFactory(multiplexedProtocol);
        
        PooledClient<T> pooledClient = new PooledClient<T>(client, bufferedTransport, _idleTimeout);
        await pooledClient.Open();
        
        return pooledClient;
    }

    public async Task<PooledClient<T>> Get()
    {
        while (_clients.TryTake(out PooledClient<T>? client))
        {
            if (!client.IsOpen())
            {
                continue;
            }
            
            return client;
        }
        
        return await CreateClient();
    }

    public void Return(PooledClient<T> pooled)
    {
        if (_clients.Count < _maxSize)
        {
            _clients.Add(pooled);
        }
        else
        {
            pooled.Close();
        }
    }

    public void Close()
    {
        foreach (PooledClient<T> client in _clients)
        {
            client.Close();
        }
        _clients.Clear();
    }
}
