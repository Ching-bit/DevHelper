using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Framework.Common;
using Menu.DevData;
using Plugin.DevData;

namespace UniClient;

public partial class MainViewModel : UniViewModel
{
    #region Constructors
    public MainViewModel()
    {
        // init menus
        _menus =
        [
            new MenuConf { Id = "menu_column", ResourceName = "R_STR_COLUMNS", MenuLevel = 1, Assembly = typeof(ColumnsView).Assembly.GetName().Name!, ViewName = nameof(ColumnsView)},
            new MenuConf { Id = "menu_table", ResourceName = "R_STR_TABLES", MenuLevel = 1 }
        ];
        foreach (string tableClass in Global.Get<IDevData>().Tables.Keys)
        {
            List<TableInfo> tableList = Global.Get<IDevData>().Tables[tableClass];
            MenuConf targetMenu;
            MenuConf menuTable = GetMenu("menu_table")!;
            if (string.IsNullOrEmpty(tableClass))
            {
                targetMenu = menuTable;
            }
            else
            {
                string tableClassName = tableClass.Split("_")[0];
                targetMenu = new MenuConf
                {
                    Id = "menu_table_" + tableClassName,
                    ParentId = "menu_table",
                    MenuLevel = menuTable.MenuLevel + 1,
                    Name = tableClass,
                };
                menuTable.SubMenus.Add(targetMenu);
            }

            foreach (TableInfo tableInfo in tableList)
            {
                targetMenu.SubMenus.Add(new MenuConf
                {
                    Id = $"{targetMenu.Id}_{tableInfo.Name}",
                    ParentId = targetMenu.Id,
                    MenuLevel = targetMenu.MenuLevel + 1,
                    Name = $"{tableInfo.Name} ({tableInfo.Description})",
                    Assembly = typeof(TableView).Assembly.GetName().Name!,
                    ViewName = nameof(TableView)
                });
            }
        }

        // register language changed
        WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this, (_, _) =>
        {
            foreach (MenuConf menuConf in Menus)
            {
                menuConf.RefreshMenuName();
            }
        });
    }
    #endregion


    #region Properties
    [ObservableProperty] private ObservableCollection<MenuConf> _menus;
    [ObservableProperty] private MenuConf? _selectedMenu;

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


    private void OnMenuSelected()
    {
        if (null == SelectedMenu ||
            null == View)
        {
            return;
        }

        ((MainView)View).OpenMenu(SelectedMenu);
    }

    private MenuConf? GetMenu(string menuId)
    {
        return Menus.FirstOrDefault(x => x.Id.Equals(menuId));
    }
}