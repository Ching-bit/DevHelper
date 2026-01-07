using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Control.Basic;
using Framework.Common;
using Framework.Utils;
using Plugin.AppEnv;
using Plugin.Log;

namespace UniClient;

public partial class ConnectionsDialogViewModel : ConfirmDialogViewModel
{
    public ConnectionsDialogViewModel()
    {
        ConnConfs = [];
        IGlobalSetting globalSetting = Global.Get<IGlobalSetting>();
        ConnConfs.Clear();
        if (SystemConfig.ConnConfDict.TryGetValue(ConnConfType.Trade, out List<ConnConf>? tradeConnList))
        {
            tradeConnList.ForEach(x => ConnConfs.Add(new ConnConfModel(x)));
        }

        ConnConfModel? selectedConnConf = ConnConfs.FirstOrDefault(x => x.Name == globalSetting.SelectedConn);
        if (null != selectedConnConf)
        {
            selectedConnConf.IsSelected = true;
        }
    }
    
    [ObservableProperty] private ObservableCollection<ConnConfModel> _connConfs;
    
    [RelayCommand] private void TestDelay()
    {
        Global.Get<ILog>().Debug(LogModule.DIALOG, "TestDelay button clicked");

        foreach (ConnConfModel model in ConnConfs)
        {
            if (string.IsNullOrEmpty(model.Host) || 0 == model.Port)
            {
                continue;
            }

            BackgroundWorker worker = new();
            worker.DoWork += (_, _) =>
            {
                long delay = NetworkHelper.Delay(model.Host, model.Port);
                model.Delay = delay;
            };
            worker.RunWorkerAsync();
        }
    }
}