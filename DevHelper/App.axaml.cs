using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Framework.Common;
using Plugin.AppEnv;
using Plugin.Log;

namespace UniClient;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Global.InitPlugins();
        Global.StartPlugins();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new LoginWindow();
            desktop.Exit += (sender, args) =>
            {
                Global.Get<ILog>().Info(LogModule.FRAMEWORK, "App exit");
                Global.StopPlugins();
            };
        }
        
        base.OnFrameworkInitializationCompleted();
        Global.Get<ILog>().Info(LogModule.FRAMEWORK, "App started");
    }
    
}