using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Common;

namespace UniClient;

public partial class MenuConfModel : MenuConf
{
    private readonly MenuType _menuType;
    public MenuType MenuType
    {
        get => _menuType;
        init
        {
            _menuType = value;
            switch (value)
            {
                case MenuType.Columns:
                    IsAddGroupVisible = false;
                    IsAddVisible = false;
                    IsModifyVisible = false;
                    IsDeleteVisible = false;
                    break;
                case MenuType.Tables:
                    IsAddGroupVisible = true;
                    IsAddVisible = true;
                    IsModifyVisible = false;
                    IsDeleteVisible = false;
                    break;
                case MenuType.TableGroup:
                    IsAddGroupVisible = true;
                    IsAddVisible = true;
                    IsModifyVisible = true;
                    IsDeleteVisible = true;
                    break;
                case MenuType.Table:
                    IsAddGroupVisible = false;
                    IsAddVisible = false;
                    IsModifyVisible = true;
                    IsDeleteVisible = true;
                    break;
            }
        }
    }
    
    [ObservableProperty] private bool _isAddGroupVisible;
    [ObservableProperty] private bool _isAddVisible;
    [ObservableProperty] private bool _isModifyVisible;
    [ObservableProperty] private bool _isDeleteVisible;
    
    public MenuConfModel? ParentMenu { get; set; }
    public object? Entity { get; set; }
}