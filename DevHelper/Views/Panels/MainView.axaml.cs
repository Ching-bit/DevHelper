using System.IO;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Framework.Common;
using Framework.Utils.Helpers;
using Plugin.AppEnv;
using Plugin.Log;

namespace UniClient;

public partial class MainView : UniPanel
{
    public MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private UniMenu? _currentActivatedMenu;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (null == MenuDock.Factory)
        {
            Global.Get<ILog>().Error(LogModule.FRAMEWORK, "Menu dock factory null");
            return;
        }
        
        // register language changed
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (r, m) =>
        {
            if (null == MenuDock.VisibleDockables)
            {
                return;
            }
            
            foreach (IDockable dockable in MenuDock.VisibleDockables)
            {
                if (dockable is not Document doc ||
                    doc.Content is not UniMenu menu ||
                    null == menu.MenuConf)
                {
                    continue;
                }
                doc.Title = ResourceHelper.FindStringResource(menu.MenuConf.ResourceName, doc.Title);
            }
        });
        
        // Menu OnInit()
        MenuDock.Factory.DockableAdded += (o, args) =>
        {
            if (args.Dockable is not Document doc ||
                doc.Content is not UniMenu menu)
            {
                return;
            }
            
            Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnInit() [Id={MenuId}, Name={MenuName}]",
                menu.MenuConf?.Id, menu.MenuConf?.Name);
            menu.OnInit();
        };

        // Menu OnShown()
        MenuDock.Factory.FocusedDockableChanged += (o, args) =>
        {
            if (args.Dockable is not Document doc)
            {
                return;
            }
            
            if (doc.Content is not UniMenu menu)
            {
                // Home
                Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Home page shown");
                Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnHidden() [Id={MenuId}, Name={MenuName}]",
                    _currentActivatedMenu?.MenuConf?.Id, _currentActivatedMenu?.MenuConf?.Name);
                _currentActivatedMenu?.OnHidden();
                _currentActivatedMenu = null;
                return;
            }
            
            Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnShown() [Id={MenuId}, Name={MenuName}]",
                menu.MenuConf?.Id, menu.MenuConf?.Name);
            menu.OnShown();

            if (null != _currentActivatedMenu)
            {
                Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnHidden() [Id={MenuId}, Name={MenuName}]",
                    _currentActivatedMenu.MenuConf?.Id, _currentActivatedMenu.MenuConf?.Name);
                _currentActivatedMenu.OnHidden();
            }

            _currentActivatedMenu = menu;
        };
        
        // Menu OnClosing()
        MenuDock.Factory.DockableClosing += (o, args) =>
        {
            if (args.Dockable is not Document doc ||
                doc.Content is not UniMenu menu)
            {
                return;
            }
            
            Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnClosing() [Id={MenuId}, Name={MenuName}]",
                menu.MenuConf?.Id, menu.MenuConf?.Name);
            menu.OnClosing();
        };

        // Menu OnClosed()
        MenuDock.Factory.DockableClosed += (o, args) =>
        {
            if (args.Dockable is not Document doc ||
                doc.Content is not UniMenu menu)
            {
                return;
            }
            
            Global.Get<ILog>().Info(LogModule.FRAMEWORK, "Menu OnClosed() [Id={MenuId}, Name={MenuName}]",
                menu.MenuConf?.Id, menu.MenuConf?.Name);
            menu.OnClosed();
            if (null != menu.MenuConf)
            {
                menu.MenuConf.Instance = null;
            }
        };
    }

    public void OpenMenu(MenuConf menuConf)
    {
        if (null == MenuDock.Factory)
        {
            Global.Get<ILog>().Error(LogModule.FRAMEWORK, "Menu dock factory null");
            return;
        }
        
        if (string.IsNullOrEmpty(menuConf.Assembly) || string.IsNullOrEmpty(menuConf.ViewName))
        {
            return; // Not a menu
        }

        if (menuConf.Instance != null)
        {
            // Not null, the menu already opened
            IDockable? targetDocument =
                MenuDock.Factory.FindDockable(MenuDock, x => x is Document doc && doc.Id == menuConf.Id);
            if (null != targetDocument)
            {
                MenuDock.Factory.SetActiveDockable(targetDocument);
            }
            
            return;
        }
        
        // Open a new menu
        string dllPath = Path.Join(Global.Get<IAppEnv>().AppDir, menuConf.Assembly + ".dll");
        if (!File.Exists(dllPath))
        {
            Global.Get<ILog>().Error(LogModule.FRAMEWORK, $"Can't find dll {dllPath}");
            return;
        }
        
        menuConf.Instance = DllHelper.CreateInstance<UniMenu>(dllPath, menuConf.Assembly, menuConf.ViewName);
        if (null == menuConf.Instance)
        {
            Global.Get<ILog>().Error(LogModule.FRAMEWORK, $"Can't create instance: {dllPath} {menuConf.Assembly}.{menuConf.ViewName}");
            return;
        }

        menuConf.Instance.MenuConf = menuConf;
        
        Document doc = new()
        {
            Id = menuConf.Id,
            Title = menuConf.Name,
            Content = menuConf.Instance
        };
        
        MenuDock.AddDocument(doc);
    }
}