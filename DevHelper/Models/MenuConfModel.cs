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
                case MenuType.TopItem:
                    IsAddGroupVisible = false;
                    IsAddVisible = false;
                    IsModifyVisible = false;
                    IsDeleteVisible = false;
                    break;
                case MenuType.TopGroup:
                    IsAddGroupVisible = true;
                    IsAddVisible = true;
                    IsModifyVisible = false;
                    IsDeleteVisible = false;
                    break;
                case MenuType.Group:
                    IsAddGroupVisible = true;
                    IsAddVisible = true;
                    IsModifyVisible = true;
                    IsDeleteVisible = true;
                    break;
                case MenuType.Item:
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

    private Type? _viewType;
    public Type? ViewType
    {
        get => _viewType;
        set
        {
            _viewType = value;
            Assembly = value?.Assembly.GetName().Name ?? string.Empty;
            ViewName = value?.Name ?? string.Empty;
        }
    }
    
    public Type? LeafEntityType { get; set; }
}