using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;

namespace Menu.DevData;

public partial class DataDictionaryItemModel : UniModel
{
    [ObservableProperty] private string _value = string.Empty;
    [ObservableProperty] private string _meaning = string.Empty;
}