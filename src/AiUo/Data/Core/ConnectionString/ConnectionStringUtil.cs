namespace AiUo.Data;

/// <summary>
/// 数据库连接字符串辅助类
/// </summary>
public static class ConnectionStringUtil
{
    /// <summary>
    /// 获取SQL Server连接字符串
    /// </summary>
    /// <param name="server">要连接到的 SQL Server 实例的名称或网络地址</param>
    /// <param name="database">与该连接关联的数据库的名称</param>
    /// <param name="userid">接到 SQL Server 时要使用的用户 ID，如为空，则使用集成认证</param>
    /// <param name="password">SQL Server 帐户的密码</param>
    /// <param name="configs">其他数据库连接字符串配置</param>
    /// <returns></returns>
    public static string GetSqlServer(string server, string database, string userid, string password, params string[] configs)
    {
        string ret = null;
        server = string.IsNullOrEmpty(server) ? "(local)" : server;
        ret = string.IsNullOrEmpty(userid) 
            ? $"Data Source={server};Initial Catalog={database};Integrated Security=True;"
            : $"Data Source={server};Initial Catalog={database};User ID={userid};Password={password};";
        if (configs != null)
        {
            foreach (var item in configs)
            {
                ret += item + ";";
            }
        }
        return ret;
    }

    /// <summary>
    /// 获取Access连接字符串
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="userid"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string GetAccess(string fileName, string userid = null, string password = null)
    {
        return (string.IsNullOrEmpty(userid))
            ? $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={fileName};User ID=Admin;Password=;"
            : $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={fileName};User ID={userid};Password={password};";
    }

    /// <summary>
    /// 获取Excel连接字符串
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="hdr">第一行是否字段名</param>
    /// <param name="imex">0-写入 1-读取 2-读写</param>
    /// <returns></returns>
    public static string GetExcel(string filename, bool hdr = true, int imex = 2)
    {
        return string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR={1};IMEX={2}'"
            , filename, (hdr) ? "YES" : "NO", imex);
    }

    /// <summary>
    /// 获取Oracle连接字符串
    /// </summary>
    /// <param name="dataSource">如：//172.28.8.31/ServiceName</param>
    /// <param name="userid"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string GetOracle(string dataSource, string userid, string password)
        => $"Data Source={dataSource};User ID={userid};Password={password};";

    /// <summary>
    /// 获取Oracle连接字符串
    /// </summary>
    /// <param name="host">主机IP</param>
    /// <param name="port">端口1521</param>
    /// <param name="serviceName">服务名</param>
    /// <param name="userid">用户名</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    public static string GetOracle(string host, int port, string serviceName, string userid, string password)
    {
        string ret = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" 
                     + $"(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={userid};Password={password};";
        return ret;
    }

    /// <summary>
    /// 获取MySQL连接字符串
    /// </summary>
    /// <param name="server"></param>
    /// <param name="database"></param>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="configs"></param>
    /// <returns></returns>
    public static string GetMySql(string server, string database, string user, string password, params string[] configs)
    {
        // Charset = utf8;
        string ret = $"server={server};database={database};user={user};password={password};Allow User Variables=True;";
        if (configs != null)
        {
            foreach (var item in configs)
            {
                ret += item + ";";
            }
        }
        return ret;
    }
}