using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Menu.DevData;
using Plugin.AppEnv;
using Plugin.DevData;

namespace UniClient.Models;

public partial class GenTaskModel : UniModel
{
    public GenTaskModel(GenTask task, Avalonia.Controls.Control? view)
    {
        ShouldGenerate = true;
        TaskName = task.TaskName;
        RecursionLevel = task.RecursionLevel;
        TemplateFile = task.TemplateFile;
        OutputFile = task.OutputFile;
        ProgramLanguage = task.ProgramLanguage;
        IsUsingString = task.IsUsingString;
        DatabaseType = task.DatabaseType;
        IsIncludingHistoryDatabases = task.IsIncludingHistoryDatabases;
        RpcType = task.RpcType;
        
        TargetDatabases = [];
        foreach (string databaseName in task.TargetDatabases)
        {
            if (Global.Get<IDevData>().GetAllDatabases().Select(x => x.Name).Contains(databaseName))
            {
                TargetDatabases.Add(databaseName);
            }
        }
        OutputDir = task.OutputDir;
        
        TemplateDir = task.TemplateDir;
        
        _view = view;
    }
    
    [ObservableProperty] private bool _shouldGenerate;
    [ObservableProperty] private string _taskName = string.Empty;
    [ObservableProperty] private RecursionLevel _recursionLevel;
    [ObservableProperty] private string _templateFile = string.Empty;
    [ObservableProperty] private string _outputFile = string.Empty;
    [ObservableProperty] private ProgramLanguage _programLanguage;
    [ObservableProperty] private bool _isUsingString;
    [ObservableProperty] private DatabaseType _databaseType;
    [ObservableProperty] private bool _isIncludingHistoryDatabases;
    [ObservableProperty] private RpcType _rpcType;
    
    [ObservableProperty] private string _outputDir = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _targetDatabases;
    
    [ObservableProperty] private string _templateDir = string.Empty;

    private readonly Avalonia.Controls.Control? _view;

    [RelayCommand]
    private async Task PickFolder()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(_view);
        IStorageProvider? sp = topLevel?.StorageProvider;
        if (null == sp)
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

        GenTaskConf? conf = Global.Get<IUserSetting>().GenTaskConfs.FirstOrDefault(x => x.TemplateDir.Equals(TemplateDir));
        if (null == conf)
        {
            conf = new GenTaskConf() { TemplateDir = TemplateDir };
            Global.Get<IUserSetting>().GenTaskConfs.Add(conf);
        }

        conf.OutputDir = OutputDir;
        Global.Get<IUserSetting>().Save();
    }

    [RelayCommand]
    private async Task PickDatabase()
    {
        ConfirmDialogResult result =
            await ConfirmDialog.Show<DatabaseSelectionDialog, DatabaseSelectionDialogViewModel>();
        if (!result.IsConfirmed || result.ReturnParameter is not ObservableCollection<string> targetDatabases)
        {
            return;
        }

        TargetDatabases = targetDatabases;

        GenTaskConf? conf = Global.Get<IUserSetting>().GenTaskConfs.FirstOrDefault(x => x.TemplateDir.Equals(TemplateDir));
        if (null == conf)
        {
            return;
        }

        conf.TargetDatabases = TargetDatabases.ToList();
        Global.Get<IUserSetting>().Save();
    }

    public GenTask GetTask()
    {
        return new GenTask
        {
            TaskName = TaskName,
            RecursionLevel = RecursionLevel,
            TemplateFile = TemplateFile,
            OutputFile = OutputFile,
            ProgramLanguage = ProgramLanguage,
            IsUsingString = IsUsingString,
            DatabaseType = DatabaseType,
            IsIncludingHistoryDatabases = IsIncludingHistoryDatabases,
            RpcType = RpcType,
            
            OutputDir = OutputDir,
            TargetDatabases = TargetDatabases.ToList(),
            
            TemplateDir = TemplateDir
        };
    }
    
}