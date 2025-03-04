using AiUo.Data.ORM;
using MySql.Data.MySqlClient;

namespace AiUo.Data.MySql;

/// <summary>
/// MySQL 视图操作类MO基类
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class MySqlViewMO<TEntity> : DbViewMO<MySqlDatabase, MySqlParameter, MySqlDbType, TEntity>
    where TEntity : IRowMapper<TEntity>
{
    /// <summary>
    /// MySqlOrmProvider
    /// </summary>
    protected override IDbOrmProvider<MySqlDatabase, MySqlParameter, MySqlDbType> OrmProvider => new MySqlOrmProvider();
    /// <summary>
    /// DbDataProvider.MySqlClient
    /// </summary>
    public override DbDataProvider Provider => DbDataProvider.MySqlClient;

    #region Constructors
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="database"></param>
    public MySqlViewMO(MySqlDatabase database)
    {
        if (database != null)
            Database = database;
        else
            Init(null, (config) => new MySqlDatabase(config));
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="connectionString">MySql数据库连接字符串，如server=192.168.1.1;database=testdb;uid=root;pwd=root</param>
    /// <param name="commandTimeout">Command的Timeout时间</param>
    public MySqlViewMO(string connectionString, int commandTimeout)
        : this(new MySqlDatabase(connectionString, commandTimeout)) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name = "connectionStringName">连接字符串名称</param>
    public MySqlViewMO(string connectionStringName = null)
        => Init(connectionStringName, (config) => new MySqlDatabase(config));
    #endregion
}