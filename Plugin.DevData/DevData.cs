using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class DevData : IDevData
{
    private static string DevDataDir => Path.Combine(SystemConfig.AppConf.UserDataDir, "DevData");
    private static string DictSetsFilePath => Path.Combine(DevDataDir, "DictSets.xml");
    private static string ColumnsFilePath => Path.Combine(DevDataDir, "Columns.xml");
    private static string TableDataDir => Path.Combine(DevDataDir, "Tables");
    
    #region IPlugin
    public void OnStart()
    {
        DictSets.Clear();
        foreach (DictSet dictSet in ObjectHelper.FromXmlFile<List<DictSet>>(DictSetsFilePath))
        {
            DictSets.Add(dictSet.Id, dictSet);
        }
        
        Columns = ObjectHelper.FromXmlFile<List<ColumnInfo>>(ColumnsFilePath);
        
    }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn() { }

    public void OnLoggedOut() { }
    #endregion


    #region IDevData
    public Dictionary<string, DictSet> DictSets { get; } = [];
    public List<ColumnInfo> Columns { get; private set; } = [];
    public Dictionary<string, List<TableInfo>> Tables { get; } = [];
    #endregion
}