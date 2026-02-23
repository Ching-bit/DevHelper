using NPOI.SS.UserModel;

namespace Framework.Utils;

public static class ExcelHelper
{
    public static void WriteRow(ISheet sheet, int rowIndex, int cellIndex, string[] contents, ICellStyle style)
    {
        WriteRow(sheet, rowIndex, cellIndex, contents, [style]);
    }
    
    public static void WriteRow(ISheet sheet, int rowIndex, int cellIndex, string[] contents, ICellStyle[] styles)
    {
        IRow row = sheet.CreateRow(rowIndex);

        for (int i = 0; i < contents.Length; i++)
        {
            ICell cell = row.CreateCell(i + cellIndex);
            if (styles.Length > 0)
            {
                cell.CellStyle = i < styles.Length ? styles[i] : styles[^1];
            }
            cell.SetCellValue(contents[i]);
        }
    }

    public static void SetColumnWidth(ISheet sheet, int cellIndex, double[] widths)
    {
        for (int i = 0; i < widths.Length; i++)
        {
            sheet.SetColumnWidth(cellIndex + i, widths[i]);
        }
    }
}