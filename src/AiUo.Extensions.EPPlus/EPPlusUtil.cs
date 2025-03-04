using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OfficeOpenXml;
using System.Data;
using AiUo.Reflection;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// EPPLus辅助类，XLSX,CSV,List,DataTable
/// </summary>
public static class EPPlusUtil
{
    public static void NoLicense() => ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    #region ReadToTable (Excel => DataTable)
    public static DataTable ReadToTable(string file, string sheetName, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetName: {sheetName} file:{file}");
            return ReadToTable(sheet, config);
        }
    }
    /// <summary>
    /// 读取Excel到DataTable
    /// </summary>
    /// <param name="file">Excel文件</param>
    /// <param name="sheetIndex">sheet索引，从0开始</param>
    /// <param name="config">读取配置</param>
    /// <returns></returns>
    public static DataTable ReadToTable(string file, int sheetIndex = 0, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex} file:{file}");
            return ReadToTable(sheet, config);
        }
    }
    public static DataTable ReadToTable(Stream stream, string sheetName, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetName: {sheetName}");
            return ReadToTable(sheet, config);
        }
    }
    /// <summary>
    /// 读取Excel数据流到DataTable
    /// </summary>
    /// <param name="stream">Excel数据流</param>
    /// <param name="config">读取配置</param>
    /// <param name="sheetIndex">sheet索引，从0开始</param>
    /// <returns></returns>
    public static DataTable ReadToTable(Stream stream, int sheetIndex = 0, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex}");
            return ReadToTable(sheet, config);
        }
    }
    /// <summary>
    /// 读取ExcelWorksheet到DataTable
    /// </summary>
    /// <param name="sheet">ExcelWorksheet</param>
    /// <param name="config">读取配置，默认不使用Excel的header，则自动使用ABCD作为列名</param>
    /// <returns></returns>
    public static DataTable ReadToTable(ExcelWorksheet sheet, ExcelReadConfig config = null)
    {
        config = config ?? new ExcelReadConfig();
        config.Init(sheet);
        var ret = new DataTable();
        foreach (var header in config.Headers)
        {
            if(string.IsNullOrEmpty(header.MapName))
                continue;
            var column = new DataColumn()
            {
                ColumnName = header.MapName,
                DataType = header.DotNetType,
            };
            //excel表头文本
            column.ExtendedProperties.Add("#TITLE#", header.Title);
            foreach (var item in header.Properties)
            {
                column.ExtendedProperties.Add(item.Key, item.Value);
            }
            ret.Columns.Add(column);
        }
        foreach (var row in config.GetRows())
        {
            var newRow = ret.NewRow();
            var idx = 0;
            foreach (var header in config.Headers)
            {
                if (string.IsNullOrEmpty(header.MapName))
                {
                    idx++;
                    continue;
                }
                newRow[header.MapName] = row[idx];
                idx++;
            }
            ret.Rows.Add(newRow);
        }
        return ret;
    }

    #endregion

    #region Write (DataTable => Excel)
    public static void Write(this ExcelPackageEx pkg, string sheetName, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
            sheet = pkg.Workbook.Worksheets.Add(sheetName);
        Write(sheet, table, config);
        if (!string.IsNullOrEmpty(toFile))
            pkg.Save(toFile);
        else
            pkg.Save();
    }
    public static void Write(this ExcelPackageEx pkg, int sheetIndex, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
            throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex} file:{pkg.File?.FullName}");
        Write(sheet, table, config);
        if (!string.IsNullOrEmpty(toFile))
            pkg.Save(toFile);
        else
            pkg.Save();
    }
    public static void Write(string file, string sheetName, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            toFile = toFile ?? file;
            Write(pkg, sheetName, table, config, toFile);
        }
    }
    public static void Write(string file, int sheetIndex, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            toFile = toFile ?? file;
            Write(pkg, sheetIndex, table, config, toFile);
        }
    }
    public static void Write(Stream stream, string sheetName, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            Write(pkg, sheetName, table, config, toFile);
        }
    }
    public static void Write(Stream stream, int sheetIndex, DataTable table, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            Write(pkg, sheetIndex, table, config, toFile);
        }
    }
    public static void Write(ExcelWorksheet sheet, DataTable table, ExcelWriteConfig config)
    {
        config = config ?? new ExcelWriteConfig();
        config.Init(sheet, table, null);
        // headers
        if (config.IsWriteHeader)
        {
            foreach (var header in config.Headers)
            {
                sheet.Cells[config.HeaderRowIndex, header.ColumnIndex].Value = header.Title;
                foreach (var property in header.Properties)
                {
                    sheet.Cells[property.rowIndex, header.ColumnIndex].Value = property.value;
                }
            }
        }
        // rows
        for (int i = 0; i < table.Rows.Count; i++)
        {
            var currRowIndex = config.StartRowIndex + i;
            DataRow row = table.Rows[i];
            foreach (var header in config.Headers)
            {
                header.WriteValue(sheet.Cells[currRowIndex, header.ColumnIndex], row[header.MapName]);
            }
        }
    }
    #endregion

    #region ReadToList (Excel => List<T>)
    public static List<T> ReadToList<T>(string file, string sheetName, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetName: {sheetName} file:{file}");
            return ReadToList<T>(sheet, config);
        }
    }
    public static List<T> ReadToList<T>(string file, int sheetIndex=0, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex} file:{file}");
            return ReadToList<T>(sheet, config);
        }
    }
    public static List<T> ReadToList<T>(Stream stream, string sheetName, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetName: {sheetName}");
            return ReadToList<T>(sheet, config);
        }
    }
    public static List<T> ReadToList<T>(Stream stream, int sheetIndex = 0, ExcelReadConfig config = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
                throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex}");
            return ReadToList<T>(sheet, config);
        }
    }
    public static List<T> ReadToList<T>(ExcelWorksheet sheet, ExcelReadConfig config)
    {
        config = config ?? new ExcelReadConfig();
        config.Init(sheet, typeof(T));
        var ret = new List<T>();
        foreach (var row in config.GetRows())
        {
            var newItem = ReflectionUtil.CreateInstance<T>();
            for (int i = 0; i < config.Headers.Count; i++)
            {
                var header = config.Headers.GetByListIndex(i);
                var propInfo = config.EntityPropertiesDic[header.MapName];
                ReflectionUtil.SetPropertyValue(newItem, propInfo, row[i]);
            }
            ret.Add(newItem);
        }
        return ret;
    }

    #endregion

    #region Write (List<T> => Excel)
    public static void Write<T>(this ExcelPackageEx pkg, string sheetName, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        if (!pkg.TryGetWorksheet(sheetName, out ExcelWorksheet sheet))
            sheet = pkg.Workbook.Worksheets.Add(sheetName);
        Write<T>(sheet, list, config);
        if (!string.IsNullOrEmpty(toFile))
            pkg.Save(toFile);
        else
            pkg.Save();
    }
    public static void Write<T>(this ExcelPackageEx pkg, int sheetIndex, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        if (!pkg.TryGetWorksheet(sheetIndex, out ExcelWorksheet sheet))
            throw new Exception($"Excel文件中不包含此SheetIndex: {sheetIndex} file:{pkg.File?.FullName}");
        Write<T>(sheet, list, config);
        if (!string.IsNullOrEmpty(toFile))
            pkg.Save(toFile);
        else
            pkg.Save();
    }
    /// <summary>
    /// 根据excel模板文件或者新建excel写入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <param name="sheetName"></param>
    /// <param name="list"></param>
    /// <param name="config"></param>
    /// <param name="toFile"></param>
    public static void Write<T>(string file, string sheetName, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            toFile = toFile ?? file;
            Write<T>(pkg, sheetName, list, config, toFile);
        }
    }
    /// <summary>
    /// 根据excel模板文件写入数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="file"></param>
    /// <param name="sheetIndex">模板文件所在index，0开始</param>
    /// <param name="list"></param>
    /// <param name="config"></param>
    /// <param name="toFile"></param>
    public static void Write<T>(string file, int sheetIndex, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(file))
        {
            toFile = toFile ?? file;
            Write<T>(pkg, sheetIndex, list, config, toFile);
        }
    }
    public static void Write<T>(Stream stream, string sheetName, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            Write<T>(pkg, sheetName, list, config, toFile);
        }
    }
    public static void Write<T>(Stream stream, int sheetIndex, List<T> list, ExcelWriteConfig config, string toFile = null)
    {
        using (var pkg = new ExcelPackageEx(stream))
        {
            Write<T>(pkg, sheetIndex, list, config, toFile);
        }
    }
    public static void Write<T>(ExcelWorksheet sheet, List<T> list, ExcelWriteConfig config)
    {
        config = config ?? new ExcelWriteConfig();
        config.Init(sheet, null, typeof(T));
        // headers
        if (config.IsWriteHeader)
        {
            foreach (var header in config.Headers)
            {
                sheet.Cells[config.HeaderRowIndex, header.ColumnIndex].Value = header.Title;
                foreach (var property in header.Properties)
                {
                    sheet.Cells[property.rowIndex, header.ColumnIndex].Value = property.value;
                }
            }
        }
        // rows
        for (int i = 0; i < list.Count; i++)
        {
            var currRowIndex = config.StartRowIndex + i;
            var item = list[i];
            foreach (var header in config.Headers)
            {
                var value = ReflectionUtil.GetPropertyValue(item, config.EntityPropertiesDic[header.MapName]);
                header.WriteValue(sheet.Cells[currRowIndex, header.ColumnIndex], value);
            }
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// 是否安装Excel
    /// </summary>
    public static bool HasExcel 
    { 
        get 
        {
#pragma warning disable CA1416 // 验证平台兼容性
            return Type.GetTypeFromProgID("Excel.Application") != null;
#pragma warning restore CA1416 // 验证平台兼容性
        } 
    }
    /// <summary>
    /// 使用微软Excel打开
    /// </summary>
    /// <param name="file"></param>
    public static void OpenExcel(string file)
    {
        System.Diagnostics.Process.Start(file);
    }

    public static (int RowIndex, int ColIndex) ParseCellIndex(string cellString)
    {
        int row = 0;
        string colStr = null;
        for (int i = 0; i < cellString.Length; i++)
        {
            if (Char.IsDigit(cellString[i]))
            {
                colStr = cellString.Substring(0, i);
                row = int.Parse(cellString.Substring(i));
                break;
            }
        }
        if (row == 0 || string.IsNullOrEmpty(colStr))
            throw new Exception($"Excel cell格式错误。cell: {cellString}");
        int col = ParseColumnIndex(colStr);
        return (row, col);
    }
    public static int ParseColumnIndex(string columnStr)
    {
        int ret = 0;
        for (int i = 0; i < columnStr.Length - 1; i++)
        {
            var num = columnStr[i] - 64;
            var prefix = (int)Math.Pow(26, columnStr.Length - i - 1);
            ret += num * prefix;
        }
        ret += columnStr[columnStr.Length - 1] - 65;
        return ret + 1;// EPPlus Column索引从1开始
    }
    public static string ParseColumnString(int columnIndex)
    {
        StringBuilder ret = new StringBuilder();
        while (columnIndex > 26)
        {
            columnIndex = Math.DivRem(columnIndex, 26, out int result);
            ret.Insert(0, (char)(result + 64));
        }
        ret.Insert(0, (char)(columnIndex + 64));
        return ret.ToString();
    }
    public static string ParseCellString(int columnIndex, int rowIndex)
        => $"{ParseColumnString(columnIndex)}{rowIndex}";
    #endregion
}