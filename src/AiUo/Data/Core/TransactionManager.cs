using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using System.Diagnostics;
using AiUo.Data.Instrumentation;

namespace AiUo.Data;

/// <summary>
/// 数据库事务管理类
/// 请显示调用Commit()或Rollback()释放资源
/// </summary>
public class TransactionManager
{
    /// <summary>
    /// 事务级别
    /// ReadUncommitted - 未提交读(存在其他事务未提交的数据造成的脏读)
    /// ReadCommitted - 已提交读(存在其他事务update和delete并提交后造成的不可重复读)
    /// RepeatableRead - 可重复读(存在其他事务insert时造成的幻读)
    /// Serializable - 序列化
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; }

    private List<Action> _commitCallbacks = new List<Action>();
    private List<Action> _rollbackCallbacks = new List<Action>();
    public void AddCommitCallback(Action callback)
        => _commitCallbacks.Add(callback);
    public void AddRollbackCallback(Action callback)
        => _rollbackCallbacks.Add(callback);

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="isolationLevel">事务级别，默认IsolationLevel.ReadCommitted</param>
    public TransactionManager(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        IsolationLevel = isolationLevel;
        var stack = new StackTrace(0, true);
        _exception = new Exception($"TransactionManager对象在析构函数中调用释放，请显示调用Commit()或Rollback()释放资源。StackTrace:{stack.ToString()}");
    }

    // key : ConnectionString
    private readonly ConcurrentDictionary<string, DbTransaction> _trans = new ConcurrentDictionary<string, DbTransaction>(2, 3);
    private bool _isOpened = false;
    private object _sync = new();

    private IDataInstProvider _instrumentationProvider = null;
    private readonly Exception _exception = null;

    /// <summary>
    /// 根据连接字符串获取已存在或创建一个新的数据库事务
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    public DbTransaction GetTransaction(Database database)
    {
        string key = database.ConnectionString;
        if (!_trans.TryGetValue(key, out var ret))
        {
            lock (_sync)
            {
                if (!_trans.TryGetValue(key, out ret))
                {
                    DbConnection conn = database.CreateConnection();
                    database.OpenConnection(conn);
                    ret = conn.BeginTransaction(IsolationLevel);
                    _trans.TryAdd(key, ret);
                    _isOpened = true;
                    //只取一个检测程序
                    if (_instrumentationProvider == null)
                    {
                        _instrumentationProvider = database.InstProvider;
                    }
                }
            }
        }
        return ret;
    }

    private void Process(bool isCommit)
    {
        GC.SuppressFinalize(this);
        if (_isOpened)
        {
            try
            {
                if (_trans.Count == 0)
                    throw new Exception("TransactionManager事物对象已Open，但没有DbTransaction对象。");
                foreach (DbTransaction tran in _trans.Values)
                {
                    if (tran != null && tran.Connection != null)
                    {
                        if (isCommit) tran.Commit(); else tran.Rollback();
                    }
                }
            }
            finally
            {
                CloseConnections();
                if (isCommit)
                    _commitCallbacks.ForEach(x => x());
                else
                    _rollbackCallbacks.ForEach(x => x());
            }
        }
    }
    private void CloseConnections()
    {
        _isOpened = false;
        foreach (DbTransaction tran in _trans.Values)
        {
            tran?.Connection?.Close();
        }
    }
    /// <summary>
    /// 提交事务
    /// </summary>
    public void Commit() => Process(true);

    /// <summary>
    /// 回滚事务
    /// </summary>
    public void Rollback() => Process(false);

    /// <summary>
    /// 析构函数
    /// </summary>
    ~TransactionManager()
    {
        if (!_isOpened) return;
        try
        {
            CloseConnections();
        }
        catch { }

        // 记录没有手动释放资源的错误!!!
        try
        {
            _instrumentationProvider?.FireTransactionUndisposedEvent(_exception);
        }
        catch { }
    }
}