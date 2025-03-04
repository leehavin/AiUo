using System.Data;
using MySql.Data.MySqlClient;

namespace AiUo.Data.MySql;

/// <summary>
/// MySql数据库SQL语句操作类
/// </summary>
public class MySqlSqlDao : DaoBase<MySqlParameter, MySqlDbType>
{
    #region Constructs
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="database">数据库访问对象</param>
    public MySqlSqlDao(string sql, Database database)
        : base(sql, CommandType.Text, database) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    public MySqlSqlDao(string sql)
        : base(sql, CommandType.Text, new MySqlDatabase()) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="connectionStringName">数据库连接字符串名称</param>
    public MySqlSqlDao(string sql, string connectionStringName)
        : base(sql, CommandType.Text, new MySqlDatabase(connectionStringName)) { }


    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="commandTimeout">Timeout时间</param>
    public MySqlSqlDao(string sql, string connectionString, int commandTimeout)
        : base(sql, CommandType.Text, new MySqlDatabase(connectionString, commandTimeout)) { }

    #endregion

    /// <summary>
    /// 设置MySqlParameter的MySqlDbType
    /// </summary>
    /// <param name="para"></param>
    /// <param name="dbType"></param>
    protected override void SetParameterDbType(MySqlParameter para, MySqlDbType dbType)
    {
        para.MySqlDbType = dbType;
    }
}

/// <summary>
/// MySQL的Dao基类
/// </summary>
public class MySqlProcDao : DaoBase<MySqlParameter, MySqlDbType>
{
    #region Constructs
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="database">数据库访问对象</param>
    public MySqlProcDao(string proc, Database database)
        : base(proc, CommandType.StoredProcedure, database) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    public MySqlProcDao(string proc)
        : base(proc, CommandType.StoredProcedure, new MySqlDatabase()) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="connectionStringName">数据库连接字符串名称</param>
    public MySqlProcDao(string proc, string connectionStringName)
        : base(proc, CommandType.StoredProcedure, new MySqlDatabase(connectionStringName)) { }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="proc">存储过程名称</param>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <param name="commandTimeout">commandTimeout</param>
    public MySqlProcDao(string proc, string connectionString, int commandTimeout)
        : base(proc, CommandType.StoredProcedure, new MySqlDatabase(connectionString, commandTimeout)) { }
    #endregion

    /// <summary>
    /// 设置MySqlParameter的MySqlDbType
    /// </summary>
    /// <param name="para"></param>
    /// <param name="dbType"></param>
    protected override void SetParameterDbType(MySqlParameter para, MySqlDbType dbType)
    {
        para.MySqlDbType = dbType;
    }
}