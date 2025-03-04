using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// 读取Excel配置
///     默认设置：
///         HeaderRowIndex = sheet.Dimension.Start.Row
///         StartRowIndex = sheet.Dimension.Start.Row + 1
///         StartColumnIndex = sheet.Dimension.Start.Column
///         所有的Header.MapName = Excel的标题文本(header.Title)
///     当添加Header时（AddHeader方法）:
///         设定ColumnIndex, ColumnName, Title其一用于确定读取时Excel的列。
///         设定MapName用于设定映射的DataColumn名称或对象属性名
/// </summary>
public class ExcelReadConfig
{
    #region Properties
    /// <summary>
    /// Header所在行索引，从1开始。
    ///     如果等于 0，表示没有设置，header行使用sheet.Dimension.Start.Row
    ///     如果等于-1，表示excel中没有header行，header的title使用excel的ColumnName代替，如A,B,C...
    /// </summary>
    public int HeaderRowIndex { get; set; }
    /// <summary>
    /// 自定义Header的映射。
    /// 当AutoMapHeaders=true时，将会使用excel中的titls定义自动补充剩余的header映射
    /// </summary>
    public HeaderMapperCollection<ReadHeaderMapper> Headers { get; set; } = new HeaderMapperCollection<ReadHeaderMapper>();
    /// <summary>
    /// 当AutoMapHeaders=true时，将会使用excel中的titls定义自动补充剩余的header映射
    /// </summary>
    public bool AutoMapHeaders { get; set; } = true;
    /// <summary>
    /// header扩展属性集合。通过此定义可以读取 Cells[rowIndex, Header.ColumnIndex] 的值，并保存到ReadHeaderMapper.Properties中(键值=key)，映射转换时可以使用
    /// </summary>
    public List<(int rowIndex, string key)> HeaderProperties { get; set; } = new List<(int rowIndex, string key)>();
    /// <summary>
    /// 首行数据索引, 从1开始
    /// 如果小于1，则按一下规则取值：
    ///     如果HeaderRowIndex=-1，则值等于 sheet.Dimension.Start.Row
    ///     如果HeaderRowIndex= 0，则值等于 sheet.Dimension.Start.Row + 1
    /// </summary>
    public int StartRowIndex { get; set; }
    /// <summary>
    /// 首列数据索引，从1开始
    /// 如果小于1，则值等于sheet.Dimension.Start.Column
    /// </summary>
    public int StartColumnIndex { get; set; }
    public int EndRowIndex { get; set; }
    public int EndColumnIndex { get; set; }

    public Func<int, ExcelRangeBase, bool> EndRowChecker { get; set; }
    public Func<int, ExcelRangeBase, bool> EndColumnChecker { get; set; }
    #endregion

