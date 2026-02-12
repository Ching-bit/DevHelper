using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;

namespace Control.Basic;

[WithDirectProperty(typeof(IEnumerable), "ItemsSource", nullable: true)]
public partial class DataGridSelectableColumn : DataGridTemplateColumn
{
    public string BindingPath { get; set; } = string.Empty;

    private readonly CheckBox _headerCheckBox;

    public DataGridSelectableColumn()
    {
        _headerCheckBox = new CheckBox
        {
            IsThreeState = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _headerCheckBox.IsCheckedChanged += (_, _) =>
        {
            SetAll(_headerCheckBox.IsChecked);
        };
        
        HeaderTemplate = new FuncDataTemplate<object>((_, _) => _headerCheckBox);
        
        CellTemplate = new FuncDataTemplate<object>((item, _) =>
        {
            CheckBox cb = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            cb.Bind(ToggleButton.IsCheckedProperty,
                new Binding(BindingPath) { Mode = BindingMode.TwoWay });
            
            if (item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == BindingPath)
                        UpdateHeaderState();
                };
            }
            
            return cb;
        });
        
        CanUserReorder = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ItemsSourceProperty)
        {
            HookItemsSource();
            UpdateHeaderState();
        }
    }

    private void HookItemsSource()
    {
        if (ItemsSource is not INotifyCollectionChanged coll)
        {
            return;
        }
        
        coll.CollectionChanged += (_, _) => UpdateHeaderState();

        foreach (object? item in ItemsSource)
        {
            if (item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == BindingPath)
                    {
                        UpdateHeaderState();
                    }
                };
            }
        }
    }

    private void SetAll(bool? value)
    {
        if (null == value || ItemsSource == null)
        {
            return;
        }

        foreach (object? item in ItemsSource)
        {
            PropertyInfo? prop = item?.GetType().GetProperty(BindingPath);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(item, value);
            }
        }

        UpdateHeaderState();
    }

    private void UpdateHeaderState()
    {
        if (ItemsSource == null) { return; }

        List<bool> values = ItemsSource.Cast<object>()
            .Select(i => i.GetType().GetProperty(BindingPath)?.GetValue(i) is true)
            .ToList();

        if (values.Count == 0)
        {
            _headerCheckBox.IsChecked = false;
            return;
        }

        if (values.All(v => v))
        {
            _headerCheckBox.IsChecked = true;
        }
        else if (values.All(v => !v))
        {
            _headerCheckBox.IsChecked = false;
        }
        else
        {
            _headerCheckBox.IsChecked = null;
        }
    }
        
}