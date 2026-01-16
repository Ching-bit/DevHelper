using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Control.Basic;
using Framework.Common;
using Menu.DevData;
using Plugin.DevData;
using Plugin.Log;

namespace UniClient;

public partial class MainViewModel : UniViewModel
{
    #region Constructors
    public MainViewModel()
    {
        Menus = [];
        InitMenus();

        // register language changed
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (_, _) =>
        {
            foreach (MenuConfModel menuConf in Menus)
            {
                menuConf.RefreshMenuName();
            }
        });
    }

    private void InitMenus()
    {
        Menus.Clear();
        
        // column
        Menus.Add(new MenuConfModel
        {
            Id = "menu_column",
            ResourceName = "R_STR_COLUMNS",
            MenuLevel = 1,
            MenuType = MenuType.TopItem,
            Entity = null,
            ViewType = typeof(ColumnsView),
            LeafEntityType = typeof(ColumnInfo)
        });
        
        // table
        MenuConfModel tableMenu = new()
        {
            Id = "menu_table",
            ResourceName = "R_STR_TABLES",
            MenuLevel = 1,
            MenuType = MenuType.TopGroup,
            Entity = Global.Get<IDevData>().TableRoot,
            ViewType = typeof(TableView),
            LeafEntityType = typeof(TableInfo)
        };
        Menus.Add(tableMenu);
        
        // all tables
        InitMenusInner(tableMenu, Global.Get<IDevData>().TableRoot!);
    }

    private void InitMenusInner(MenuConfModel menu, IDirectoryNode root)
    {
        foreach (IDirectoryNode subDirectory in root.SubDirectories)
        {
            MenuConfModel subMenu = AddGroupMenu(menu, subDirectory);
            InitMenusInner(subMenu, subDirectory);
        }
        foreach (IFileNode fileNode in root.Instances)
        {
            _ = AddItemMenu(menu, fileNode);
        }
    }

    public override void OnLoaded(object? sender, RoutedEventArgs e)
    {
        base.OnLoaded(sender, e);
        _manager = new WindowNotificationManager(TopLevel.GetTopLevel(View))
        {
            Position = NotificationPosition.TopCenter,
            MaxItems = 1
        };
    }
    #endregion


    #region Properties
    [ObservableProperty] private ObservableCollection<MenuConfModel> _menus;
    [ObservableProperty] private MenuConfModel? _selectedMenu;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (nameof(SelectedMenu) == e.PropertyName)
        {
            OnMenuSelected();
            SelectedMenu = null;
        }
    }
    #endregion
    
    
    #region Commands
    [RelayCommand]
    private async Task AddGroup(MenuConfModel menu)
    {
        if (menu.Entity is not DirectoryNode directory)
        {
            Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to add a group, but the selected menu doesn't have its directory object");
            return;
        }

        AddItemDialogViewModel vm = new();
        vm.OnConfirmEvent += () =>
        {
            if (directory.SubDirectories.Any(subDirectory => subDirectory.Name.ToLower().Equals(vm.AddItemModel.Name.ToLower())))
            {
                ShowError("R_STR_GROUP_NAME_EXIST_NOTICE");
                return false;
            }
            return true;
        };
        ConfirmDialogResult result = await ConfirmDialog.Show<AddItemDialog>(vm);
        if (!result.IsConfirmed)
        {
            return;
        }

        AddItemModel itemModel = (AddItemModel)result.ReturnParameter!;
        if (!Global.Get<IDevData>().AddGroup(directory, itemModel.Name, itemModel.Description, out IDirectoryNode? newDirectoryNode))
        {
            ShowError("R_STR_ADD_FAILED");
            return;
        }

        AddGroupMenu(menu, newDirectoryNode!);
    }

    [RelayCommand]
    private async Task AddItem(MenuConfModel menu)
    {
        if (menu.Entity is not DirectoryNode directory)
        {
            Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to add an item, but the selected menu doesn't have its directory object");
            return;
        }
            
        AddItemDialogViewModel vm = new();
        vm.OnConfirmEvent += () =>
        {
            if (directory.Instances.Any(instance => instance.Name.ToLower().Equals(vm.AddItemModel.Name.ToLower())))
            {
                ShowError("R_STR_NAME_EXIST_NOTICE");
                return false;
            }
            return true;
        };
        ConfirmDialogResult result = await ConfirmDialog.Show<AddItemDialog>(vm);
        if (!result.IsConfirmed)
        {
            return;
        }

        AddItemModel itemModel = (AddItemModel)result.ReturnParameter!;
        
        if (!Global.Get<IDevData>().AddItem(directory, itemModel.Name, itemModel.Description, out IFileNode? item, menu.LeafEntityType!))
        {
            ShowError("R_STR_ADD_FAILED");
            return;
        }
            
        _ = AddItemMenu(menu, item!);
    }

    [RelayCommand]
    private async Task ModifyItem(MenuConfModel menu)
    {
        if (MenuType.Group == menu.MenuType)
        {
            if (menu.Entity is not DirectoryNode directory || menu.ParentMenu?.Entity is not DirectoryNode parentDirectory)
            {
                Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to modify a group, but the selected menu or the parent menu doesn't have its directory object");
                return;
            }

            AddItemDialogViewModel vm = new()
            {
                AddItemModel =
                {
                    Name = directory.Name,
                    Description = directory.Description
                }
            };
            vm.OnConfirmEvent += () =>
            {
                if (parentDirectory.SubDirectories.Any(x => x.Name.ToLower().Equals(vm.AddItemModel.Name.ToLower()) && !x.Name.Equals(directory.Name)))
                {
                    ShowError("R_STR_GROUP_NAME_EXIST_NOTICE");
                    return false;
                }
                return true;
            };
            
            ConfirmDialogResult result = await ConfirmDialog.Show<AddItemDialog>(vm);
            if (!result.IsConfirmed)
            {
                return;
            }

            AddItemModel itemModel = (AddItemModel)result.ReturnParameter!;
            if (!Global.Get<IDevData>().ModifyGroup(directory, itemModel.Name, itemModel.Description))
            {
                ShowError("R_STR_MODIFY_FAILED");
                return;
            }

            menu.Name = directory.MenuName;
            
        }
        else if (MenuType.Item == menu.MenuType)
        {
            if (menu.Entity is not FileNode fileNode || menu.ParentMenu?.Entity is not DirectoryNode parentDirectory)
            {
                Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to modify an item, but the selected menu or the parent menu doesn't have its directory object");
                return;
            }

            AddItemDialogViewModel vm = new()
            {
                AddItemModel =
                {
                    Name = fileNode.Name,
                    Description = fileNode.Description
                }
            };
            vm.OnConfirmEvent += () =>
            {
                if (parentDirectory.Instances.Any(x => x.Name.ToLower().Equals(vm.AddItemModel.Name.ToLower()) && !x.Name.Equals(fileNode.Name)))
                {
                    ShowError("R_STR_NAME_EXIST_NOTICE");
                    return false;
                }
                return true;
            };
            
            ConfirmDialogResult result = await ConfirmDialog.Show<AddItemDialog>(vm);
            if (!result.IsConfirmed)
            {
                return;
            }

            AddItemModel itemModel = (AddItemModel)result.ReturnParameter!;
            if (!Global.Get<IDevData>().ModifyItem(fileNode, itemModel.Name, itemModel.Description))
            {
                ShowError("R_STR_MODIFY_FAILED");
                return;
            }

            menu.Name = fileNode.MenuName;
        }
        
    }

    [RelayCommand]
    private async Task DeleteItem(MenuConfModel menu)
    {
        // confirm dialog
        string noticeMsg = ResourceHelper.FindStringResource("R_STR_DELETE_CONFIRM_NOTICE").Replace("#", menu.Name);
        if (!await MessageDialog.Show(noticeMsg, isCancelButtonVisible: true))
        {
            return;
        }
        
        if (MenuType.Group == menu.MenuType)
        {
            // delete a group
            if (menu.Entity is not DirectoryNode directory || menu.ParentMenu?.Entity is not DirectoryNode parentDirectory)
            {
                Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to delete a group, but the selected menu or its parent menu doesn't have its target object");
                return;
            }

            if (!Global.Get<IDevData>().RemoveGroup(parentDirectory, directory) ||
                !RemoveMenu(menu))
            {
                ShowError("R_STR_DELETE_FAILED");
            }
        }
        else if (MenuType.Item == menu.MenuType)
        {
            // delete an item
            if (menu.Entity is not FileNode fileNode || menu.ParentMenu?.Entity is not DirectoryNode parentDirectory)
            {
                Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to delete an item, but the selected menu or its parent menu doesn't have its target object");
                return;
            }

            if (!Global.Get<IDevData>().RemoveItem(parentDirectory, fileNode) ||
                !RemoveMenu(menu))
            {
                ShowError("R_STR_DELETE_FAILED");
            }
        }
    }
    #endregion


    #region Private Members
    private WindowNotificationManager? _manager;
    #endregion


    #region Private Functions
    private void ShowError(string message)
    {
        Notification notification = new()
        {
            Message = ResourceHelper.FindStringResource(message)
        };
        _manager?.Show(notification, type: NotificationType.Error);
    }

    #region Menu Related
    private void OnMenuSelected()
    {
        if (null == SelectedMenu ||
            null == View)
        {
            return;
        }

        ((MainView)View).OpenMenu(SelectedMenu);
    }

    private MenuConfModel AddItemMenu(MenuConfModel parent, IFileNode instance)
    {
        MenuType menuType = parent.MenuType switch
        {
            MenuType.TopGroup or MenuType.Group => MenuType.Item,
            _ => throw new InvalidEnumArgumentException("")
        };
        
        MenuConfModel menu = new()
        {
            Id = $"{parent.Id}_{instance.Name}",
            ParentId = parent.Id,
            ParentMenu = parent,
            MenuLevel = parent.MenuLevel + 1,
            Name = instance.MenuName,
            MenuType = menuType,
            Entity = instance,
            ViewType = parent.ViewType,
            LeafEntityType = instance.GetType()
        };
        parent.SubMenus.Add(menu);
        return menu;
    }

    private MenuConfModel AddGroupMenu(MenuConfModel parent, IDirectoryNode directoryNode)
    {
        MenuType menuType = parent.MenuType switch
        {
            MenuType.TopGroup or MenuType.Group => MenuType.Group,
            _ => throw new InvalidEnumArgumentException("")
        };

        MenuConfModel subMenu = new MenuConfModel
        {
            Id = $"{parent.Id}_{directoryNode.Name}",
            ParentId = parent.Id,
            ParentMenu = parent,
            MenuLevel = parent.MenuLevel + 1,
            Name = directoryNode.MenuName,
            MenuType = menuType,
            Entity = directoryNode,
            ViewType = parent.ViewType,
            LeafEntityType = parent.LeafEntityType
        };
        parent.SubMenus.Add(subMenu);
        return subMenu;
    }

    private bool RemoveMenu(MenuConfModel menu)
    {
        if (null == menu.ParentMenu)
        {
            return false;
        }

        return menu.ParentMenu.SubMenus.Remove(menu);
    }
    #endregion
    
    #endregion
    
    
}