    #region 设置属性方法
    /// <summary>
    /// 设置无header行
    /// </summary>
    /// <returns></returns>
    public ExcelReadConfig SetNoHeader()
    {
        HeaderRowIndex = -1;
        return this;
    }
    /// <summary>
    /// 设置header所在行索引，从1开始
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public ExcelReadConfig SetHeaderRowIndex(int rowIndex = 1)
    {
        HeaderRowIndex = rowIndex;
        return this;
    }
    /// <summary>
    /// 添加header映射。必须设置ColumnIndex, ColumnName, Title中的一项
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    public ExcelReadConfig AddHeaderMapper(ReadHeaderMapper header)
    {
        Headers.Add(header);
        return this;
    }
    public ExcelReadConfig AddHeaderMapper(int columnIndex, string mapName=null, Type type=null)
    {
        var header = new ReadHeaderMapper { 
            ColumnIndex = columnIndex,
            MapName = mapName,
            DotNetType = type??typeof(string)
        };
        Headers.Add(header);
        return this;
    }
    public ExcelReadConfig AddHeaderMapper(string title, string mapName = null, Type type = null)
    {
        var header = new ReadHeaderMapper
        {
            Title = title,
            MapName = mapName,
            DotNetType = type ?? typeof(string)
        };
        Headers.Add(header);
        return this;
    }
    /// <summary>
    /// 添加属性
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ExcelReadConfig AddHeaderProperty(int rowIndex, string key)
    {
        HeaderProperties.Add((rowIndex, key));
        return this;
    }
    /// <summary>
    /// 设置数据的起始Cell，如：A2
    /// </summary>
    /// <param name="cellString"></param>
    public ExcelReadConfig SetStartCell(string cellString)
    {
        var cell = EPPlusUtil.ParseCellIndex(cellString);
        StartRowIndex = cell.RowIndex;
        StartColumnIndex = cell.ColIndex;
        return this;
    }
    /// <summary>
    /// 设置数据起始行索引
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public ExcelReadConfig SetStartRowIndex(int rowIndex)
    {
        StartRowIndex = rowIndex;
        return this;
    }
    /// <summary>
    /// 设置数据起始列索引
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public ExcelReadConfig SetStartColumnIndex(int columnIndex = 1)
    {
        StartColumnIndex = columnIndex;
        return this;
    }
    /// <summary>
    /// 设置结束Cell。如B3
    /// </summary>
    /// <param name="cellString"></param>
    public ExcelReadConfig SetEndCell(string cellString)
    {
        var cell = EPPlusUtil.ParseCellIndex(cellString);
        EndRowIndex = cell.RowIndex;
        EndColumnIndex = cell.ColIndex;
        return this;
    }
    public ExcelReadConfig SetEndRowIndex(int rowIndex)
    {
        EndRowIndex = rowIndex;
        return this;
    }
    public ExcelReadConfig SetEndColumnIndex(int columnIndex)
    {
        EndColumnIndex = columnIndex;
        return this;
    }
    /// <summary>
    /// 设置结束行检查器（根据首列值判断）
    /// </summary>
    /// <returns></returns>
    public ExcelReadConfig SetEndRowChecker(CheckerMode mode, string text = null)
    {
        EndRowChecker = (rowIndex, cell) =>
        {
            switch (mode)
            {
                case CheckerMode.Empty:
                    return string.IsNullOrEmpty(cell.Text);
                case CheckerMode.Equals:
                    return cell.Text == text;
                case CheckerMode.Contains:
                    return cell.Text.Contains(text);
                case CheckerMode.StartsWith:
                    return cell.Text.StartsWith(text);
            }
            throw new Exception($"EndRowChecker不支持此模式: {mode}");
        };
        return this;
    }
    /// <summary>
    /// 设置结束列检查器列值为空
    /// </summary>
    /// <returns></returns>
    public ExcelReadConfig SetEndColumnChecker(CheckerMode mode, string text = null)
    {
        EndColumnChecker = (colIndex, cell) =>
        {
            switch (mode)
            {
                case CheckerMode.Empty:
                    return string.IsNullOrEmpty(cell.Text);
                case CheckerMode.Equals:
                    return cell.Text == text;
                case CheckerMode.Contains:
                    return cell.Text.Contains(text);
                case CheckerMode.StartsWith:
                    return cell.Text.StartsWith(text);
            }
            throw new Exception($"EndColumnChecker不支持此模式: {mode}");
        };
        return this;
    }
    public ExcelReadConfig SetAutoAddHeaders(bool auto)
    {
        AutoMapHeaders = auto;
        return this;
    }
    #endregion

