using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;
using Plugin.DevData;

namespace Menu.DevData;

public partial class DatabaseSelectionDialogViewModel : ConfirmDialogViewModel
{
    public DatabaseSelectionDialogViewModel()
    {
        DatabaseList = [];
        foreach (DatabaseInfo databaseInfo in Global.Get<IDevData>().GetAllDatabases())
        {
            DatabaseList.Add(databaseInfo.Name);
        }

        TargetDatabases = [];
    }
    
    [ObservableProperty] private ObservableCollection<string> _databaseList;
    [ObservableProperty] private ObservableCollection<string> _targetDatabases;
}