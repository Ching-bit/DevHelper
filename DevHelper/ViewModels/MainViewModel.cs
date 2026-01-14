using System;
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
            Assembly = typeof(ColumnsView).Assembly.GetName().Name!,
            ViewName = nameof(ColumnsView),
            MenuType = MenuType.Columns
        });
        
        // table
        MenuConfModel tableMenu = new()
        {
            Id = "menu_table",
            ResourceName = "R_STR_TABLES",
            MenuLevel = 1,
            MenuType = MenuType.Tables,
            Entity = Global.Get<IDevData>().TableRoot
        };
        Menus.Add(tableMenu);
        
        // all tables
        InitTableMenus(tableMenu, Global.Get<IDevData>().TableRoot);
    }

    private void InitTableMenus(MenuConfModel menu, DirectoryNode<TableInfo> tableRoot)
    {
        foreach (TableInfo tableInfo in tableRoot.Instances)
        {
            AddMenu(menu, tableInfo, typeof(TableView), MenuType.Table);
        }

        foreach (DirectoryNode<TableInfo> subDirectory in tableRoot.SubDirectories)
        {
            MenuConfModel subMenu =AddGroupMenu(menu, subDirectory);
            InitTableMenus(subMenu, subDirectory);
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
        if (MenuType.Tables == menu.MenuType)
        {
            if (menu.Entity is not DirectoryNode<TableInfo> node)
            {
                // Error
                return;
            }

            AddItemDialogViewModel vm = new();
            vm.OnConfirmEvent += () =>
            {
                if (node.SubDirectories.Any(subDirectory => subDirectory.Name.Equals(vm.AddItemModel.Name)))
                {
                    ShowError("R_STR_GROUP_EXIST_NOTICE");
                    return false;
                }
                return true;
            };
            ConfirmDialogResult result = await ConfirmDialog.Show<AddItemDialog>(vm);
            if (!result.IsConfirmed)
            {
                return;
            }

            AddItemModel addItemModel = (AddItemModel)result.ReturnParameter!;
            if (!Global.Get<IDevData>().AddTableGroup(node, addItemModel.Name, addItemModel.Description, out DirectoryNode<TableInfo>? newDirectoryNode))
            {
                return;
            }

            AddGroupMenu(menu, newDirectoryNode!);
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

    private MenuConfModel AddMenu<T>(MenuConfModel parent, T instance, Type viewType, MenuType menuType) where T : FileNode, new()
    {
        MenuConfModel menu = new()
        {
            Id = $"{parent.Id}_{instance.Name}",
            ParentId = parent.Id,
            MenuLevel = parent.MenuLevel + 1,
            Name = $"{instance.Name}" +
                   (string.IsNullOrEmpty(instance.Description) ? "" : $" ({instance.Description})"),
            Assembly = viewType.Assembly.GetName().Name!,
            ViewName = viewType.Name,
            MenuType = menuType,
            Entity = instance
        };
        parent.SubMenus.Add(menu);
        return menu;
    }

    private MenuConfModel AddGroupMenu<T>(MenuConfModel parent, DirectoryNode<T> directoryNode) where T : FileNode, new()
    {
        MenuConfModel subMenu = new MenuConfModel
        {
            Id = $"{parent.Id}_{directoryNode.Name}",
            ParentId = parent.Id,
            MenuLevel = parent.MenuLevel + 1,
            Name = $"{directoryNode.Name}" + (string.IsNullOrEmpty(directoryNode.Description) ? "" : $" ({directoryNode.Description})"),
            MenuType = MenuType.TableGroup,
            Entity = directoryNode
        };
        parent.SubMenus.Add(subMenu);
        return subMenu;
    }
    #endregion
    
    #endregion
    
    
}