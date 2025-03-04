using SqlSugar;
using SS = SqlSugar;
using AiUo.Logging;
using System.Linq.Expressions;

namespace AiUo.Data.SqlSugar;

public static class DbUtil
{
    #region Properties
    /// <summary>
    /// 通过IOC获取注入的Db对象
    /// </summary>
    internal static SqlSugarScope Db
        => (SqlSugarScope)DIUtil.GetRequiredService<ISqlSugarClient>();
    private static string _defaultConfigId;
    /// <summary>
    /// 默认configId
    /// </summary>
    public static string DefaultConfigId
        => _defaultConfigId ??= Convert.ToString(Db.CurrentConnectionConfig.ConfigId);
    #endregion

    #region GetDb
    public static ISqlSugarClient GetDb(SS.DbType dbType, string connectionString)
    {
        var ret = new SqlSugarClient(new ConnectionConfig
        {
            DbType = dbType,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
        });
        InitDb(ret, null);
        return ret;
    }

    /// <summary>
    /// 根据IDbSplitProvider获取ISqlSugarClient
    /// </summary>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static ISqlSugarClient GetDb(object splitDbKey = null)
        => GetDb<object>(splitDbKey);

    /// <summary>
    /// 根据IDbSplitProvider获取ISqlSugarClient
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static ISqlSugarClient GetDb<T>(object splitDbKey = null)
    {
        var configId = DIUtil.GetRequiredService<IDbSplitProvider>()
            .SplitDb<T>(splitDbKey);
        return GetDbById(configId);
    }

    /// <summary>
    /// 获取指定configId的ISqlSugarClient
    /// </summary>
    /// <param name="configId"></param>
    /// <returns></returns>
    public static ISqlSugarClient GetDbById(string configId = null)
    {
        // 主库
        if (string.IsNullOrEmpty(configId) || configId == DefaultConfigId)
            return Db.GetConnection(DefaultConfigId);

        var config = GetConfig(configId);
        if (!Db.IsAnyConnection(configId))
        {
            Db.AddConnection(config);
        }
        var ret = Db.GetConnection(configId);
        InitDb(ret, config);
        return ret;
    }
    #endregion

    #region Repository
    /// <summary>
    /// 创建Repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey">分库标识</param>
    /// <returns></returns>
    public static Repository<T> GetRepository<T>(object splitDbKey = null)
        where T : class, new()
    {
        return new Repository<T>(splitDbKey);
    }
    public static Repository<T> GetRepositoryById<T>(string configId = null)
        where T : class, new()
    {
        var db = GetDbById(configId);
        return new Repository<T>(db);
    }
    #endregion

    #region 常用方法=>Select|Delete|Insert|Update|InsertOrUpdate

    #region 查询=>Select

