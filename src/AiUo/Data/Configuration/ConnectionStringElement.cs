namespace AiUo.Data;

/// <summary>
/// 连接字符串配置节
/// </summary>
public class ConnectionStringElement
{
    /// <summary>
    /// 读写数据库连接字符串
    /// </summary>
    public string ConnectionString { get; set; }
        
    /// <summary>
    /// 只读数据库连接字符串
    /// </summary>
    public string ReadConnectionString { get; set; }
        
    /// <summary>
    /// 数据提供程序名称。
    /// ODBC: System.Data.Odbc ,odbc
    /// OleDB: System.Data.OleDb ,oledb,access
    /// Sql Server: System.Data.SqlClient ,sqlclient,sqlserver,sql,sqlfile
    /// Oracle8.1.7-9iR2: System.Data.OracleClient,oracleclient
    /// Oracle9iR2以上: Oracle.ManagedDataAccess.Client,oracle,odac,odp.net
    /// MySQL: mysql
    /// </summary>
    public string ProviderName { get; set; }

    /// <summary>
    /// 加密方式:none,password,all
    /// </summary>
    public string Encrypt { get; set; }
        
    /// <summary>
    /// Command执行SQL时的Timeout时间，单位秒，默认30秒
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
        
    /// <summary>
    /// 跟踪服务提供程序
    /// </summary>
    public string InstProvider { get; set; }

    /// <summary>
    /// Orm的MO对象创建时Database通过MO的namespace映射，用;分隔
    /// </summary>
    public string OrmMap { get; set; }

}