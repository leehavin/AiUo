using System;
using System.ComponentModel;

namespace AiUo.Data.Schema;

/// <summary>
/// 数据库对象Schema基类
/// </summary>
[Serializable]
public class DBObjectSchemaBase
{
    /// <summary>
    /// 数据库名
    /// </summary>
    [Category("基础属性")]
    [Description("数据库名")]
    [Browsable(true)]
    public string DatabaseName { get; set; }
    /// <summary>
    /// 数据库类型
    /// </summary>
    [Category("代码生成辅助")]
    [Description("连接数据库的数据提供程序类型(包括Oracle的ODAC和MySql的Connector Net)")]
    [Browsable(true)]
    public DbDataProvider DbDataProvider { get; set; } = DbDataProvider.Unknown;
    /// <summary>
    /// 所属Database
    /// </summary>
    public DatabaseSchema Database { get; set; }
        
    /// <summary>
    /// 表名或视图名 User
    /// </summary>
    [Browsable(false)]
    public string SourceName { get; set; }
        
    /// <summary>
    /// SQL语句中Table名或View名的表示，如：`User` 或 [User]
    /// </summary>
    public string SqlSourceName => SchemaUtil.GetSqlName(DbDataProvider, SourceName);

    /// <summary>
    /// 描述
    /// </summary>
    [Category("基础属性")]
    [Description("对象的描述信息")]
    [Browsable(true)]
    public string Comment { get; set; }
    /// <summary>
    /// 注释的第一行信息
    /// </summary>
    public string CommentFirst => StringUtil.GetFirstLine(Comment).Trim();
}