    #region Queryable
    /// <summary>
    /// 获取查询器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static ISugarQueryable<T> GetQueryable<T>(object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable<T>();
    public static ISugarQueryable<T, T2> GetQueryable<T, T2>(Expression<Func<T, T2, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3> GetQueryable<T, T2, T3>(Expression<Func<T, T2, T3, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3, T4> GetQueryable<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3, T4, T5> GetQueryable<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);

    /// <summary>
    /// 获取查询器(inner join)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static ISugarQueryable<T, T2> GetQueryable<T, T2>(Expression<Func<T, T2, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3> GetQueryable<T, T2, T3>(Expression<Func<T, T2, T3, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3, T4> GetQueryable<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public static ISugarQueryable<T, T2, T3, T4, T5> GetQueryable<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    #endregion

    #region SelectById
    /// <summary>
    /// 按单一主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<T> SelectByIdAsync<T>(dynamic id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(id);
    /// <summary>
    /// 按单一主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<List<T>> SelectByIdAsync<T>(List<dynamic> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(ids);
    /// <summary>
    /// 按联合主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<T> SelectByIdAsync<T>(T id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(id);
    /// <summary>
    /// 按联合主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<List<T>> SelectByIdAsync<T>(List<T> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(ids);
    #endregion

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<List<T>> SelectAllAsync<T>(object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetListAsync();

    /// <summary>
    /// 按条件查询
    /// </summary>
    /// <typeparam name="T">如：it=>it.Id==1 &amp;&amp; it.Age>10</typeparam>
    /// <param name="whereExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<List<T>> SelectAsync<T>(Expression<Func<T, bool>> whereExpression, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetListAsync(whereExpression);
    #endregion

    #region 删除=>Delete

    #region DeleteById
    /// <summary>
    /// 按单一主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteByIdAsync<T>(dynamic id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(id);
    /// <summary>
    /// 按单一主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteByIdAsync<T>(List<dynamic> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(ids);
    /// <summary>
    /// 按联合主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteByIdAsync<T>(T id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(id);
    /// <summary>
    /// 按联合主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<bool> DeleteByIdAsync<T>(List<T> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(ids);
    #endregion

    #region 实体
    /// <summary>
    /// 按实体删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<int> DeleteAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Deleteable<T>(item).ExecuteCommandAsync();
    public static async Task<int> DeleteAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Deleteable<T>(items).ExecuteCommandAsync();
    #endregion

    public static IDeleteable<T> GetDeleteable<T>(object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Deleteable<T>();

    public static async Task<int> DeleteAllAsync<T>(object splitDbKey = null)
        where T : class, new()
        => await GetDeleteable<T>(splitDbKey).ExecuteCommandAsync();

    /// <summary>
    /// 按条件删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="whereExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public static async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> whereExpression, object splitDbKey = null)
        where T : class, new()
        => await GetDeleteable<T>(splitDbKey).Where(whereExpression).ExecuteCommandAsync();
    #endregion

    #region 插入=>Insert
    public static async Task<int> InsertAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Insertable<T>(item).ExecuteCommandAsync();
    public static async Task<int> InsertAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Insertable<T>(items).ExecuteCommandAsync();
    #endregion

    #region 更新=>Update

    public static IUpdateable<T> GetUpdateable<T>(object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Updateable<T>();

    public static async Task<bool> UpdateAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).UpdateAsync(item);
    public static async Task<bool> UpdateAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).UpdateRangeAsync(items);

    public static async Task<int> UpdateAsync<T>(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        where T : class, new()
        => await GetDb<T>().Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync();

    #endregion

    #region 插入或更新=>InsertOrUpdate
    public static async Task<bool> InsertOrUpdateAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).InsertOrUpdateAsync(item);
    public static async Task<bool> InsertOrUpdateAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).InsertOrUpdateAsync(items);
    #endregion

    #endregion

    #region Utils
    internal static ConnectionElement GetConfig(string configId)
    {
        var provider = DIUtil.GetRequiredService<IDbConfigProvider>();
        var config = provider.GetConfig(configId);
        if (config == null)
            throw new Exception($"配置SqlSugar:ConnectionStrings没有找到连接。configId:{configId} type:{provider.GetType().FullName}");

        config.LanguageType = LanguageType.Chinese;
        config.IsAutoCloseConnection = true;
        return config;
    }
    internal static void InitDb(ISqlSugarClient db, ConnectionElement config)
    {
        // log
        if (config?.LogEnabled == true)
        {
            db.Aop.OnLogExecuting = (sql, paras) =>
            {
                var tmpSql = config.LogSqlMode switch
                {
                    0 => sql,
                    1 => UtilMethods.GetNativeSql(sql, paras),
                    2 => UtilMethods.GetSqlString(config.DbType, sql, paras),
                    _ => throw new Exception($"未知LogSqlMode模式: {config.LogSqlMode}")
                };
                LogUtil.Log(config.LogLevel, $"执行SQL: {tmpSql}");
            };
            db.Aop.OnLogExecuted = (sql, paras) =>
            {
                //var log = LogUtil.GetContextLog();
                //log.AddMessage($"SQL执行时间: {db.Ado.SqlExecutionTime.TotalMilliseconds}ms");
                //if (!log.IsContextLog)
                //    log.SetFlag("SqlSugar").Save();
            };
        }
        db.Aop.OnError = (ex) =>
        {
            // 无参数化
            var tmpSql = config?.LogSqlMode == 2
                ? UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres)
                : UtilMethods.GetNativeSql(ex.Sql, (SugarParameter[])ex.Parametres);

            LogUtil.GetContextLogger()
                .SetLevel(Microsoft.Extensions.Logging.LogLevel.Error)
                .AddMessage("SqlSugar:SQL执行异常")
                .AddField("SqlSugar.ConfigId", config?.ConfigId)
                .AddField("SqlSugar.SQL", tmpSql)
                .AddField("SqlSugar.Exception", ex)
                .Save();
        };

        //全局禁止读写分离
        if (config != null)
            db.Ado.IsDisableMasterSlaveSeparation = !config.SlaveEnabled;
    }
    #endregion
}