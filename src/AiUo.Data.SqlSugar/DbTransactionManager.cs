using SqlSugar;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using AiUo.Logging;

namespace AiUo.Data.SqlSugar;

/// <summary>
/// SqlSugar事务管理对象
/// </summary>
public class DbTransactionManager
{
    #region Properties
    private SqlSugarClient _db;
    public IsolationLevel IsolationLevel { get; }
    private readonly Exception _exception = null;
    private DbTransactionStatus _status = DbTransactionStatus.Init;

    public DbTransactionManager(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        IsolationLevel = isolationLevel;
        _db = DbUtil.Db.CopyNew();
        var stack = new StackTrace(0, true);
        _exception = new Exception($"DbTransactionManager对象在析构函数中调用释放，请显示调用Commit()或Rollback()释放资源。StackTrace:{stack.ToString()}");
    }
    #endregion

    #region Callback
    /// <summary>
    /// 执行Callback时是否抛出异常
    /// </summary>
    public bool ThrowCallbackException { get; set; } = true;
    private List<Action> _commitCallbacks = new List<Action>();
    private List<Action> _rollbackCallbacks = new List<Action>();
    public void AddCommitCallback(Action callback)
        => _commitCallbacks.Add(callback);
    public void AddRollbackCallback(Action callback)
        => _rollbackCallbacks.Add(callback);
    #endregion

    #region Begin
    public void Begin()
    {
        CheckBeginTran();
        _db.BeginTran(IsolationLevel);
        _status = DbTransactionStatus.TranBegined;
    }
    public async Task BeginAsync()
    {
        CheckBeginTran();
        await _db.BeginTranAsync(IsolationLevel);
        _status = DbTransactionStatus.TranBegined;
    }
    private void CheckBeginTran()
    {
        if (_status != DbTransactionStatus.Init)
            throw new Exception("DbTransactionManager执行Begin()时Status必须是Init");
    }
    #endregion

