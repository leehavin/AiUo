using System;
using System.ComponentModel;

namespace AiUo.Data.Schema;

/// <summary>
/// Table和View的Schema基类
/// </summary>
[Serializable]
public class TableViewSchemaBase : DBObjectSchemaBase, ISchemaCollectionKey
{
    /// <summary>
    /// 列集合
    /// </summary>
    public SchemaCollection<ColumnSchema> Columns = new SchemaCollection<ColumnSchema>();
    /// <summary>
    /// 排序规则
    /// </summary>
    [Category("扩展属性")]
    [Description("排序规则")]
    [Browsable(true)]
    public string CollationName { get; set; }
    /// <summary>
    /// 获取Key
    /// </summary>
    /// <returns></returns>
    public string GetKey()
    {
        return SourceName;
    }
}