namespace Framework.Services;

public class LoginService
{
    public class Request
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class Response
    {
        public Response()
        {
            ErrCode = 0;
            ErrMsg = string.Empty;
        }

        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }
    }

    public static Response Call(Request request)
    {
        // TODO
        return new() { ErrCode = 0, ErrMsg = "" };
    }
}