using System;
using AiUo.Data.Instrumentation;

namespace AiUo.Data;

/// <summary>
/// 数据库连接配置信息类，用于DbFactory构造Database用
/// </summary>
[Serializable]
public class ConnectionStringConfig
{
    /// <summary>
    /// 配置中数据库连接字符串名
    /// </summary>
    public string ConnectionStringName { get; set; } = "UserCustom";
        
    /// <summary>
    /// 数据提供程序
    /// </summary>
    public DbDataProvider Provider { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 只读数据库连接字符串
    /// </summary>
    public string ReadConnectionString { get; set; }

    /// <summary>
    /// Command执行的Timeout单位秒
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 性能侦测提供程序
    /// </summary>
    public IDataInstProvider InstProvider { get; set; }
}