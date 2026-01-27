using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;

namespace UniClient;

public partial class MenuConfModel : MenuConf
{
    [ObservableProperty] private MenuType _menuType;
    
    public MenuConfModel? ParentMenu { get; set; }
    public Type? LeafViewType { get; set; }
    public Type? LeafEntityType { get; set; }
}