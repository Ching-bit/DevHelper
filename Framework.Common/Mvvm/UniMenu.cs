namespace Framework.Common;

public class UniMenu : UniPanel
{
    public MenuConf? MenuConf { get; set; }
    
    public virtual void OnInit() { }
    public virtual void OnShown() { }
    public virtual void OnHidden() { }
    public virtual void OnClosing() { }
    public virtual void OnClosed() { }
}