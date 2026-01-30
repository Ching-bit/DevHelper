using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Menu.DevData;
using Plugin.AppEnv;
using UniClient.Models;

namespace UniClient;

public partial class HomeViewModel : UniViewModel
{
    public override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        LoadGenTasks();
    }
    
    private void LoadGenTasks()
    {
        foreach (GenTask task in Global.Get<IUserSetting>().GenTasks)
        {
            GenTasks.Add(new GenTaskModel(task, View));
        }
    }

    [ObservableProperty] private ObservableCollection<GenTaskModel> _genTasks = [];
    
    [RelayCommand]
    private async Task SystemSetting()
    {
        ConfirmDialogResult result = await ConfirmDialog.Show<SystemSettingDialog, SystemSettingDialogViewModel>();
        if (!result.IsConfirmed || result.ReturnParameter is not SystemSettingModel systemSettingModel)
        {
            return;
        }
        
        systemSettingModel.ToUserSetting();
        Global.Get<IUserSetting>().Save();
    }

    [RelayCommand]
    private async Task Generate()
    {
        List<GenTaskModel> tasks = GenTasks.Where(x => x.ShouldGenerate).ToList();
        foreach (GenTaskModel task in tasks)
        {
            switch (task.TaskType)
            {
                case GenTaskType.TableDocument:
                    CodeGenerator.GenerateTableDocument(task.GeneratingDir);
                    break;
                case GenTaskType.TableScripts:
                    CodeGenerator.GenerateTableScripts(task.GeneratingDir);
                    break;
                case GenTaskType.UserDefined:
                    CodeGenerator.GenerateUserDefined(task.GeneratingDir);
                    break;
                default:
                    await MessageDialog.Show($"The task type [{task.TaskType}] is not supported yet.");
                    break;
            }
        }
    }
    
}