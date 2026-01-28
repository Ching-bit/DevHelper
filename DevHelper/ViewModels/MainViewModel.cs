using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
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
            MenuType = MenuType.Columns,
            Entity = null,
            LeafViewType = typeof(ColumnsView),
            Assembly = typeof(ColumnsView).Assembly.GetName().Name ?? string.Empty,
            ViewName = nameof(ColumnsView),
            LeafEntityType = typeof(ColumnInfo)
        });
        
        // tables
        MenuConfModel tablesMenu = new()
        {
            Id = "menu_tables",
            ResourceName = "R_STR_TABLES",
            MenuLevel = 1,
            MenuType = MenuType.Tables,
            Entity = Global.Get<IDevData>().TableRoot,
            LeafViewType = typeof(TableView),
            LeafEntityType = typeof(TableInfo)
        };
        Menus.Add(tablesMenu);
        
        // databases
        foreach (IDirectoryNode databaseDirectory in Global.Get<IDevData>().TableRoot!.SubDirectories)
        {
            MenuConfModel databaseMenu = AddGroupMenu(tablesMenu, databaseDirectory, MenuType.Database);
            InitMenusInner(databaseMenu, databaseDirectory, MenuType.TableGroup, MenuType.Table);
        }
    }

    private void InitMenusInner(MenuConfModel menu, IDirectoryNode root, MenuType groupMenuType, MenuType itemMenuType)
    {
        foreach (IDirectoryNode subDirectory in root.SubDirectories)
        {
            MenuConfModel subMenu = AddGroupMenu(menu, subDirectory, groupMenuType);
            InitMenusInner(subMenu, subDirectory, groupMenuType, itemMenuType);
        }
        foreach (IFileNode fileNode in root.Instances)
        {
            _ = AddItemMenu(menu, fileNode, itemMenuType);
        }
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
    private async Task AddDatabase(MenuConfModel menu)
    {
        if (menu.Entity is not DirectoryNode directory)
        {
            Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to add a database, but the selected menu doesn't have its directory object");
            return;
        }
        
        AddItemDialogViewModel vm = new();
        vm.OnConfirmEvent += () =>
        {
            if (directory.SubDirectories.Any(subDirectory => subDirectory.Name.ToLower().Equals(vm.AddItemModel.Name.ToLower())))
            {
                ShowNotification("R_STR_NAME_EXIST_NOTICE", NotificationType.Error);
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
            ShowNotification("R_STR_ADD_FAILED", NotificationType.Error);
            return;
        }

        AddGroupMenu(menu, newDirectoryNode!, MenuType.Database);
    }

    [RelayCommand]
    private async Task AddTableGroup(MenuConfModel menu)
    {
        await AddGroup(menu, MenuType.TableGroup);
    }
    
    [RelayCommand]
    private async Task AddTable(MenuConfModel menu)
    {
        await AddItem(menu, MenuType.Table);
    }
    
    private async Task AddGroup(MenuConfModel menu, MenuType menuType)
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
                ShowNotification("R_STR_GROUP_NAME_EXIST_NOTICE", NotificationType.Error);
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
            ShowNotification("R_STR_ADD_FAILED", NotificationType.Error);
            return;
        }

        AddGroupMenu(menu, newDirectoryNode!, menuType);
    }
    
    private async Task AddItem(MenuConfModel menu, MenuType menuType)
    {
        if (menu.Entity is not DirectoryNode directory)
        {
            Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to add an item, but the selected menu doesn't have its directory object");
            return;
        }
            
        AddItemDialogViewModel vm = new();
        vm.OnConfirmEvent += () =>
        {
            IDirectoryNode? rootDirectory = Global.Get<IDevData>().GetRootDirectory(directory);
            if (null != rootDirectory && Global.Get<IDevData>().IsItemNameExists(rootDirectory, vm.AddItemModel.Name))
            {
                ShowNotification("R_STR_NAME_EXIST_NOTICE", NotificationType.Error);
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
            ShowNotification("R_STR_ADD_FAILED", NotificationType.Error);
            return;
        }
            
        _ = AddItemMenu(menu, item!, menuType);
    }

    [RelayCommand]
    private async Task ModifyItem(MenuConfModel menu)
    {
        if (menu.MenuType is MenuType.Database or MenuType.TableGroup)
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
                    ShowNotification("R_STR_GROUP_NAME_EXIST_NOTICE", NotificationType.Error);
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
                ShowNotification("R_STR_MODIFY_FAILED", NotificationType.Error);
                return;
            }

            menu.Name = directory.MenuName;
            
        }
        else if (menu.MenuType is MenuType.Table)
        {
            if (menu.Entity is not FileNode fileNode)
            {
                Global.Get<ILog>().Error(LogModule.PUBLIC, "Try to modify an item, but the selected menu doesn't have its file object");
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
                IDirectoryNode? rootDirectory = Global.Get<IDevData>().GetRootDirectory(fileNode);
                if (null != rootDirectory && Global.Get<IDevData>().IsItemNameExists(rootDirectory, vm.AddItemModel.Name, [fileNode.Name]))
                {
                    ShowNotification("R_STR_NAME_EXIST_NOTICE", NotificationType.Error);
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
                ShowNotification("R_STR_MODIFY_FAILED", NotificationType.Error);
                return;
            }

            menu.Name = fileNode.MenuName;
        }
        
    }

    [RelayCommand]
    private async Task DeleteItem(MenuConfModel menu)
    {
        // confirm dialog
        string noticeMsg = ResourceHelper.FindStringResource("R_STR_DELETE_CONFIRM_NOTICE")
            .Replace("#1", menu.Name);
        if (!await MessageDialog.Show(noticeMsg, isCancelButtonVisible: true))
        {
            return;
        }
        
        if (menu.MenuType is MenuType.Database or MenuType.TableGroup)
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
                ShowNotification("R_STR_DELETE_FAILED", NotificationType.Error);
            }
        }
        else if (menu.MenuType is MenuType.Table)
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
                ShowNotification("R_STR_DELETE_FAILED", NotificationType.Error);
            }
        }
    }
    #endregion


    #region Private Functions
    private void OnMenuSelected()
    {
        if (null == SelectedMenu ||
            null == View)
        {
            return;
        }

        ((MainView)View).OpenMenu(SelectedMenu);
    }

    private MenuConfModel AddItemMenu(MenuConfModel parent, IFileNode instance, MenuType menuType)
    {
        MenuConfModel menu = new()
        {
            Id = $"{parent.Id}_{instance.Name}",
            ParentId = parent.Id,
            ParentMenu = parent,
            MenuLevel = parent.MenuLevel + 1,
            Name = instance.MenuName,
            MenuType = menuType,
            Entity = instance,
            LeafViewType = parent.LeafViewType,
            Assembly = parent.LeafViewType?.Assembly.GetName().Name ?? string.Empty,
            ViewName = parent.LeafViewType?.Name ?? string.Empty,
            LeafEntityType = instance.GetType()
        };
        parent.SubMenus.Add(menu);
        return menu;
    }

    private MenuConfModel AddGroupMenu(MenuConfModel parent, IDirectoryNode directoryNode, MenuType menuType)
    {
        MenuConfModel subMenu = new MenuConfModel
        {
            Id = $"{parent.Id}_{directoryNode.Name}",
            ParentId = parent.Id,
            ParentMenu = parent,
            MenuLevel = parent.MenuLevel + 1,
            Name = directoryNode.MenuName,
            MenuType = menuType,
            Entity = directoryNode,
            LeafViewType = parent.LeafViewType,
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
    
}