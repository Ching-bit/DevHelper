using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Control.Basic;

internal class NetDelayToColorConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3 || !long.TryParse(values[0] + "", out long delay))
        {
            return Brushes.Black;
        }

        if (delay < 0 || long.TryParse(values[2] + "", out long errorThreshold) && delay >= errorThreshold)
        {
            return Brushes.Red;
        }

        if (long.TryParse(values[1] + "", out long warnThreshold) && delay >= warnThreshold)
        {
            return Brushes.Yellow;
        }

        return Brushes.Green;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    
}