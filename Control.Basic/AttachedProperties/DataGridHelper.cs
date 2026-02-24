using System.Collections;
using System.Windows.Input;
using Attributes.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Control.Basic;

[WithAttachedProperty(typeof(DataGrid), typeof(bool), "EnableRowDrag", false)]
[WithAttachedProperty(typeof(DataGrid), typeof(ICommand), "RowDraggedCommand", nullable: true)]
public partial class DataGridHelper
{
    #region Constructor
    static DataGridHelper()
    {
        EnableRowDragProperty.Changed.AddClassHandler<DataGrid>((dataGrid, e) =>
        {
            if (e.NewValue is not bool enabled) { return; }
            
            dataGrid.LoadingRow -= OnLoadingRow;
            
            if (enabled)
            {
                dataGrid.LoadingRow += OnLoadingRow;
            }
        });
    }

    private static void OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.AddHandler(InputElement.PointerPressedEvent, Row_PointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        e.Row.AddHandler(InputElement.PointerMovedEvent, Row_PointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        e.Row.AddHandler(InputElement.PointerReleasedEvent, Row_PointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }
    #endregion


    #region Private Fields
    private static int _startRowIndex = -1;
    private static int _endRowIndex = -1;
    private static Point _dragStart;
    private static DataGridRow? _currentTargetRow;
    #endregion


    #region Event Handlers
    private static void Row_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled ||
            sender is not DataGridRow dataGridRow ||
            GetOwnerDataGrid(dataGridRow) is not { ItemsSource: IList list })
        {
            return;
        }

        _startRowIndex = list.IndexOf(dataGridRow.DataContext);
        _dragStart = e.GetPosition(null);

        e.Handled = true;
    }
    
    private static void Row_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Handled ||
            sender is not DataGridRow row ||
            GetOwnerDataGrid(row) is not { ItemsSource: IList } dataGrid ||
            !e.GetCurrentPoint(row).Properties.IsLeftButtonPressed)
        {
            return;
        }
        Point diff = e.GetPosition(null) - _dragStart;
        if (Math.Abs(diff.X) < 5) { return; }

        DataGridRow? targetRow = HitRow(dataGrid, e.GetPosition(dataGrid));
        if (targetRow == _currentTargetRow) { return; }

        if (null != _currentTargetRow)
        {
            _currentTargetRow.BorderThickness = new Thickness(0);
            _currentTargetRow.BorderBrush = null;
        }
        else
        {
            DataGridRow? bottomRow = GetRows(dataGrid).MaxBy(x => x.Bounds.Y);
            if (null != bottomRow)
            {
                bottomRow.BorderThickness = new Thickness(0);
                bottomRow.BorderBrush = null;
            }
        }
        _currentTargetRow = targetRow;

        if (null == _currentTargetRow)
        {
            DataGridRow? bottomRow = GetRows(dataGrid).MaxBy(x => x.Bounds.Y);
            if (null != bottomRow)
            {
                bottomRow.BorderThickness = new Thickness(0, 0, 0, 2);
                bottomRow.BorderBrush = Brushes.Aqua;
            }
        }
        else
        {
            _currentTargetRow.BorderThickness = new Thickness(0, 2, 0, 0);
            _currentTargetRow.BorderBrush = Brushes.Aqua;
        }

        e.Handled = true;
    }

    private static void Row_PointerReleased(object? sender, PointerEventArgs e)
    {
        if (e.Handled ||
            sender is not DataGridRow row ||
            GetOwnerDataGrid(row) is not { ItemsSource: IList list } dataGrid)
        {
            return;
        }
        Point diff = e.GetPosition(null) - _dragStart;
        if (Math.Abs(diff.X) < 5) { return; }
            
        DataGridRow? targetRow = HitRow(dataGrid, e.GetPosition(dataGrid));
        _endRowIndex = null == targetRow ? list.Count : list.IndexOf(targetRow.DataContext);
        if (_startRowIndex < _endRowIndex) { _endRowIndex--; }

        if (_startRowIndex != _endRowIndex)
        {
            // change position
            list.Remove(row.DataContext);
            list.Insert(_endRowIndex, row.DataContext);
        
            // raise command
            RowDraggedCommandArgs args = new(row.DataContext, _startRowIndex, _endRowIndex);
            RaiseRowDragged(dataGrid, args);
        }
        
        // reset
        ResetBorders(dataGrid);
        _currentTargetRow = null;
        _startRowIndex = -1;
        _endRowIndex = -1;
        e.Handled = true;
    }
    
    private static void RaiseRowDragged(DataGrid dataGrid, RowDraggedCommandArgs args)
    {
        ICommand? command = GetRowDraggedCommand(dataGrid);
        if (null == command) { return; }
        
        if (command.CanExecute(args))
        {
            command.Execute(args);
        }
    }
    #endregion


    #region Helper Functions
    private static List<DataGridRow> GetRows(DataGrid dataGrid)
    {
        return dataGrid.GetVisualDescendants().OfType<DataGridRow>().ToList();
    }

    private static DataGridRow? HitRow(DataGrid dataGrid, Point point)
    {
        List<DataGridRow> rows = GetRows(dataGrid);
        if (rows.Count == 0) { return null; }
        
        DataGridRow? targetRow = rows.FirstOrDefault(row => row.Bounds.Contains(point));
        if (null != targetRow)
        {
            return targetRow;
        }

        DataGridRow? toppestRow = rows.MinBy(x => x.Bounds.Y);
        if (toppestRow!.Bounds.Top > point.Y)
        {
            return toppestRow;
        }
        
        return null;
    }

    private static DataGrid? GetOwnerDataGrid(DataGridRow row)
    {
        Visual? parent = row;
        while (parent != null && parent is not DataGrid)
        {
            parent = parent.GetVisualParent();
        }
        return parent as DataGrid;
    }

    private static void ResetBorders(DataGrid dataGrid)
    {
        foreach (DataGridRow dataGridRow in GetRows(dataGrid))
        {
            dataGridRow.BorderThickness = new Thickness(0);
            dataGridRow.BorderBrush = null;
        }
    }
    #endregion


    #region Internal Classes
    public class RowDraggedCommandArgs
    {
        public RowDraggedCommandArgs(object? item, int startIndex, int endIndex)
        {
            Item = item;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    
        public object? Item { get; }
        public int StartIndex { get; }
        public int EndIndex { get; }
    }
    #endregion
}