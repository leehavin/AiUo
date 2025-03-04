using System.ComponentModel;

namespace AiUo.Data;

/// <summary>
/// 连接数据库的数据提供程序类型(包括Oracle的ODAC和MySql的Connector Net)
/// </summary>
[Description("数据提供程序")]
public enum DbDataProvider
{
    /// <summary>
    /// 未知的数据提供程序类型
    /// </summary>
    [Description("未知的数据提供程序")]
    Unknown = 0,

    /// <summary>
    /// 用于 ODBC 的 .NET Framework 数据提供程序。命名空间System.Data.Odbc
    /// </summary>
    //[Obsolete("已过时，暂不支持")]
    [Description("ODBC的.NET数据提供程序")]
    Odbc = 1,

    /// <summary>
    /// 用于 OLE DB 的 .NET Framework 数据提供程序。命名空间System.Data.OleDb
    /// </summary>
    [Description("OLE DB的.NET数据提供程序")]
    OleDb = 2,

    /// <summary>
    /// 用于 SQL Server 的 .NET Framework 数据提供程序。命名空间System.Data.SqlClient
    /// </summary>
    [Description("SQL Server的.NET数据提供程序")]
    SqlClient = 3,

    /// <summary>
    /// 用于 Oracle 的 .NET Framework 数据提供程序,只支持Oracle8.1.7-9iR2版本。命名空间 System.Data.OracleClient
    /// </summary>
    [Description("Oracle的.NET数据提供程序")]
    //[Obsolete("建议使用第三方Oracle提供的驱动程序ODP.NET")]
    OracleClient = 4,

    /// <summary>
    /// 用于 SQL Server Compact 4.0 的托管数据提供程序。命名空间System.Data.SqlServerCe
    /// </summary>
    //[Obsolete("暂不支持")]
    [Description("SQL Server Compact的.NET数据提供程序")]
    SqlServerCe = 5,

    /// <summary>
    /// 用于 Oracle 的ODAC(ODP.NET)数据提供程序。命名空间Oracle.ManagedDataAccess.Client
    /// </summary>
    [Description("Oracle的ODAC(ODP.NET)数据提供程序")]
    Odac = 6,

    /// <summary>
    /// 用于 MySQL 的MySQL Connector Net数据提供程序。命名空间MySql.Data.MySqlClient
    /// </summary>
    [Description("MySQL的Connector/Net数据提供程序")]
    MySqlClient = 7
}