using Framework.Common;
using Framework.Utils.Helpers;
using NLog;
using NLog.Layouts;

namespace Plugin.Log;

internal class Log : ILog
{
    #region IPlugin
    public void OnStart()
    {
        LogManager.Setup().LoadConfiguration(builder => {
            builder.ForLogger(loggerNamePattern: "*").FilterMinLevel(LogLevel.Trace).WriteToFile(
                archiveAboveSize: 1024 * 1024 * 100,    // 100M
                fileName: SystemConfig.AppConf.LogPath.Replace("{YYYYMMDD}", DateTimeHelper.Today()),
                layout: Layout.FromString(@"[${date:format=yyyyMMdd HH\:mm\:ss.fff}] [${level:uppercase=true}][T=${threadid}][${logger}] ${message}"));
        });
    }

    public void OnStop()
    {
        LogManager.Shutdown();
    }

    public void OnLogin() { }
    
    public void OnLoggedIn() { }

    public void OnLoggedOut() { }
    #endregion


    #region ILogger
    public void Trace(string module, string message, params object?[] args)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Trace(message, args);
    }

    public void Debug(string module, string message, params object?[] args)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Debug(message, args);
    }

    public void Info(string module, string message, params object?[] args)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Info(message, args);
    }

    public void Warn(string module, string message, params object?[] args)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Warn(message, args);
    }

    public void Error(string module, string message, params object?[] args)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Error(message, args);
    }

    public void Error(string module, Exception ex)
    {
        Logger logger = LogManager.GetLogger(module);
        logger.Error(ex);
        if (null != ex.StackTrace)
        {
            logger.Error(ex.StackTrace);
        }
    }
    #endregion
}