    #region GetDb & GetRepository
    public Repository<T> GetRepository<T>(object splitDbKey = null)
        where T : class, new()
    {
        var db = GetDb<T>(splitDbKey);
        return new Repository<T>(db);
    }
    public ISqlSugarClient GetDb(object splitDbKey = null)
        => GetDb<object>(splitDbKey);
    public ISqlSugarClient GetDb<T>(object splitDbKey = null)
    {
        var configId = DIUtil.GetRequiredService<IDbSplitProvider>()
            .SplitDb<T>(splitDbKey);
        return GetDbById(configId);
    }
    public ISqlSugarClient GetDbById(string configId = null)
    {
        if (_status == DbTransactionStatus.End)
            throw new Exception("DbTransactionManager执行GetDb()时已经Commit或者Rollback(Status=End)");
        if (_status == DbTransactionStatus.Init)
            Begin();
        _status = DbTransactionStatus.TranUsed;

        // 主库
        if (string.IsNullOrEmpty(configId) || configId == DbUtil.DefaultConfigId)
            return _db.GetConnection(DbUtil.DefaultConfigId);

        var config = DbUtil.GetConfig(configId);
        if (!_db.IsAnyConnection(configId))
        {
            _db.AddConnection(config);
        }
        var ret = _db.GetConnection(configId);
        DbUtil.InitDb(ret, config);
        return ret;
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
    public ISugarQueryable<T> GetQueryable<T>(object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable<T>();
    public ISugarQueryable<T, T2> GetQueryable<T, T2>(Expression<Func<T, T2, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3> GetQueryable<T, T2, T3>(Expression<Func<T, T2, T3, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3, T4> GetQueryable<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3, T4, T5> GetQueryable<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, JoinQueryInfos>> joinExpression, object splitDbKey = null)
        => GetDb<T>(splitDbKey).Queryable(joinExpression);

    /// <summary>
    /// 获取查询器(inner join)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public ISugarQueryable<T, T2> GetQueryable<T, T2>(Expression<Func<T, T2, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3> GetQueryable<T, T2, T3>(Expression<Func<T, T2, T3, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3, T4> GetQueryable<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression, object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Queryable(joinExpression);
    public ISugarQueryable<T, T2, T3, T4, T5> GetQueryable<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression, object splitDbKey = null)
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
    public async Task<T> SelectByIdAsync<T>(dynamic id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(id);
    /// <summary>
    /// 按单一主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<List<T>> SelectByIdAsync<T>(List<dynamic> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(ids);
    /// <summary>
    /// 按联合主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<T> SelectByIdAsync<T>(T id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(id);
    /// <summary>
    /// 按联合主键查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<List<T>> SelectByIdAsync<T>(List<T> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetByIdAsync(ids);
    #endregion

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<List<T>> SelectAllAsync<T>(object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).GetListAsync();

    /// <summary>
    /// 按条件查询
    /// </summary>
    /// <typeparam name="T">如：it=>it.Id==1 &amp;&amp; it.Age>10</typeparam>
    /// <param name="whereExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<List<T>> SelectAsync<T>(Expression<Func<T, bool>> whereExpression, object splitDbKey = null)
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
    public async Task<bool> DeleteByIdAsync<T>(dynamic id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(id);
    /// <summary>
    /// 按单一主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<bool> DeleteByIdAsync<T>(List<dynamic> ids, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(ids);
    /// <summary>
    /// 按联合主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<bool> DeleteByIdAsync<T>(T id, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).DeleteByIdAsync(id);
    /// <summary>
    /// 按联合主键删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ids"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<bool> DeleteByIdAsync<T>(List<T> ids, object splitDbKey = null)
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
    public async Task<int> DeleteAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Deleteable<T>(item).ExecuteCommandAsync();
    public async Task<int> DeleteAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Deleteable<T>(items).ExecuteCommandAsync();
    #endregion

    public IDeleteable<T> GetDeleteable<T>(object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Deleteable<T>();

    public async Task<int> DeleteAllAsync<T>(object splitDbKey = null)
        where T : class, new()
        => await GetDeleteable<T>(splitDbKey).ExecuteCommandAsync();

    /// <summary>
    /// 按条件删除
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="whereExpression"></param>
    /// <param name="splitDbKey"></param>
    /// <returns></returns>
    public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> whereExpression, object splitDbKey = null)
        where T : class, new()
        => await GetDeleteable<T>(splitDbKey).Where(whereExpression).ExecuteCommandAsync();
    #endregion

    #region 插入=>Insert
    public async Task<int> InsertAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Insertable<T>(item).ExecuteCommandAsync();
    public async Task<int> InsertAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetDb<T>(splitDbKey).Insertable<T>(items).ExecuteCommandAsync();
    #endregion

    #region 更新=>Update

    public IUpdateable<T> GetUpdateable<T>(object splitDbKey = null)
        where T : class, new()
        => GetDb<T>(splitDbKey).Updateable<T>();

    public async Task<bool> UpdateAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).UpdateAsync(item);
    public async Task<bool> UpdateAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).UpdateRangeAsync(items);

    public async Task<int> UpdateAsync<T>(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        where T : class, new()
        => await GetDb<T>().Updateable<T>().SetColumns(columns).Where(whereExpression).ExecuteCommandAsync();

    #endregion

    #region 插入或更新=>InsertOrUpdate
    public async Task<bool> InsertOrUpdateAsync<T>(T item, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).InsertOrUpdateAsync(item);
    public async Task<bool> InsertOrUpdateAsync<T>(List<T> items, object splitDbKey = null)
        where T : class, new()
        => await GetRepository<T>(splitDbKey).InsertOrUpdateAsync(items);
    #endregion

    #endregion

    #region Commit & Rollback
    public void Commit()
    {
        if (NeedSubmit())
        {
            _db.CommitTran();
            SubmitCallback(true);
        }
    }
    public async Task CommitAsync()
    {
        if (NeedSubmit())
        {
            await _db.CommitTranAsync();
            SubmitCallback(true);
        }
    }
    public void Rollback()
    {
        if (NeedSubmit())
        {
            _db.RollbackTran();
            SubmitCallback(false);
        }
    }
    public async Task RollbackAsync()
    {
        if (NeedSubmit())
        {
            await _db.RollbackTranAsync();
            SubmitCallback(false);
        }
    }
    private bool NeedSubmit()
    {
        var ret = false;
        GC.SuppressFinalize(this);
        switch (_status)
        {
            case DbTransactionStatus.Init:
                LogUtil.Info(_exception, "DbTransactionManager在Commit或Rollback时已创建但没有使用");
                break;
            case DbTransactionStatus.TranBegined:
                LogUtil.Warning(_exception, "DbTransactionManager在Commit或Rollback时已Begin()但没有调用GetDb使用事务");
                break;
            case DbTransactionStatus.TranUsed:
                ret = true;
                break;
            case DbTransactionStatus.End:
                throw new Exception("DbTransactionManager在Commit或Rollback时已经调用Commit或Rollback提交过事务", _exception);
        }
        _status = DbTransactionStatus.End;
        return ret;
    }
    private void SubmitCallback(bool isCommit)
    {
        int idx = 0;
        try
        {
            if (isCommit)
                _commitCallbacks.ForEach(x => { idx++; x(); });
            else
                _rollbackCallbacks.ForEach(x => { idx++; x(); });
        }
        catch (Exception ex)
        {
            if (ThrowCallbackException)
            {
                throw;
            }
            else
            {
                LogUtil.GetContextLogger()
                    .SetLevel(Microsoft.Extensions.Logging.LogLevel.Warning)
                    .AddField($"DbTransactionManager.Callback.{idx}", ex)
                    .Save();
            }
        }
    }
    #endregion

    ~DbTransactionManager()
    {
        try
        {
            LogUtil.Error(_exception, $"DbTransactionManager析构函数被执行，请Commit或Rollback。status:{_status}");
        }
        catch { }
    }
}
internal enum DbTransactionStatus
{
    /// <summary>
    /// 初始
    /// </summary>
    Init = 0,
    /// <summary>
    /// 事务已开启
    /// </summary>
    TranBegined = 1,
    /// <summary>
    /// 事务已使用
    /// </summary>
    TranUsed = 2,
    /// <summary>
    /// 结束
    /// </summary>
    End = 3
}