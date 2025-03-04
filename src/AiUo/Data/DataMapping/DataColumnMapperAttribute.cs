using System;

namespace AiUo.Data.DataMapping;

/// <summary>
/// 实体类的属性和数据库表字段进行映射的类，用于ExecuteReader(T)()方法反射实现ORM映射
/// </summary>
public class DataColumnMapperAttribute : Attribute
{
    /// <summary>
    /// 获取映射的数据库表字段名
    /// </summary>
    public string ColumnName { get; internal set; }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="columnName">映射的数据库表字段名</param>
    public DataColumnMapperAttribute(string columnName = null)
    {
        ColumnName = columnName;
    }
}