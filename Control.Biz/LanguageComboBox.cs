using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Framework.Common;

namespace Control.Biz;

public class LanguageComboBox : ComboBox
{
    public LanguageComboBox()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ItemsSource = new LanguageConst();
        SelectedValueBinding = new Binding("Key");
        SelectionChanged -= OnSelectionChanged;
        SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        string lang = SelectedValue + "";
        LanguageHelper.SetLanguage(lang);
    }
}