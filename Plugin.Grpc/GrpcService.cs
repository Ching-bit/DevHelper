using Framework.Common;
using Grpc.Core;
using Grpc.Net.Client;
using Plugin.AppEnv;

namespace Plugin.Grpc;

public partial class GrpcService : IGrpcService
{
    private GrpcChannel? _channel;
    private readonly Dictionary<Type, ClientBase> _clients = [];


    private void Shutdown()
    {
        if (_channel != null)
        {
            _channel.ShutdownAsync().Wait();
            _channel = null;
        }

        _clients.Clear();
    }

    #region IGrpcService
    public T GetService<T>() where T : ClientBase
    {
        if (!_clients.TryGetValue(typeof(T), out ClientBase? client))
        {
            throw new ArgumentException("Cannot get the GRPC client");
        }
        
        return (T)client;
    }
    #endregion
    
    #region IPlugin
    public void OnStart() { }

    public void OnStop()
    {
        Shutdown();
    }

    public void OnLogin()
    {
        SocketsHttpHandler httpHandler = new()
        { 
            PooledConnectionIdleTimeout =  TimeSpan.FromSeconds(SystemConfig.AppConf.GrpcPooledConnectionIdleTimeout),
            KeepAlivePingDelay = TimeSpan.FromSeconds(SystemConfig.AppConf.GrpcKeepAlivePingDelay),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(SystemConfig.AppConf.GrpcKeepAlivePingTimeout),
            EnableMultipleHttp2Connections = SystemConfig.AppConf.GrpcEnableMultipleHttp2Connections
        };
        
        ConnConf connConf = Global.Get<IGlobalSetting>().GetSelectedTradeConnection();
        string address = SystemConfig.AppConf.GrpcProtocol + "://" + connConf.Address;
        
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            MaxReceiveMessageSize = SystemConfig.AppConf.GrpcMaxReceiveMessageSize
        });

        _clients.Clear();
        Init();
    }

    public void OnLoggedIn() { }

    public void OnLoggedOut()
    {
        Shutdown();
    }
    #endregion
    
}