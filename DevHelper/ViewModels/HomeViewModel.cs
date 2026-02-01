using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils.Helpers;
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
        string templateDir = Path.Combine(Global.Get<IAppEnv>().AppDir, "templates");
        string[] templateFolders = Directory.GetDirectories(templateDir);
        foreach (string folder in templateFolders)
        {
            string taskConfPath = Path.Combine(folder, "GenTask.xml");
            if (!File.Exists(taskConfPath)) { continue; }

            GenTask task = ObjectHelper.FromXmlFile<GenTask>(taskConfPath);
            task.TemplateDir = folder;
            GenTasks.Add(new GenTaskModel(task, View));
        }
    }

    [ObservableProperty] private ObservableCollection<GenTaskModel> _genTasks = [];
    [ObservableProperty] private string _logs = string.Empty;
    
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
        if (tasks.Count <= 0)
        {
            await MessageDialog.Show("R_STR_SELECT_EMPTY_TASK_NOTICE");
            return;
        }

        Logs = string.Empty;
        foreach (GenTaskModel taskModel in tasks)
        {
            try
            {
                PrintLog(ResourceHelper.FindStringResource("R_STR_START_TO_RUN_TASK").Replace("#1", taskModel.TaskName));
                CodeGenerator.GenFile(taskModel.GetTask());
                PrintLog(ResourceHelper.FindStringResource("R_STR_TASK_FINISHED").Replace("#1", taskModel.TaskName));
            }
            catch (Exception e)
            {
                PrintLog(e.StackTrace ?? e.Message);
            }
        }
    }

    private void PrintLog(string log)
    {
        BackgroundWorker worker = new();
        worker.DoWork += (_, _) =>
        {
            Logs += $"{Environment.NewLine}{log}";
        };
        worker.RunWorkerAsync();
    }
}