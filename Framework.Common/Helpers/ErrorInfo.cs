namespace Framework.Common;

public class ErrorInfo
{
    public int ErrorCode { get; private init; }
    public string ErrorMessage { get; private init; } = string.Empty;

    public bool IsSuccess => ErrorCode == 0;
    
    public static ErrorInfo Success(string errorMessage = "")
    {
        return new ErrorInfo { ErrorCode = 0, ErrorMessage = errorMessage };
    }

    public static ErrorInfo Fail(string errorMessage, int errorCode = -1)
    {
        return new ErrorInfo { ErrorCode = errorCode, ErrorMessage = errorMessage };
    }
}
