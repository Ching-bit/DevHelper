using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Framework.Common;
using Plugin.AppEnv;
using Plugin.Log;

namespace UniClient;

public partial class LoginWindow : UniWindow
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // do after the client updated
        BackgroundWorker worker = new();
        string? initParams = null;
        worker.DoWork += (_, _) =>
        {
            try
            {
                string pipeName = Global.Get<IAppEnv>().AppName;
                using NamedPipeServerStream server = new(pipeName);
                Task connTask = server.WaitForConnectionAsync();
                if (!connTask.Wait(5 * 1000))
                {
                    return;
                }

                using StreamReader reader = new(server);
                initParams = reader.ReadLine();
            }
            catch (Exception ex)
            {
                Global.Get<ILog>().Error(LogModule.FRAMEWORK, ex);
            }
        };
        worker.RunWorkerCompleted += (_, _) =>
        {
            if (null != initParams)
            {
                UpdateInterParameters parameters = UpdateInterParameters.FromCommandLine(initParams);

                Global.Get<ILog>().Debug(LogModule.FRAMEWORK, "Autologin with cache");
                if (LoginView.DataContext is not LoginViewModel loginViewModel)
                {
                    Global.Get<ILog>().Error(LogModule.FRAMEWORK, "MainWindow is invalid");
                    return;
                }

                loginViewModel.Username = parameters.Username;
                loginViewModel.Password = parameters.Password;
                loginViewModel.SkipUpdate = parameters.SkipUpdate;
                loginViewModel.LoginCommand.Execute(null);
            }
    
            Global.Get<ILog>().Debug(LogModule.FRAMEWORK, "Pipe end");
        };
        worker.RunWorkerAsync();
    }
}