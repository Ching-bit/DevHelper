
using Avalonia.Controls;

namespace Menu.DevData;

public partial class ColumnDialog : UserControl
{
    public ColumnDialog()
    {
        InitializeComponent();

        NumericBoxLength.ValueChanged += NumericBoxNullCheck;
        NumericBoxScale.ValueChanged += NumericBoxNullCheck;
    }

    private static void NumericBoxNullCheck(object? s, NumericUpDownValueChangedEventArgs e)
    {
        if (s is not NumericUpDown numericBox)
        {
            return;
        }

        numericBox.Value ??= e.OldValue ?? numericBox.Minimum;
    }
}