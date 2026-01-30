using Framework.Common;

namespace Plugin.AppEnv;

public interface IUserSetting : IPlugin
{
    public string DatabaseType { get; set; }
    public string HistoryDatabaseName { get; set; }
    public string HistoryTableName { get; set; }
    public string ArchiveDateColumnName { get; set; }
    
    public List<GenTask> GenTasks { get; set; }
    
    public void Save();
}