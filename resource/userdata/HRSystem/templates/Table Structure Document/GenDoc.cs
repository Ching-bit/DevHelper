using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using Framework.Common;
using Framework.Utils;
using Plugin.AppEnv;
using Plugin.DevData;

IWorkbook workbook = new HSSFWorkbook();
IFont font;

// Styles
// head style
ICellStyle styleHead = workbook.CreateCellStyle();
font = workbook.CreateFont();
font.FontName = "Arial";
font.IsBold = true;
font.FontHeightInPoints = 11;
styleHead.SetFont(font);
styleHead.Alignment = HorizontalAlignment.Center;
styleHead.VerticalAlignment = VerticalAlignment.Center;
styleHead.BorderTop = BorderStyle.Thin;
styleHead.BorderBottom = BorderStyle.Thin;
styleHead.BorderLeft = BorderStyle.Thin;
styleHead.BorderRight = BorderStyle.Thin;
styleHead.FillForegroundColor = IndexedColors.Grey25Percent.Index;
styleHead.FillPattern = FillPattern.SolidForeground;

// column name style
ICellStyle styleColName = workbook.CreateCellStyle();
font = workbook.CreateFont();
font.FontName = "Arial";
font.IsBold = false;
font.FontHeightInPoints = 11;
styleColName.SetFont(font);
styleColName.Alignment = HorizontalAlignment.Left;
styleColName.VerticalAlignment = VerticalAlignment.Center;
styleColName.BorderTop = BorderStyle.Thin;
styleColName.BorderBottom = BorderStyle.Thin;
styleColName.BorderLeft = BorderStyle.Thin;
styleColName.BorderRight = BorderStyle.Thin;
styleColName.FillForegroundColor = IndexedColors.LightGreen.Index;
styleColName.FillPattern = FillPattern.SolidForeground;

// general style
ICellStyle styleGeneral = workbook.CreateCellStyle();
font = workbook.CreateFont();
font.FontName = "Arial";
font.IsBold = false;
font.FontHeightInPoints = 11;
styleGeneral.SetFont(font);
styleGeneral.Alignment = HorizontalAlignment.Center;
styleGeneral.VerticalAlignment = VerticalAlignment.Center;
styleGeneral.BorderTop = BorderStyle.Thin;
styleGeneral.BorderBottom = BorderStyle.Thin;
styleGeneral.BorderLeft = BorderStyle.Thin;
styleGeneral.BorderRight = BorderStyle.Thin;


// Cover sheet
ISheet sheetCover = workbook.CreateSheet("Cover");
sheetCover.SetColumnWidth(1, 20000);

IRow rowCoverTitle = sheetCover.CreateRow(1);
rowCoverTitle.Height = 2000;
ICell cellCoverTitle = rowCoverTitle.CreateCell(1);
cellCoverTitle.CellStyle = workbook.CreateCellStyle();
font = workbook.CreateFont();
font.FontName = "Arial";
font.IsBold = true;
font.FontHeightInPoints = 22;
cellCoverTitle.CellStyle.SetFont(font);
cellCoverTitle.CellStyle.Alignment = HorizontalAlignment.Center;
cellCoverTitle.CellStyle.VerticalAlignment = VerticalAlignment.Center;
cellCoverTitle.SetCellValue("Table Structure of HR System");

// Database sheet
foreach (DatabaseInfo databaseInfo in Tables.Keys)
{
	ISheet sheetDatabase = workbook.CreateSheet(databaseInfo.Name + (string.IsNullOrEmpty(databaseInfo.Description) ? string.Empty : $"({databaseInfo.Description})"));
	const int cellStartIndex = 0;
	ExcelHelper.SetColumnWidth(sheetDatabase, cellStartIndex, [20 * 256, 20 * 256, 18 * 256, 15 * 256, 10 * 256, 32 * 256, 32 * 256]);

	int rowIndex = 1;
	List<TableInfo> tableList = Tables[databaseInfo];
	foreach (TableInfo tableInfo in tableList)
	{
		IndexInfo? primaryKeyInfo = tableInfo.IndexList.FirstOrDefault(x => x.Type == IndexType.Primary);
		
		ExcelHelper.WriteRow(sheetDatabase, rowIndex++, cellStartIndex, ["Table", tableInfo.Name, "History Table", tableInfo.HasHistoryTable ? "Y" : "N", "", "", ""], [styleHead, styleGeneral, styleHead, styleGeneral]);
		ExcelHelper.WriteRow(sheetDatabase, rowIndex++, cellStartIndex, ["Name", "Description", "Type", "Primary Key", "Nullable", "Data Dictionary", "Remark"], styleHead);
		foreach (int columnId in tableInfo.ColumnIdList)
		{
			ColumnInfo? columnInfo = Global.Get<IDevData>().Columns.FirstOrDefault(x => x.Id == columnId);
			if (null == columnInfo) { continue; }
			bool isPrimaryKey = null != primaryKeyInfo && primaryKeyInfo.ColumnIdList.Contains(columnInfo.Id);
			ExcelHelper.WriteRow(sheetDatabase, rowIndex++, cellStartIndex, [columnInfo.Name, columnInfo.Description, columnInfo.GetDbType(GenTask.DatabaseType), isPrimaryKey ? "✓" : string.Empty, columnInfo.IsNullable ? "✓" : string.Empty, columnInfo.DataDict, columnInfo.Remark], [styleColName, styleGeneral]);
		}
		
		rowIndex++;
	}
}


// write file
string filePath = Path.Combine(GenTask!.OutputDir, GenTask!.OutputFile);
using (FileStream file = new(filePath, FileMode.Create, FileAccess.Write))
{
    workbook.Write(file);
}

// modify time of the file
string dataDir = Path.Combine(SystemConfig.AppConf.UserDataDir, Global.Get<IAppEnv>().User?.Username ?? string.Empty, "DevData");
string columnsConfPath = Path.Combine(dataDir, "Columns.xml");
string tablesConfDir = Path.Combine(dataDir, "Tables");
DateTime lastWriteTime = File.GetLastWriteTime(columnsConfPath);
DateTime dt = Directory.EnumerateFiles(tablesConfDir, "*", SearchOption.AllDirectories).Select(f => File.GetLastWriteTime(f)).Max();
if (dt > lastWriteTime)
{
    lastWriteTime = dt;
}

File.SetCreationTime(filePath, lastWriteTime);
File.SetLastWriteTime(filePath, lastWriteTime);
File.SetLastAccessTime(filePath, lastWriteTime);