    protected ExcelWorksheet Sheet;
    private Type _toType;
    internal Dictionary<string, PropertyInfo> EntityPropertiesDic = new Dictionary<string, PropertyInfo>();
    public void Init(ExcelWorksheet sheet, Type toType = null)
    {
        Sheet = sheet;
        _toType = toType;
        if (toType != null)
        {
            foreach (var prop in toType.GetProperties())
                EntityPropertiesDic.Add(prop.Name, prop);
        }
        // 设置无header
        if (HeaderRowIndex < 0)
        {
            // 没设置StartRowIndex
            if (StartRowIndex < 1)
                StartRowIndex = sheet.Dimension.Start.Row;
        }
        else
        {
            //没有设置header
            if (HeaderRowIndex == 0)
            {
                HeaderRowIndex = sheet.Dimension.Start.Row;
            }
            // 没设置StartRowIndex
            if (StartRowIndex < 1)
                StartRowIndex = HeaderRowIndex + 1;
        }
        //没有设置StartColumnIndex
        if (StartColumnIndex < 1)
            StartColumnIndex = sheet.Dimension.Start.Column;
        HandleHeaders();
    }
    private void HandleHeaders()
    {
        var sheetHeaders = GetSheetHeaders();
        RepairHeaderMappers(sheetHeaders);
        //Properties
        foreach (var property in HeaderProperties)
        {
            foreach (var header in Headers)
            {
                var value = Sheet.Cells[property.rowIndex, header.ColumnIndex].Value;
                header.Properties.Add(property.key, value);
            }
        }
        Headers.CheckAndComplete();
    }
    protected SheetHeaderCollection GetSheetHeaders()
    {
        var ret = new SheetHeaderCollection();
        // 设置无header
        if (HeaderRowIndex < 0)
        {
            for (int i = StartColumnIndex; i < int.MaxValue; i++)
            {
                if (CheckColumnEnd(i, null))
                    break;
                ret.Add(i, EPPlusUtil.ParseColumnString(i));
            }
        }
        else
        {
            for (int i = StartColumnIndex; i < int.MaxValue; i++)
            {
                var range = Sheet.Cells[HeaderRowIndex, i];
                if (CheckColumnEnd(i, range))
                    break;
                ret.Add(i, range.Text);
            }
        }
        if (ret.Count == 0)
            throw new Exception($"没有获得预期的SheetHeaders。HeaderRowIndex={HeaderRowIndex}");
        return ret;
    }
    private void RepairHeaderMappers(SheetHeaderCollection sheetHeaders)
    {
        // 1. 完善自定义的headers
        foreach (var header in Headers)
        {
            // ColumnIndex => Title
            if (header.ColumnIndex > 0)
            {
                if (string.IsNullOrEmpty(header.Title))
                {
                    if (!sheetHeaders.Contains(header.ColumnIndex))
                        throw new Exception($"定义ReadHeaderMapper时通过ColumnIndex定位时必须定义HeaderRowIndex");
                    header.Title = sheetHeaders[header.ColumnIndex];
                }
            }
            else // Title => ColumnIndex
            { 
                if (string.IsNullOrEmpty(header.Title))
                    throw new Exception($"定义ReadHeaderMapper时ColumnIndex和Title不能同时为空");
                if (!sheetHeaders.Contains(header.Title))
                    throw new Exception($"定义ReadHeaderMapper时通过Title定位时必须定义HeaderRowIndex");
                header.ColumnIndex = sheetHeaders[header.Title];
            }
            // MapToName 默认使用Title
            if (string.IsNullOrEmpty(header.MapName))
                header.MapName = header.Title;
        }
        // 2. 根据sheetHeaders自动填充
        if (AutoMapHeaders)
        {
            foreach (var sheetHeader in sheetHeaders)
            {
                if (Headers.ContainsColumnIndex(sheetHeader.ColumnIndex))
                    continue;
                var header = new ReadHeaderMapper
                {
                    ColumnIndex = sheetHeader.ColumnIndex,
                    Title = sheetHeader.Title,
                    MapName = sheetHeader.Title
                };
                Headers.Add(header);
            }
        }
        // 3. 根据EntityProperties缩减Headers
        if (EntityPropertiesDic.Count > 0)
        {
            var mapNameDic = Headers.GetMapNameDic();
            Headers.Clear();
            foreach (var item in mapNameDic)
            {
                if (EntityPropertiesDic.ContainsKey(item.Key))
                {
                    item.Value.DotNetType = EntityPropertiesDic[item.Key].PropertyType;
                    Headers.Add(item.Value);
                }
            }
        }
    }

    public IEnumerable<List<object>> GetRows()
    {
        for (int i = StartRowIndex; i < int.MaxValue; i++)
        {
            var ret = new List<object>();
            var rowCell = Sheet.Cells[i, Headers.First().ColumnIndex, i, Headers.Last().ColumnIndex];
            if (CheckRowEnd(i, rowCell))
                break;
            foreach (var header in Headers)
            {
                var cell = Sheet.Cells[i, header.ColumnIndex];
                var value = header.ReadCellValue(cell);
                ret.Add(value);
            }
            yield return ret;
        }
    }
    private bool CheckColumnEnd(int columnIndex, ExcelRangeBase cell)
    {
        if (EndColumnIndex > 0 && EndColumnIndex < columnIndex)
            return true;
        if (EndColumnChecker != null)
            return EndColumnChecker(columnIndex, cell);
        //设置无header
        if (HeaderRowIndex < 0)
            return Sheet.Dimension.End.Column < columnIndex;
        else
            return string.IsNullOrEmpty(cell.Text);
    }

    private bool CheckRowEnd(int rowIndex, ExcelRangeBase cell)
    {
        if (EndRowIndex > 0 && EndRowIndex < rowIndex)
            return true;
        if (EndRowChecker != null)
            return EndRowChecker(rowIndex, cell);
        //最后一行采用Dimension.End判断
        return Sheet.Dimension.End.Row < rowIndex;
        //return string.IsNullOrEmpty(cell.Text);
    }
}