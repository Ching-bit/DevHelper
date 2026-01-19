using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Utils.Helpers;

namespace Framework.Common;

public partial class MenuConf : ObservableObject
{
    public MenuConf()
    {
        Id = string.Empty;
        ParentId = string.Empty;
        Name = string.Empty;
        ResourceName = string.Empty;
        MenuLevel = 0;
        Assembly = string.Empty;
        ViewName = string.Empty;
        SubMenus = [];
    }

    [ObservableProperty] private string _id;
    [ObservableProperty] private string _parentId;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _resourceName;
    [ObservableProperty] private string _assembly;
    [ObservableProperty] private string _viewName;

    [XmlIgnore] public UniMenu? Instance { get; set; }
    [XmlIgnore] public int MenuLevel { get; set; }
    [XmlIgnore] public ObservableCollection<MenuConf> SubMenus { get; set; }
    [XmlIgnore] public object? Entity { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (nameof(ResourceName) == e.PropertyName)
        {
            Name = ResourceHelper.FindStringResource(ResourceName, Name);
        }
    }


    public static ObservableCollection<MenuConf> FromXml(string xmlPath)
    {
        ObservableCollection<MenuConf> menuConfList = ObjectHelper.FromXmlFile<ObservableCollection<MenuConf>>(xmlPath);

        foreach (MenuConf menuConf in menuConfList)
        {
            menuConf.Name = ResourceHelper.FindStringResource(menuConf.ResourceName);
            FillMenuLevel(menuConfList, menuConf);
        }
        return [.. menuConfList.Where(x => string.IsNullOrEmpty(x.ParentId))];
    }

    private static void FillMenuLevel(ObservableCollection<MenuConf> menuConfList, MenuConf menuConf)
    {
        if (string.IsNullOrEmpty(menuConf.ParentId))
        {
            menuConf.MenuLevel = 1;
        }

        MenuConf? parentMenuConf = menuConfList.FirstOrDefault(x => x.Id == menuConf.ParentId);
        if (null == parentMenuConf)
        {
            return;
        }
        FillMenuLevel(menuConfList, parentMenuConf);
        menuConf.MenuLevel = parentMenuConf.MenuLevel + 1;
        parentMenuConf.SubMenus.Remove(menuConf);
        parentMenuConf.SubMenus.Add(menuConf);
    }

    public void RefreshMenuName()
    {
        Name = ResourceHelper.FindStringResource(ResourceName, Name);

        foreach (MenuConf menuConf in SubMenus)
        {
            menuConf.RefreshMenuName();
        }
    }
}