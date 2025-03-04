using System;

namespace AiUo.Data.Schema;

/// <summary>
/// 试图的概要信息
/// </summary>
[Serializable]
public class ViewSchema: TableViewSchemaBase
{
    /// <summary>
    /// 试图名
    /// </summary>
    public string ViewName
    {
        get { return SourceName; }
        set { SourceName = value; }
    }
    /// <summary>
    /// SQL语句中使用的视图名称
    /// </summary>
    public string SqlViewName
    {
        get { return SqlSourceName; }
    }
    /// <summary>
    /// 视图的SQL查询语句
    /// </summary>
    public string Definition { get; set; }
    ///// <summary>
    ///// 创建时间
    ///// </summary>
    //public DateTime CreateTime { get; set; }
    /// <summary>
    /// 字符集
    /// </summary>
    public string CharSetName { get; set; }
}