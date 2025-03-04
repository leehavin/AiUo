using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// Excel写入配置
///     默认设置：
///         IsWriteHeader = true
///         HeaderRowIndex = 1
///         StartRowIndex = 2
///         StartColumnIndex = 1
/// </summary>
public class ExcelWriteConfig
{
    /// <summary>
    /// 是否写入header
    /// </summary>
    public bool IsWriteHeader { get; set; } = true;
    /// <summary>
    /// header所在行索引
    /// </summary>
    public int HeaderRowIndex { get; set; }
    public HeaderMapperCollection<WriteHeaderMapper> Headers { get; set; } = new HeaderMapperCollection<WriteHeaderMapper>();
    //public List<WriteHeaderMapper> Headers { get; set; } = new List<WriteHeaderMapper>();
    /// <summary>
    /// 是否根据DataColumns或对象属性自动添加Headers
    /// </summary>
    public bool AutoMapHeaders { get; set; } = true;
    public List<(int rowIndex, string key)> HeaderProperties { get; set; } = new List<(int rowIndex, string key)>();
    public int StartRowIndex { get; set; }
    public int StartColumnIndex { get; set; }

    #region 设置属性方法
    /// <summary>
    /// 设置是否写入header
    /// </summary>
    /// <returns></returns>
    public ExcelWriteConfig SetIsWriteHeader(bool isWrite = false)
    {
        IsWriteHeader = isWrite;
        return this;
    }
    public ExcelWriteConfig SetHeaderRowIndex(int rowIndex = 1)
    {
        HeaderRowIndex = rowIndex;
        return this;
    }
    public ExcelWriteConfig AddHeaderMapper(WriteHeaderMapper header)
    {
        Headers.Add(header);
        return this;
    }
    public ExcelWriteConfig AddHeaderMapper(int columnIndex, string mapName = null)
    {
        var header = new WriteHeaderMapper
        {
            ColumnIndex = columnIndex,
            MapName = mapName
        };
        Headers.Add(header);
        return this;
    }
    public ExcelWriteConfig AddHeaderMapper(string title, string mapName = null)
    {
        var header = new WriteHeaderMapper
        {
            Title = title,
            MapName = mapName
        };
        Headers.Add(header);
        return this;
    }
    public ExcelWriteConfig AddHeaderProperty(int rowIndex, string key)
    {
        HeaderProperties.Add((rowIndex, key));
        return this;
    }
    public ExcelWriteConfig SetStartCell(string cellString)
    {
        var cell = EPPlusUtil.ParseCellIndex(cellString);
        StartRowIndex = cell.RowIndex;
        StartColumnIndex = cell.ColIndex;
        return this;
    }
    public ExcelWriteConfig SetStartRowIndex(int rowIndex)
    {
        StartRowIndex = rowIndex;
        return this;
    }
    public ExcelWriteConfig SetStartColumnIndex(int columnIndex)
    {
        StartColumnIndex = columnIndex;
        return this;
    }

    #endregion

