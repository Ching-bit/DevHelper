namespace Framework.Common;

public class UniMenu : UniPanel
{
    public MenuConf? MenuConf { get; set; }

    public virtual void OnInit()
    {
        if (DataContext is UniViewModel vm)
        {
            vm.OnMenuInit();
        }
    }

    public virtual void OnShown()
    {
        if (DataContext is UniViewModel vm)
        {
            vm.OnMenuShown();
        }
    }

    public virtual void OnHidden()
    {
        if (DataContext is UniViewModel vm)
        {
            vm.OnMenuHidden();
        }
    }

    public virtual void OnClosing()
    {
        if (DataContext is UniViewModel vm)
        {
            vm.OnMenuClosing();
        }
    }

    public virtual void OnClosed()
    {
        if (DataContext is UniViewModel vm)
        {
            vm.OnMenuClosed();
        }
    }
}