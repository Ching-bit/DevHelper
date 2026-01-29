using System.Collections;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Framework.Common;

namespace Control.Basic;

[WithDirectProperty(typeof(IEnumerable), "ItemsSource")]
[WithDirectProperty(typeof(object), "SelectedItem", nullable: true)]
[WithDirectProperty(typeof(bool), "IsEditable")]
[WithDirectProperty(typeof(string), "Text", nullable: true)]
[WithDirectProperty(typeof(string), "Watermark", nullable: true)]
[WithRoutedEvent(typeof(ItemRemovedEventArgs), "ItemRemoved", EventRoutingStrategies.Bubble)]
public partial class RemovableComboBox : TemplatedControl
{
    public class ItemRemovedEventArgs : RoutedEventArgs
    {
        public ItemRemovedEventArgs(RoutedEvent routedEvent, object? removedItem)
            : base(routedEvent)
        {
            RemovedItem = removedItem;
        }
        
        public object? RemovedItem { get; }
    }
    
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
        RaiseEvent(new ItemRemovedEventArgs(ItemRemovedEvent, parameter));
    }
}