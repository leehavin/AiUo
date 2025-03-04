using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using AiUo.Logging;

namespace AiUo.Data;

/// <summary>
/// DbCommand对象包装类
/// 无法使用构造函数，请通过Database.CreateCommand创建此对象
/// </summary>
public class CommandWrapper : IDisposable
{
    private readonly DbCommand _command;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString
        => _command.Connection?.ConnectionString;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="command">包装的DbCommand对象</param>
    public CommandWrapper(DbCommand command)
        => _command = command;

    /// <summary>
    /// 包装的DbCommand对象
    /// </summary>
    public DbCommand Command => _command;

    /// <summary>
    /// DbConnection对象
    /// </summary>
    public DbConnection Connection { get { return _command.Connection; } set { _command.Connection = value; } }

    /// <summary>
    /// 事务对象
    /// </summary>
    public DbTransaction Transaction { get { return _command.Transaction; } set { _command.Transaction = value; } }

    /// <summary>
    /// 是否存在事务处理
    /// </summary>
    public bool HasTransaction => _command.Transaction != null;

    /// <summary>
    /// 获取DbCommand对象的参数集合
    /// </summary>
    public DbParameterCollection Parameters => _command.Parameters;

    /// <summary>
    /// 获取或设置DbCommand对象的CommandText属性
    /// </summary>
    public string CommandText { get { return _command.CommandText; } set { _command.CommandText = value; } }

    /// <summary>
    /// 获取或设置在终止执行命令的尝试并生成错误之前的等待时间。默认30秒，0表示不限制时间
    /// </summary>
    public int CommandTimeout { get { return _command.CommandTimeout; } set { _command.CommandTimeout = value; } }

    /// <summary>
    /// 获取或设置DbCommand对象的CommandType属性
    /// </summary>
    public CommandType CommandType { get { return _command.CommandType; } set { _command.CommandType = value; } }

    /// <summary>
    /// 执行SQL语句并返回受影响的行数
    /// </summary>
    /// <returns></returns>
    public int ExecuteNonQuery()
        => _command.ExecuteNonQuery();
    public Task<int> ExecuteNonQueryAsync()
        => _command.ExecuteNonQueryAsync();
    public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        => _command.ExecuteNonQueryAsync(cancellationToken);


    /// <summary>
    /// 执行查询语句并返回首行首列数据
    /// 返回null表示数据库没有记录，返回DBNull表示此字段为空
    /// </summary>
    /// <returns></returns>
    public object ExecuteScalar()
        => _command.ExecuteScalar();
    public async Task<object> ExecuteScalarAsync()
        => await _command.ExecuteScalarAsync();
    public async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        => await _command.ExecuteScalarAsync(cancellationToken);

    /// <summary>
    /// 执行SQL语句并返回结果集
    /// </summary>
    /// <returns></returns>
    public DataReaderWrapper ExecuteReader(CommandBehavior behavior = CommandBehavior.Default)
    {
        var reader = _command.ExecuteReader(behavior);
        return new DataReaderWrapper(this, reader);
    }

    public async Task<DataReaderWrapper> ExecuteReaderAsync()
    {
        var reader = await _command.ExecuteReaderAsync();
        return new DataReaderWrapper(this, reader);
    }
    public async Task<DataReaderWrapper> ExecuteReaderAsync(CommandBehavior behavior)
    {
        var reader = await _command.ExecuteReaderAsync(behavior);
        return new DataReaderWrapper(this, reader);
    }
    public async Task<DataReaderWrapper> ExecuteReaderAsync(CancellationToken cancellationToken)
    {
        var reader = await _command.ExecuteReaderAsync(cancellationToken);
        return new DataReaderWrapper(this, reader);
    }
    public async Task<DataReaderWrapper> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        var reader = await _command.ExecuteReaderAsync(behavior, cancellationToken);
        return new DataReaderWrapper(this, reader);
    }

    #region IDisposable
    private bool _isDisposed = false;
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Close()
        => Dispose();
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        GC.SuppressFinalize(this);
        _isDisposed = true;
        if (!HasTransaction && _command.Connection != null) //如果存在事务，交给事务释放资源，此处忽略
            _command.Connection.Close();
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~CommandWrapper()
    {
        if (_command != null && _command.Connection != null && _command.Connection.State != ConnectionState.Closed)
        {
            _command.Connection.Close();
            LogUtil.Error("CommandWrapper对象在析构函数中调用Dispose时连接未关闭。transaction:{transaction} commandText:{commandText}"
                , _command.Transaction != null ? "Transaction对象未Commit或Rollback" : string.Empty
                , _command.CommandText);
        }
    }
    #endregion
}