using System;

namespace AiUo.Data.Schema;

/// <summary>
/// 表的外键概要信息
/// </summary>
[Serializable]
public class ForeignKeySchema: TableObjectSchemaBase
{
    /// <summary>
    /// 外键名称
    /// </summary>
    public string ForeignKeyName { get; set; }
    /// <summary>
    /// 外键关联的主表名称
    /// </summary>
    public string ReferenceTableName { get; set; }

    /// <summary>
    /// 外键关联的主表
    /// </summary>
    public TableSchema ReferenceTable { get; set; }
    /// <summary>
    /// 外键关联的主表的字段集合
    /// </summary>
    public SchemaCollection<ColumnSchema> ReferenceColumns = new SchemaCollection<ColumnSchema>();

}