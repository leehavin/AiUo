using System;

namespace AiUo.Extensions.EPPlus;

public class HeaderMapperBase : IComparable<HeaderMapperBase>
{
    /// <summary>
    /// 通过Excel列索引定位，从1开始
    /// </summary>
    public int ColumnIndex { get; set; } = 0;
    /// <summary>
    /// 通过Excel列索引名定位，如A
    /// </summary>
    public string ColumnName
    {
        get { return EPPlusUtil.ParseColumnString(ColumnIndex); }
        set { ColumnIndex = EPPlusUtil.ParseColumnIndex(value); }
    }
    /// <summary>
    /// 通过Excel的header标题文本定位
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 映射的DataColumn名称或者对象属性名称
    /// </summary>
    public string MapName { get; set; }
    public Type DotNetType { get; set; } = typeof(string);
    /// <summary>
    /// cell数据转换格式化器，常用于日期类型
    /// </summary>
    public string Formatter { get; set; }

    public int CompareTo(HeaderMapperBase other) => ColumnIndex.CompareTo(other.ColumnIndex);
}