namespace Plugin.Grpc;

public partial class GrpcService
{
    private void Init()
    {
        // UserService
        _clients.Add(typeof(UserService.UserServiceClient), new UserService.UserServiceClient(_channel));
    }
}