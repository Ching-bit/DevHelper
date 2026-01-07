using Framework.Common;

namespace Plugin.Log;

public interface ILog : IPlugin
{
    public void Trace(string module, string message, params object?[] args);

    public void Debug(string module, string message, params object?[] args);

    public void Info(string module, string message, params object?[] args);

    public void Warn(string module, string message, params object?[] args);

    public void Error(string module, string message, params object?[] args);

    public void Error(string module, Exception ex);
}