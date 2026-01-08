using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.DevData;

public class DevData : IDevData
{
    private static string DevDataDir => Path.Combine(SystemConfig.AppConf.UserDataDir, "DevData");
    private static string ColumnsFilePath => Path.Combine(DevDataDir, "Columns.xml");
    private static string TableDataDir => Path.Combine(DevDataDir, "Tables");
    
    #region IPlugin
    public void OnStart()
    {
        _columns = ObjectHelper.FromXmlFile<List<ColumnInfo>>(ColumnsFilePath);
        
    }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn() { }

    public void OnLoggedOut() { }
    #endregion


    #region IDevData
    private List<ColumnInfo> _columns = [];
    public List<ColumnInfo> Columns
    {
        get => _columns;
        set
        {
            _columns = value;
            if (!ObjectHelper.ToXml(ColumnsFilePath, _columns))
            {
                throw new ApplicationException("Failed to save column file");
            }
        }
    }
    
    
    public Dictionary<string, List<TableInfo>> Tables { get; } = [];
    #endregion
}