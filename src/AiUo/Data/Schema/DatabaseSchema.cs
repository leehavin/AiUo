using System;
using System.ComponentModel;

namespace AiUo.Data.Schema;

/// <summary>
/// 数据库的概要信息
/// </summary>
[DefaultProperty("DatabaseName")]
[Serializable]
public class DatabaseSchema
{
    /// <summary>
    /// 数据库名
    /// </summary>
    [Category("基础属性")]
    [Description("数据库名称")]
    [Browsable(true)]
    public string DatabaseName { get; set; }
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    [Category("基础属性")]
    [Description("数据库连接字符串")]
    [Browsable(true)]
    public string ConnectionString { get; set; }
    /// <summary>
    /// 数据库连接字符串信息
    /// </summary>
    public ConnectionStringInfo ConnectionStringInfo { get; set; }
        
    /// <summary>
    /// 服务器地址
    /// </summary>
    [Category("基础属性")]
    [Description("服务器地址")]
    [Browsable(true)]
    public string DataSource
    {
        get
        {
            if (ConnectionStringInfo == null)
                throw new Exception("未知的ConnectionStringInfo");
            return ConnectionStringInfo.DataSource;
        }
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [Category("基础属性")]
    [Description("数据提供程序")]
    [Browsable(true)]
    public DbDataProvider DbDataProvider { get; set; } = DbDataProvider.Unknown;
    /// <summary>
    /// 表集合
    /// </summary>
    public SchemaCollection<TableSchema> Tables = new SchemaCollection<TableSchema>();
    /// <summary>
    /// 视图集合
    /// </summary>
    public SchemaCollection<ViewSchema> Views = new SchemaCollection<ViewSchema>();
    /// <summary>
    /// 存储过程集合
    /// </summary>
    public SchemaCollection<ProcSchema> Procs = new SchemaCollection<ProcSchema>();

    /// <summary>
    /// 默认字符集
    /// </summary>
    [Category("基础属性")]
    [Description("默认字符集")]
    [Browsable(true)]
    public string DefaultCharSetName { get; set; }
    /// <summary>
    /// 默认排序规则
    /// </summary>
    [Category("基础属性")]
    [Description("默认排序规则")]
    [Browsable(true)]
    public string DefaultCollationName { get; set; }
}