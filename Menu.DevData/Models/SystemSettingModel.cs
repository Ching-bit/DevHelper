using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;
using Plugin.AppEnv;

namespace Menu.DevData;

public partial class SystemSettingModel : UniModel
{
    [ObservableProperty] private string _historyDatabaseName = string.Empty;
    [ObservableProperty] private string _historyTableName = string.Empty;
    [ObservableProperty] private string _archiveDateColumnName = string.Empty;

    public void FromUserSetting()
    {
        HistoryDatabaseName = Global.Get<IUserSetting>().HistoryDatabaseName;
        HistoryTableName = Global.Get<IUserSetting>().HistoryTableName;
        ArchiveDateColumnName = Global.Get<IUserSetting>().ArchiveDateColumnName;
    }

    public void ToUserSetting()
    {
        Global.Get<IUserSetting>().HistoryDatabaseName = HistoryDatabaseName;
        Global.Get<IUserSetting>().HistoryTableName = HistoryTableName;
        Global.Get<IUserSetting>().ArchiveDateColumnName = ArchiveDateColumnName;
    }
}