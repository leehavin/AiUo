using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AiUo.Data.Schema;

/// <summary>
/// MySQL表的概要信息
/// </summary>
[DefaultProperty("TableName")]
[Serializable]
public class TableSchema : TableViewSchemaBase
{
    #region 基础属性
    /// <summary>
    /// 表名
    /// </summary>
    [Category("基础属性")]
    [Description("数据库表名")]
    [Browsable(true)]
    public string TableName
    {
        get { return SourceName; }
        set { SourceName = value; }
    }
    /// <summary>
    /// SQL语句中使用的名称
    /// </summary>
    public string SqlTableName
    {
        get { return SqlSourceName; }
    }
    /// <summary>
    /// 主键
    /// </summary>
    [Browsable(false)]
    public PrimaryKeySchema PrimaryKey { get; set; } = new PrimaryKeySchema();
    /// <summary>
    /// 外键集合
    /// </summary>
    public List<ForeignKeySchema> ForeignKeys = new List<ForeignKeySchema>();
    /// <summary>
    /// 索引集合
    /// </summary>
    public List<IndexSchema> Indexes = new List<IndexSchema>();
    /// <summary>
    /// 是否有主键
    /// </summary>
    [Category("基础属性")]
    [Description("是否有主键")]
    [Browsable(true)]
    public bool HasPrimaryKey { get { return PrimaryKey != null && PrimaryKey.Columns.Count > 0; } }
    /// <summary>
    /// 是否有外键
    /// </summary>
    [Category("基础属性")]
    [Description("是否有外键")]
    [Browsable(true)]
    public bool HasForeignKey { get { return ForeignKeys != null && ForeignKeys.Count > 0; } }
    /// <summary>
    /// 是否有唯一索引
    /// </summary>
    [Category("基础属性")]
    [Description("是否有唯一索引")]
    [Browsable(true)]
    public bool HasUniqueIndex
    {
        get
        {
            return Indexes.Any(item =>
            {
                return item.IsUnique && !item.IsPrimaryKey;
            });
        }
    }
    #endregion

    #region 扩展属性
    /// <summary>
    /// 创建时间
    /// </summary>
    [Category("扩展属性")]
    [Description("创建时间")]
    [Browsable(true)]
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// 修改时间
    /// </summary>
    [Category("扩展属性")]
    [Description("修改时间")]
    [Browsable(true)]
    public DateTime UpdateTime { get; set; }
    /// <summary>
    /// 索引长度
    /// </summary>
    [Category("扩展属性")]
    [Description("索引长度")]
    [Browsable(true)]
    public long IndexLength { get; set; }
    /// <summary>
    /// 记录数
    /// </summary>
    [Category("扩展属性")]
    [Description("记录数")]
    [Browsable(true)]
    public long TableRows { get; set; }
    /// <summary>
    /// 平均行大小
    /// </summary>
    [Category("扩展属性")]
    [Description("平均行大小")]
    [Browsable(true)]
    public long AvgRowLength { get; set; }
    /// <summary>
    /// 数据总大小
    /// </summary>
    [Category("扩展属性")]
    [Description("数据总大小")]
    [Browsable(true)]
    public long DataLength { get; set; }
    /// <summary>
    /// 自增字段值，没有此字段返回-1
    /// </summary>
    [Category("基础属性")]
    [Description("自增字段值，没有此字段返回-1")]
    [Browsable(false)]
    public long AutoIncrementValue { get; set; }
    #endregion

    #region 代码生成辅助
    private ColumnSchema _autoIncrementColumn;
    /// <summary>
    /// 表中是否存在自增字段
    /// </summary>
    [Category("基础属性")]
    [Description("表中是否存在自增字段")]
    [Browsable(true)]
    public bool HasAutoIncrementColumn
    {
        get
        {
            if (_autoIncrementColumn != null) return true;
            if (PrimaryKey == null) return false;
            _autoIncrementColumn = PrimaryKey.Columns.SingleOrDefault(item => { return item.IsAutoIncrement; });
            return _autoIncrementColumn != null;
        }
    }
    /// <summary>
    /// 获取表中的自增字段
    /// </summary>
    public ColumnSchema AutoIncrementColumn
    {
        get
        {
            if (_autoIncrementColumn != null) return _autoIncrementColumn;
            if (PrimaryKey == null) return null;
            _autoIncrementColumn = PrimaryKey.Columns.SingleOrDefault(item => { return item.IsAutoIncrement; });
            return _autoIncrementColumn;
        }
    }

    /// <summary>
    /// 字段集合过滤器，按照ColumnSelectMode设置进行
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public IEnumerable<ColumnSchema> ColumnsFilter(ColumnSelectMode mode = ColumnSelectMode.All)
    {
        IEnumerable<ColumnSchema> ret = null;
        switch (mode)
        {
            case ColumnSelectMode.All:
                ret = Columns;
                break;
            case ColumnSelectMode.NoAutoIncrement:
                ret = Columns.Where(column => { return !column.IsAutoIncrement; });
                break;
            case ColumnSelectMode.NoOnePK:
                ret = Columns.Where(column => { return !column.IsSinglePKColumn; });
                break;
            case ColumnSelectMode.NoOneUnique:
                ret = Columns.Where(column => { return !column.IsSingleUniqueColumn && !column.IsSinglePKColumn; });
                break;
            case ColumnSelectMode.NoPK:
                ret = Columns.Where(column => { return !column.IsPrimaryKey; });
                break;
            case ColumnSelectMode.CanInsertAndUpdate:
                ret = Columns.Where(column => {
                    if (column.IsAutoIncrement) return false;
                    if (!string.IsNullOrEmpty(column.DefaultValue) && column.DefaultValue.Contains("CURRENT_TIMESTAMP")) return false;
                    return true;
                });
                break;
        }
        return ret;
    }

    #endregion
}