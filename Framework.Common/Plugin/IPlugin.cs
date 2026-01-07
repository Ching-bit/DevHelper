namespace Framework.Common;

public interface IPlugin
{
    public void OnStart();
    public void OnStop();
    public void OnLogin();
    public void OnLoggedIn();
    public void OnLoggedOut();
}