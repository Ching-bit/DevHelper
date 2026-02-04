using Framework.Common;

namespace Plugin.AppEnv;

public interface IUserSetting : IPlugin
{
    public string HistoryDatabaseName { get; set; }
    public string HistoryTableName { get; set; }
    public string ArchiveDateColumnName { get; set; }
    public List<GenTaskConf> GenTaskConfs { get; set; }
    
    public void Save();
}