using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Framework.Common;
using Plugin.AppEnv;

namespace UniClient.Models;

public partial class GenTaskModel : UniModel
{
    public GenTaskModel(GenTask task, Avalonia.Controls.Control? view)
    {
        TaskType = task.TaskType;
        TaskNameResource = task.TaskNameResource;
        TaskName = ResourceHelper.FindStringResource(TaskNameResource, TaskNameResource);
        GeneratingDir = task.OutputDir;
        
        _view = view;
        
        // Register for language change message to update the task name
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (_, _) =>
        {
            TaskName = ResourceHelper.FindStringResource(TaskNameResource, TaskNameResource);
        });
    }
    
    public GenTaskType TaskType { get; }
    private string TaskNameResource { get; }
    
    [ObservableProperty] private bool _shouldGenerate;
    [ObservableProperty] private string _taskName = string.Empty;
    [ObservableProperty] private string _generatingDir = string.Empty;

    private readonly Avalonia.Controls.Control? _view;

    [RelayCommand]
    private async Task PickFolder()
    {
        var topLevel = TopLevel.GetTopLevel(_view);
        var sp = topLevel?.StorageProvider;
        if (sp is null)
        {
            return;
        }
        
        IReadOnlyList<IStorageFolder> result = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });
        if (result.Count <= 0)
        {
            return;
        }
        
        GeneratingDir = result[0].Path.AbsolutePath.Replace('/', Path.DirectorySeparatorChar);
        if (GeneratingDir.StartsWith(Global.Get<IAppEnv>().AppDir + Path.DirectorySeparatorChar))
        {
            GeneratingDir = GeneratingDir.Replace(Global.Get<IAppEnv>().AppDir, ".");
        }
    }
    
}