using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// 定义读取时header映射信息，可通过以下方式之一定位列
///     ColumnIndex, ColumnName, Title
/// 最终经过处理有效的是: ColumnIndex和MapToName
/// </summary>
public class ReadHeaderMapper : HeaderMapperBase
{
    // key: propertyKey value: propertyValue
    internal Dictionary<string, object> Properties = new Dictionary<string, object>();
    public ReadHeaderMapper() { }
    public ReadHeaderMapper(int columnIndex, string mapToName, Type dotNetType = null)
    {
        ColumnIndex = columnIndex;
        MapName = mapToName;
        DotNetType = dotNetType ?? typeof(string);
    }
    public ReadHeaderMapper(string title, string mapToName, Type dotNetType = null)
    {
        Title = title;
        MapName = mapToName;
        DotNetType = dotNetType ?? typeof(string);
    }

    public Func<ExcelRangeBase, object> ReadValueFunc { get; set; }
    public object ReadCellValue(ExcelRangeBase cell)
    {
        if (ReadValueFunc != null)
            return ReadValueFunc(cell);
        try
        {
            return AiUoUtil.ConvertTo(cell.Text, DotNetType, Formatter);
        }
        catch (Exception ex)
        {
            throw new Exception($"读取Excel的Cell值出现异常。cell:{cell.Address} value:{cell.Text} toType:{DotNetType} error:{ex.Message}");
        }
    }
}