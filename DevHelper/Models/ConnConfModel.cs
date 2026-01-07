using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;

namespace UniClient;

public partial class ConnConfModel : ConnConf
{
    public ConnConfModel(ConnConf connConf)
    {
        Type = connConf.Type;
        Name = connConf.Name;
        Address = connConf.Address;

        IsSelected = false;
        Delay = null;
    }

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private long? _delay;
}