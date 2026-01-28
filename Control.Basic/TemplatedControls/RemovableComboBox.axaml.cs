using System.Collections;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;

namespace Control.Basic;

[WithDirectProperty(typeof(IEnumerable), "ItemsSource")]
[WithDirectProperty(typeof(object), "SelectedItem", nullable: true)]
[WithDirectProperty(typeof(bool), "IsEditable")]
[WithDirectProperty(typeof(string), "Text", nullable: true)]
[WithDirectProperty(typeof(string), "Watermark", nullable: true)]
public partial class RemovableComboBox : TemplatedControl
{
    [RelayCommand]
    private async Task RemoveItem(object? parameter)
    {
        if (parameter == null || ItemsSource is not IList list)
        {
            return;
        }
        
        // confirm dialog
        string message = ResourceHelper.FindStringResource("R_STR_DELETE_CONFIRM_NOTICE", "Confirm to delete [#1]?")
            .Replace("#1", parameter.ToString());
        if (!await MessageDialog.Show(message, isCancelButtonVisible: true))
        {
            return;
        }
        
        list.Remove(parameter);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == SelectedItemProperty && null != SelectedItem)
        {
            Text = SelectedItem.ToString();
        }
    }
}