    protected ExcelWorksheet Sheet;
    private DataTable _table;
    // key: DataColumnName
    internal Dictionary<string, DataColumn> DataColumnsDic = new Dictionary<string, DataColumn>();
    private Type _toType;
    // key: PropertyName
    internal Dictionary<string, PropertyInfo> EntityPropertiesDic = new Dictionary<string, PropertyInfo>();
    public void Init(ExcelWorksheet sheet, DataTable table, Type toType)
    {
        Sheet = sheet;
        _table = table;
        if (_table != null)
        {
            foreach (DataColumn column in table.Columns)
                DataColumnsDic.Add(column.ColumnName, column);
        }
        _toType = toType;
        if (toType != null)
        {
            foreach (var prop in toType.GetProperties())
                EntityPropertiesDic.Add(prop.Name, prop);
        }
        //
        if (IsWriteHeader && HeaderRowIndex < 1)
            HeaderRowIndex = 1;
        //throw new Exception($"IsWriteHeader=true时HeaderRowIndex必须设置大于0的值。HeaderRowIndex:{HeaderRowIndex}");
        if (StartRowIndex < 1)
        {
            if (HeaderRowIndex > 0)
            {
                StartRowIndex = HeaderRowIndex + 1;
            }
            else
            {
                StartRowIndex = sheet.Dimension != null ? sheet.Dimension.Start.Row : 1;
            }
        }
        if (StartColumnIndex < 1)
            StartColumnIndex = sheet.Dimension != null ? sheet.Dimension.Start.Column : 1;
        HandleHeaders();
    }
    private void HandleHeaders()
    {
        var columnCount = _table != null ? _table.Columns.Count : EntityPropertiesDic.Count;
        var sheetHeaders = GetSheetHeaders(columnCount);
        RepairHeaderMappers(sheetHeaders);
        if (_table != null)
        {
            foreach (var property in HeaderProperties)
            {
                foreach (var header in Headers)
                {
                    var value = _table.Columns[header.MapName].ExtendedProperties[property.key];
                    header.Properties.Add((property.rowIndex, value));
                }
            }
        }
        Headers.CheckAndComplete();
    }
    private void RepairHeaderMappers(SheetHeaderCollection sheetHeaders)
    {
        // 1. 完善自定义的headers
        foreach (var header in Headers)
        {
            if (header.ColumnIndex > 0)
            {
                if (string.IsNullOrEmpty(header.Title))
                {
                    if (string.IsNullOrEmpty(header.MapName))
                        header.Title = sheetHeaders[header.ColumnIndex];
                    else
                    {
                        if (DataColumnsDic.ContainsKey(header.MapName) || EntityPropertiesDic.ContainsKey(header.MapName))
                            header.Title = header.MapName;
                        else
                            throw new Exception($"MapName在DataColumns或Properties中没有对应的项");
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(header.Title))
                    throw new Exception($"定义WriteHeaderMapper时ColumnIndex和Title不能同时为空");
                header.ColumnIndex = sheetHeaders[header.Title];
            }
            if (string.IsNullOrEmpty(header.MapName))
                header.MapName = header.Title;
        }
        // 2. 根据DataColumns或对象属性自动填充Headers
        if (AutoMapHeaders)
        {
            // DataTable
            if (DataColumnsDic.Count > 0)
            {
                foreach (var column in DataColumnsDic.Values)
                {
                    if (Headers.ContainsMapName(column.ColumnName))
                        continue;
                    var columnIndex = sheetHeaders.Contains(column.ColumnName)
                        ? sheetHeaders[column.ColumnName]
                        : column.Ordinal + StartColumnIndex;
                    var header = new WriteHeaderMapper
                    {
                        ColumnIndex = columnIndex,
                        Title = column.ColumnName,
                        MapName = column.ColumnName,
                        DotNetType = column.DataType
                    };
                    Headers.Add(header);
                }
            }
            else if (EntityPropertiesDic.Count > 0) // 对象
            {
                int idx = 0;
                foreach (var property in EntityPropertiesDic.Values)
                {
                    if (Headers.ContainsMapName(property.Name))
                        continue;
                    var columnIndex = sheetHeaders.Contains(property.Name)
                        ? sheetHeaders[property.Name]
                        : StartColumnIndex + idx;
                    var header = new WriteHeaderMapper
                    {
                        ColumnIndex = columnIndex,
                        Title = property.Name,
                        MapName = property.Name,
                        DotNetType = property.PropertyType
                    };
                    Headers.Add(header);
                    idx++;
                }
            }
            else
                throw new Exception("未知的AutoMapHeaders。必须存在DataTable或对象类型");
        }
    }
    private SheetHeaderCollection GetSheetHeaders(int columnCount)
    {
        var ret = new SheetHeaderCollection();
        for (int i = StartColumnIndex; i < columnCount + StartColumnIndex; i++)
        {
            string title = null;
            if (HeaderRowIndex > 0)
                title = Sheet.Cells[HeaderRowIndex, i].Text;
            if (string.IsNullOrEmpty(title))
                title = EPPlusUtil.ParseColumnString(i);
            ret.Add(i, title);
        }
        return ret;
    }
}