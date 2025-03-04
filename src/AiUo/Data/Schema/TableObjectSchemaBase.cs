using System;

namespace AiUo.Data.Schema;

/// <summary>
/// 表的所属对象主键，外键，索引Schema父类
/// </summary>
[Serializable]
public class TableObjectSchemaBase
{
    private DbDataProvider _dbDataProvider = DbDataProvider.Unknown;
    /// <summary>
    /// 数据提供程序
    /// </summary>
    public DbDataProvider DbDataProvider { get { return _dbDataProvider; } set { _dbDataProvider = value; } }
    /// <summary>
    /// 所在表名
    /// </summary>
    public string TableName { get; set; }
    /// <summary>
    /// 主键所在的表
    /// </summary>
    public TableSchema Table { get; set; }

    /// <summary>
    /// 包含的字段集合
    /// </summary>
    public SchemaCollection<ColumnSchema> Columns = new SchemaCollection<ColumnSchema>();
}