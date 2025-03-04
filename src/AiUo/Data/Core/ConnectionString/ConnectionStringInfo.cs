using System;

namespace AiUo.Data;

/// <summary>
/// 连接字符串信息
/// </summary>
[Serializable]
public class ConnectionStringInfo
{
    /// <summary>
    /// 数据提供程序
    /// </summary>
    public DbDataProvider Provider { get; set; }
        
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }
        
    /// <summary>
    /// 获取或设置要连接到的数据源的名称。服务器地址或服务名
    /// </summary>
    public string DataSource { get; set; }
        
    /// <summary>
    /// 数据库名
    /// </summary>
    public string Database { get; set; }
        
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }
        
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserID { get; set; }
        
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }
        
    /// <summary>
    /// 连接超时
    /// </summary>
    public int ConnectTimeout { get; set; }
}