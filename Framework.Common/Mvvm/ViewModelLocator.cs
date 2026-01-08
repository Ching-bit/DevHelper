using Avalonia.Controls;

namespace Framework.Common;

internal static class ViewModelLocator
{
    public static bool SetViewModel(ContentControl view)
    {
        UniViewModel? vm = GetViewModel(view);
        if (null == vm)
        {
            return false;
        }

        view.DataContext ??= vm;
        view.Loaded -= vm.OnLoaded;
        view.Loaded += vm.OnLoaded;
        
        vm.View = view;
        return true;
    }
    
    private static UniViewModel? GetViewModel(ContentControl view)
    {
        Type viewType = view.GetType();
        if (null == viewType.Namespace)
        {
            return null;
        }

        string viewNamespace = viewType.Namespace;
        string viewModelNampsage = (viewNamespace.EndsWith(".Views") ?
            viewNamespace[..^6] : viewNamespace) + ".ViewModels";

        string viewModelClassName = viewType.Name.EndsWith("View") ? viewType.Name + "Model" : viewType.Name + "ViewModel";

        object? viewModel = viewType.Assembly.CreateInstance($"{viewModelNampsage}.{viewModelClassName}");
        if (null == viewModel || viewModel is not UniViewModel)
        {
            viewModelNampsage = viewNamespace;
            viewModel = viewType.Assembly.CreateInstance($"{viewModelNampsage}.{viewModelClassName}");
        }

        if (null == viewModel || viewModel is not UniViewModel)
        {
            return null;
        }

        return (UniViewModel?)viewModel;
    }
}