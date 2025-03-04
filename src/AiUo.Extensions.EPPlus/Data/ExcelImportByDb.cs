using AiUo.Data;
using AiUo.Data.SqlSugar;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlSugar;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// Excel从DataTable导入数据
/// </summary>
public class ExcelImportByDb
{
    private ISqlSugarClient _db;
    public ExcelImportByDb(string connectionStringName = null)
    {
        _db = DbUtil.GetDbById(connectionStringName);
    }
    public ExcelImportByDb(DbType dbType, string connectionString)
    {
        _db = DbUtil.GetDb(dbType, connectionString);
    }

    //public async Task ImportTable(string toExcelFile, string tableName, string sheetName = null)
    //    => await ImportCore(toExcelFile, null, tableName, sheetName);
    public async Task Import(string toExcelFile, string sql, string sheetName = null)
        => await ImportCore(toExcelFile, sql, null, sheetName);

    private async Task ImportCore(string toExcelFile, string sql, string tableName, string sheetName)
    {
        // check
        if (string.IsNullOrEmpty(toExcelFile))
            throw new Exception("ExcelImportFromDb时toExcelFile不能为空");
        if (string.IsNullOrEmpty(sql) && string.IsNullOrEmpty(tableName))
            throw new Exception("ExcelImportFromDb时sql和tableName不能同时为空");
        if (string.IsNullOrEmpty(sql))
        {
            sql = $"select * from {tableName}";
        }
        else if (string.IsNullOrEmpty(tableName))
        {
            var parser = new SqlSelectParser(sql);
            tableName = parser.From.Value;
        }
        sheetName ??= tableName;
        if (_db.DbMaintenance.IsAnyTable(tableName))
            throw new Exception($"ExcelImportFromDb时没有指定SQL且TableName无效。tableName:{tableName}");

        //
        var dt = await _db.Ado.GetDataTableAsync(sql);
        using var pkg = GetPackage(toExcelFile);
        ExcelWorksheet sheet = null;
        var config = new ExcelWriteConfig()
        {
            StartColumnIndex = 2,
            HeaderRowIndex = 3,
            StartRowIndex = 4,
            IsWriteHeader = true
        };
        sheet = pkg.Workbook.Worksheets[sheetName]
                ?? pkg.Workbook.Worksheets.Add(sheetName);
        EPPlusUtil.Write(sheet, dt, config);

        // 添加类型和注释
        var columnInfos = _db.DbMaintenance.GetColumnInfosByTableName(tableName);
        for (int j = 0; j < dt.Columns.Count; j++)
        {
            var column = dt.Columns[j];
            var colSchema = columnInfos.Find(x => x.DbColumnName == column.ColumnName);

            var commentCells = sheet.Cells[1, config.StartColumnIndex + j];
            commentCells.Style.Font.Color.SetColor(System.Drawing.Color.DarkGray);
            commentCells.Style.WrapText = true;//自动换行
            commentCells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            commentCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //commentCells.Value = colSchema.Comment;
            commentCells.RichText.Clear();
            commentCells.RichText.Add(colSchema.ColumnDescription);

            var typeCells = sheet.Cells[2, config.StartColumnIndex + j];
            typeCells.Style.Font.Color.SetColor(System.Drawing.Color.DarkGray);
            typeCells.Style.WrapText = true;//自动换行
            typeCells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            typeCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            typeCells.Value = colSchema.DataType;
        }


        using (var r = sheet.Cells[config.HeaderRowIndex, config.StartColumnIndex
                   , config.HeaderRowIndex, config.StartColumnIndex + dt.Columns.Count - 1])
        {
            r.Style.Font.Bold = true;
            r.Style.ShrinkToFit = false;
            r.Style.WrapText = false;
            //r.AutoFitColumns();
            r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            r.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            r.Style.Fill.SetBackground(OfficeOpenXml.Drawing.eThemeSchemeColor.Accent4);
            //r.Style.Fill.PatternType = ExcelFillStyle.Solid;
        }
        // 添加单元格边框
        using (var r = sheet.Cells[config.HeaderRowIndex, config.StartColumnIndex
                   , dt.Rows.Count + config.HeaderRowIndex, config.StartColumnIndex + dt.Columns.Count - 1])
        {
            r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
            r.AutoFitColumns();
        }
        File.WriteAllBytes(toExcelFile, pkg.GetAsByteArray());
    }
    private ExcelPackage GetPackage(string toExcelFile)
    {
        EPPlusUtil.NoLicense();
        return !File.Exists(toExcelFile)
            ? new ExcelPackage(new FileInfo(toExcelFile))
            : new ExcelPackage(new FileInfo(toExcelFile), true);
    }
}