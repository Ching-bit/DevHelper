using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Plugin.AppEnv;
using Plugin.Log;
using UniClient.Domains;
using Ursa.Controls;

namespace UniClient;

public partial class LoginViewModel : UniViewModel
{
    #region Constructors
    public LoginViewModel()
    {
        Username = string.Empty;
        Password = string.Empty;
        ErrMsg = string.Empty;
        IsLogining = false;
        SkipUpdate = false;
        IsUpdating = false;

        CanAutoLogin = SystemConfig.AppConf.CanAutoLogin;
        IsRememberAccount = Global.Get<IGlobalSetting>().IsRememberAccount;
        IsAutoLogin = Global.Get<IGlobalSetting>().IsAutoLogin;

        LastUsers = [];
        foreach (UserInfo userInfo in Global.Get<IGlobalSetting>().LastUsers)
        {
            LastUsers.Add(new UserInfo
            {
                Username = userInfo.Username,
                Password = IsAutoLogin ? userInfo.Password : string.Empty,
            });
        }
        LastUsers.CollectionChanged += (_, _) =>
        {
            Global.Get<IGlobalSetting>().LastUsers.Clear();
            Global.Get<IGlobalSetting>().LastUsers.AddRange(LastUsers);
            Global.Get<IGlobalSetting>().Save();
        };

        if (IsRememberAccount && LastUsers.Count > 0)
        {
            Username = LastUsers[0].Username;
            if (IsAutoLogin)
            {
                Password = LastUsers[0].Password;
            }
        }

        // auto-login
        if (CanAutoLogin && IsAutoLogin)
        {
            BackgroundWorker worker = new();
            worker.DoWork += (_, _) =>
            {
                try
                {
                    using NamedPipeClientStream client = new(Global.Get<IAppEnv>().AppName);
                    client.Connect(5 * 1000);
                    if (!client.IsConnected)
                    {
                        Global.Get<ILog>().Error(LogModule.FRAMEWORK, "Auto login error: can't connect to the pipe");
                        return;
                    }

                    UpdateInterParameters updateInterParameters = new()
                    {
                        ExeName = Global.Get<IAppEnv>().AppName,
                        UpdateCacheDir = SystemConfig.AppConf.UpdateCacheDir,
                        ClientDir = ".\\",
                        Username = Username,
                        Password = Password,
                        SkipUpdate = false,
                    };

                    using StreamWriter steamWriter = new(client);
                    steamWriter.WriteLine(updateInterParameters.ToCommandLine());
                    steamWriter.Flush();
                } catch { }
            };
            worker.RunWorkerAsync();
        }
    }
    #endregion


    #region Properties
    [ObservableProperty] private ObservableCollection<UserInfo> _lastUsers;
    [ObservableProperty] private bool _canAutoLogin;
    [ObservableProperty] private bool _isAutoLogin;
    [ObservableProperty] private bool _isRememberAccount;
    [ObservableProperty] private string _username;
    [ObservableProperty] private string _password;
    [ObservableProperty] private string _errMsg;
    [ObservableProperty] private bool _isLogining;
    [ObservableProperty] private bool _isUpdating;
    [ObservableProperty] private int _updateFileCount;
    [ObservableProperty] private int _downloadedFileCount;
    
    public bool SkipUpdate { get; set; }
    
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (nameof(IsRememberAccount) == e.PropertyName)
        {
            if (CanAutoLogin && !IsRememberAccount)
            {
                IsAutoLogin = false;
            }

            Global.Get<IGlobalSetting>().IsRememberAccount = IsRememberAccount;
            Global.Get<IGlobalSetting>().Save();
        }
        else if (nameof(IsAutoLogin) == e.PropertyName)
        {
            if (CanAutoLogin && IsAutoLogin)
            {
                IsRememberAccount = true;
            }

            Global.Get<IGlobalSetting>().IsAutoLogin = IsAutoLogin;
            Global.Get<IGlobalSetting>().Save();
        }
    }
    #endregion
    
    
    [RelayCommand]
    private async Task Login()
    {
        Global.LoginPlugins();
        
        ErrMsg = string.Empty;
        IsLogining = true;
        IsUpdating = false;

        BackgroundWorker worker = new();
        
        ErrorInfo loginResponse = new();
        await OnLoginResponse(loginResponse);
    }
    
    private async Task OnLoginResponse(ErrorInfo response)
    {
        try
        {
            if (0 != response.ErrorCode)
            {
                ErrMsg = response.ErrorMessage;
                return;
            }

            // update AppEnv plugin
            Global.Get<IAppEnv>().User = new()
            {
                Username = Username,
                Password = Password,
            };
            Global.Get<ILog>().Info(LogModule.FRAMEWORK, "User {Username} logged in", Username);

            if (IsRememberAccount)
            {
                for (int i = LastUsers.Count - 1; i >= 0; --i)
                {
                    if (LastUsers[i].Username == Username)
                    {
                        LastUsers.RemoveAt(i);
                    }
                }
                LastUsers.Insert(0, new()
                {
                    Username = Username,
                    Password = IsAutoLogin ? Password : string.Empty,
                });
            }
            
            // Update
            if (!SkipUpdate)
            {
                if (!await Updater.UpdateAsync(
                        SystemConfig.AppConf.UpdateAddrs,
                        SystemConfig.AppConf.UpdateUsername,
                        SystemConfig.AppConf.UpdatePassword,
                        this))
                {
                    if (!await MessageDialog.Show("R_STR_UPDATE_FAILED_NOTICE", isAutoClick: true))
                    {
                        Global.Get<IAppEnv>().User = null;
                        return;
                    }
                }
            }

            Global.LoggedInPlugins();
            UniWindow.NavigateTo<MainWindow>(this);
        }
        finally
        {
            IsLogining = false;
            IsUpdating = false;
        }
    }
}