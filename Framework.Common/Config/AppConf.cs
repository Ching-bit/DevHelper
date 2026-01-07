namespace Framework.Common;

public class AppConf
{
    #region Update
    public string UpdateAddrs { get; set; } = string.Empty;
    public string UpdateUsername { get; set; } = string.Empty;
    public string UpdatePassword { get; set; } = string.Empty;
    public string UpdateConfPath { get; set; } = string.Empty;
    public string UpdateExeName { get; set; } = string.Empty;
    public string UpdateCacheDir { get; set; } = string.Empty;
    #endregion


    #region Paths
    public string ConnConfPath { get; set; } = string.Empty;
    public string MenuConfPath { get; set; } = string.Empty;
    public string PluginConfPath { get; set; } = string.Empty;
    public string UserDataDir { get; set; } = string.Empty;
    public string LogPath { get; set; } = string.Empty;
    public bool CanAutoLogin { get; set; } = false;
    #endregion


    #region Thrift
    public string ThriftProtocol { get; set; } = string.Empty;
    public int ThriftPoolMaxSize { get; set; } = 10;
    public int ThriftClientIdleTimeout { get; set; } = 30;
    #endregion


    #region GRPC
    public string GrpcProtocol { get; set; } = "http";
    public int GrpcPooledConnectionIdleTimeout { get; set; } = 300;
    public int GrpcKeepAlivePingDelay { get; set; } = 30;
    public int GrpcKeepAlivePingTimeout { get; set; } = 10;
    public bool GrpcEnableMultipleHttp2Connections { get; set; } = true;
    public int? GrpcMaxReceiveMessageSize { get; set; } = 2 * 1024 * 1024;
    #endregion
}