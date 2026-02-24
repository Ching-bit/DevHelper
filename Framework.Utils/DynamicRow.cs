using System.ComponentModel;

namespace Framework.Utils;

public class DynamicRow : INotifyPropertyChanged
{
    private readonly Dictionary<string, object?> _values = new();

    public object? this[string key]
    {
        get => _values.GetValueOrDefault(key);
        set
        {
            _values[key] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Dictionary<string, object?> ToDictionary()
    {
        return _values;
    }
}
