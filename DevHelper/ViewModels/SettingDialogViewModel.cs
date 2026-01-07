using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using UniClient.Models;

namespace UniClient;

public partial class SettingDialogViewModel : ConfirmDialogViewModel
{
    public SettingDialogViewModel()
    {
        Setting.FromGlobalSetting();
    }
    
    [ObservableProperty] private SettingModel _setting = new();
}