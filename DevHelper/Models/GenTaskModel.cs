using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;
using Plugin.AppEnv;

namespace UniClient.Models;

public partial class GenTaskModel : UniModel
{
    public GenTaskModel(GenTask task, Avalonia.Controls.Control? view)
    {
        TaskName = task.TaskName;
        RecursionLevel = task.RecursionLevel;
        TemplateFile = task.TemplateFile;
        OutputFile = task.OutputFile;
        OutputDir = task.OutputDir;
        TemplateDir = task.TemplateDir;
        
        _view = view;
    }
    
    [ObservableProperty] private bool _shouldGenerate;
    [ObservableProperty] private RecursionLevel _recursionLevel;
    [ObservableProperty] private string _taskName = string.Empty;
    [ObservableProperty] private string _templateFile = string.Empty;
    [ObservableProperty] private string _outputFile = string.Empty;
    [ObservableProperty] private string _outputDir = string.Empty;
    [ObservableProperty] private string _templateDir = string.Empty;

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
        
        OutputDir = result[0].Path.AbsolutePath.Replace('/', Path.DirectorySeparatorChar);
        if (OutputDir.StartsWith(Global.Get<IAppEnv>().AppDir + Path.DirectorySeparatorChar))
        {
            OutputDir = OutputDir.Replace(Global.Get<IAppEnv>().AppDir, ".");
        }
    }

    public GenTask GetTask()
    {
        return new GenTask
        {
            TaskName = TaskName,
            RecursionLevel = RecursionLevel,
            TemplateFile = TemplateFile,
            OutputFile = OutputFile,
            OutputDir = OutputDir,
            TemplateDir = TemplateDir
        };
    }
    
}