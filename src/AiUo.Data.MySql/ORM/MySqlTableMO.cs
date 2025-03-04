
using MySql.Data.MySqlClient;
using AiUo.Data.ORM;

namespace AiUo.Data.MySql;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class MySqlTableMO<TEntity> : DbTableMO<MySqlDatabase, MySqlParameter, MySqlDbType, TEntity>
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
    public MySqlTableMO(MySqlDatabase database)
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
    public MySqlTableMO(string connectionString, int commandTimeout)
        : this(new MySqlDatabase(connectionString, commandTimeout)) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name = "connectionStringName">连接字符串名称</param>
    public MySqlTableMO(string connectionStringName = null)
        => Init(connectionStringName, (config) => new MySqlDatabase(config));

    #endregion
}