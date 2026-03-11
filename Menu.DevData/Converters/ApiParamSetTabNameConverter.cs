using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Menu.DevData;

public class ApiParamSetTabNameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3 ||
            values[0] is not ApiParamSetModel apiParamSetModel ||
            values[1] is not ObservableCollection<ApiParamSetModel> apiParamSetList ||
            values[2] is not string paramSetName)
        {
            return string.Empty;
        }
        
        return $"{paramSetName} [{apiParamSetList.IndexOf(apiParamSetModel)}]";
    }
}