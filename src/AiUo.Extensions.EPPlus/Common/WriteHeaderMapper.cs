using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace AiUo.Extensions.EPPlus;

/// <summary>
/// 写入时header映射信息，可通过一下方式定位列
///     columnIndex, columnName, title(sheet必须有对应的title)
/// </summary>
public class WriteHeaderMapper : HeaderMapperBase
{
    public List<(int rowIndex, object value)> Properties = new List<(int rowIndex, object value)>();
    public Action<ExcelRangeBase, object> WriteValueAction { get; set; }
    public void WriteValue(ExcelRangeBase cell, object value)
    {
        if (WriteValueAction != null)
        {
            WriteValueAction(cell, value);
        }
        else
        {
            if (value == null || value == DBNull.Value)
                cell.Value = null;
            else
            {
                switch (DotNetType.FullName)
                {
                    case SimpleTypeNames.DateTime:
                        cell.Value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                        break;
                    case SimpleTypeNames.TimeSpan:
                        cell.Value = ((TimeSpan)value).ToString();
                        break;
                    case SimpleTypeNames.Int64:
                        var v1 = (long)value;
                        cell.Value = (v1 > 9223372036854700000L || v1 < -9223372036854700000L)
                            ? value.ToString() : value;
                        break;
                    case SimpleTypeNames.UInt64:
                        var v2 = (ulong)value;
                        cell.Value = v2 > 18446744073709500000UL ? value.ToString() : value;
                        break;
                    default:
                        cell.Value = AiUoUtil.ConvertTo(value, DotNetType, Formatter);
                        break;
                }
            }
        }
    }
}