using Framework.Common;
using Grpc.Core;

namespace Plugin.Grpc;

public interface IGrpcService : IPlugin
{
    public T GetService<T>() where T : ClientBase;
}