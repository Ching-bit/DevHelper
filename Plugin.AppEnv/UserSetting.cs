using Framework.Common;
using Framework.Utils.Helpers;

namespace Plugin.AppEnv;

public class UserSetting : IUserSetting
{
    public string DatabaseType { get; set; } = string.Empty;
    public string HistoryDatabaseName { get; set; } = string.Empty;
    public string HistoryTableName { get; set; } = string.Empty;
    public string ArchiveDateColumnName { get; set; } = string.Empty;

    public List<GenTask> GenTasks { get; set; } = [];
    
    
    public void OnStart() { }

    public void OnStop() { }

    public void OnLogin() { }

    public void OnLoggedIn()
    {
        string confPath = Path.Combine(Global.Get<IAppEnv>().UserDataDir, $"{nameof(UserSetting)}.xml");
        if (!File.Exists(confPath))
        {
            Save();
            return;
        }
        UserSetting userSetting = ObjectHelper.FromXmlDir<UserSetting>(Global.Get<IAppEnv>().UserDataDir);
        ObjectHelper.Copy(this, userSetting);

        // predefined generating tasks
        if (GenTasks.All(x => GenTaskType.TableDocument != x.TaskType))
        {
            GenTasks.Insert(0, new GenTask
            {
                TaskType = GenTaskType.TableDocument,
                TaskNameResource = "R_STR_TABLE_STRUCTURE_SPECIFICATION",
                OutputDir = Path.Combine(Global.Get<IAppEnv>().AppDir, "gen_codes")
            });
        }
        if (GenTasks.All(x => GenTaskType.TableScripts != x.TaskType))
        {
            GenTasks.Insert(1, new GenTask
            {
                TaskType = GenTaskType.TableScripts,
                TaskNameResource = "R_STR_TABLE_SCRIPTS",
                OutputDir = Path.Combine(Global.Get<IAppEnv>().AppDir, "gen_codes")
            });
        }
    }

    public void OnLoggedOut() { }


    public void Save()
    {
        ObjectHelper.ToXmlDir(Global.Get<IAppEnv>().UserDataDir, this);
    }
}