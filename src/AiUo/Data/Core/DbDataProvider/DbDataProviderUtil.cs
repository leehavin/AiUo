using System;
using System.Collections.Generic;

namespace AiUo.Data;

/// <summary>
/// 数据提供程序辅助类
/// 可以通过DbProviderFactories.GetFactory(string)返回DbProviderFactory
/// </summary>
public class DbDataProviderUtil
{
    #region InvariantNames
    /// <summary>
    /// Odbc数据提供程序名称
    /// </summary>
    public const string OdbcName = "System.Data.Odbc";

    /// <summary>
    /// OleDb数据提供程序名称
    /// </summary>
    public const string OleDbName = "System.Data.OleDb";

    /// <summary>
    /// SqlClient数据提供程序名称
    /// </summary>
    public const string SqlClientName = "System.Data.SqlClient";

    /// <summary>
    /// SqlServerCeClient数据提供程序名称
    /// </summary>
    public const string SqlServerCeName = "Microsoft.SqlServerCe.Client";

    /// <summary>
    /// OracleClient数据提供程序名称，微软提供
    /// </summary>
    public const string OracleClientName = "System.Data.OracleClient";

    /// <summary>
    /// Oracle数据提供程序名称，Oracle公司提供 Oracle.ManagedDataAccess.Client
    /// </summary>
    public const string OdacName = "Oracle.ManagedDataAccess.Client";

    /// <summary>
    /// MySQL数据提供程序
    /// </summary>
    public const string MySqlClientName = "MySql.Data.MySqlClient";
    #endregion

    #region Static Constructor
    // 数据提供程序映射缓存
    private static readonly Dictionary<string, DbDataProvider> _providers = new Dictionary<string, DbDataProvider>();

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static DbDataProviderUtil()
    {
        // SQL Server
        _providers.Add(SqlClientName.ToLower(), DbDataProvider.SqlClient);
        _providers.Add("sqlclient", DbDataProvider.SqlClient);
        _providers.Add("sqlserver", DbDataProvider.SqlClient);
        _providers.Add("sql", DbDataProvider.SqlClient);
        _providers.Add("sqlfile", DbDataProvider.SqlClient);
        // Oracle
        _providers.Add(OracleClientName.ToLower(), DbDataProvider.Odac);
        _providers.Add("oracleclient", DbDataProvider.Odac);
        //_providers.Add(OracleClientName.ToLower(), DbDataProvider.OracleClient);
        //_providers.Add("oracleclient", DbDataProvider.OracleClient);
        // ODAC
        _providers.Add(OdacName.ToLower(), DbDataProvider.Odac);
        _providers.Add("oracle", DbDataProvider.Odac);
        _providers.Add("odac", DbDataProvider.Odac);
        _providers.Add("odp.net", DbDataProvider.Odac);
        // MysQL
        _providers.Add(MySqlClientName.ToLower(), DbDataProvider.MySqlClient);
        _providers.Add("mysql", DbDataProvider.MySqlClient);
        // Oledb
        _providers.Add(OleDbName.ToLower(), DbDataProvider.OleDb);
        _providers.Add("oledb", DbDataProvider.OleDb);
        _providers.Add("access", DbDataProvider.OleDb);
        // ODBC
        _providers.Add(OdbcName.ToLower(), DbDataProvider.Odbc);
        _providers.Add("odbc", DbDataProvider.Odbc);
    }

    #endregion

    /// <summary>
    /// 获取数据提供程序类型
    /// </summary>
    /// <param name="providerName">数据提供程序名称</param>
    /// <returns></returns>
    public static DbDataProvider GetProvider(string providerName)
    {
        DbDataProvider ret = DbDataProvider.Unknown;
        string name = providerName.ToLower();
        if (_providers.ContainsKey(name))
            ret = _providers[name];
        else
            throw new ArgumentException(string.Format("不支持此数据提供程序名，请检查输入 {0} 是否正确。", providerName), "providerName");
        return ret;
    }

    /// <summary>
    /// 根据数据库类型获得数据提供程序类型
    /// </summary>
    /// <param name="databaseType"></param>
    /// <returns></returns>
    public static DbDataProvider GetProvider(Type databaseType)
    {
        var ret = DbDataProvider.Unknown;
        switch (databaseType.Name)
        {
            case "SqlDatabase":
                ret = DbDataProvider.SqlClient;
                break;
            case "MySqlDatabase":
                ret = DbDataProvider.MySqlClient;
                break;
            case "OleDbDatabase":
                ret = DbDataProvider.OleDb;
                break;
            case "OracleDatabase":
                ret = DbDataProvider.Odac;
                break;
        }
        return ret;
    }

    /// <summary>
    /// 获取DbProviderFactories.GetFactory所使用的提供程序名称
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static string GetInvariantName(DbDataProvider provider)
    {
        string ret = null;
        switch (provider)
        {
            case DbDataProvider.SqlClient:
                ret = SqlClientName;
                break;
            case DbDataProvider.OracleClient:
                ret = OracleClientName;
                break;
            case DbDataProvider.Odac:
                ret = OdacName;
                break;
            case DbDataProvider.OleDb:
                ret = OleDbName;
                break;
            case DbDataProvider.Odbc:
                ret = OdbcName;
                break;
            case DbDataProvider.SqlServerCe:
                ret = SqlServerCeName;
                break;
            case DbDataProvider.MySqlClient:
                ret = MySqlClientName;
                break;
            default:
                throw new ArgumentException(string.Format("不只支持此DbDataProvider，请检查{0}是否正确。", provider.ToString()), "provider");
        }
        return ret;
    }
    /// <summary>
    /// 获取DbProviderFactories.GetFactory所使用的提供程序名称
    /// </summary>
    /// <param name="providerName"></param>
    /// <returns></returns>
    public static string GetInvariantName(string providerName)
        => GetInvariantName(GetProvider(providerName));

    /// <summary>
    /// 获取数据库类型
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static DatabaseType GetDatabaseType(DbDataProvider provider)
    {
        DatabaseType ret = DatabaseType.Unknown;
        switch (provider)
        {
            case DbDataProvider.MySqlClient:
                ret = DatabaseType.MySql;
                break;
            case DbDataProvider.SqlClient:
                ret = DatabaseType.SqlServer;
                break;
            case DbDataProvider.Odac:
            case DbDataProvider.OracleClient:
                ret = DatabaseType.Oracle;
                break;
        }
        return ret;
    }
}