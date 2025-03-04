using System;
using System.Data;

namespace AiUo.Data;

/// <summary>
/// 数据操作DAO基类
/// </summary>
public abstract partial class DaoBase : IDao
{
    /// <summary>
    /// 获取数据库访问对象
    /// </summary>
    public Database Database { get; internal set; }
        
    /// <summary>
    /// 数据提供程序类型
    /// </summary>
    public DbDataProvider Provider => Database.Provider;

    /// <summary>
    /// 获取DbCommand封装对象
    /// </summary>
    public CommandWrapper Command { get; internal set; }

    /// <summary>
    /// 获取或设置在终止执行命令的尝试并生成错误之前的等待时间，单位秒。默认30秒，0表示不限制时间
    /// </summary>
    public int CommandTimeout
    {
        get
        {
            return Database.CommandTimeout;
        }
        set
        {
            Database.CommandTimeout = value;
        }
    }

    /// <summary>
    /// 构造函数
    /// 创建时不设置 Connection ，执行时才设置Connection
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    /// <param name="database"></param>
    public DaoBase(string commandText, CommandType commandType, Database database)
        => Init(commandText, commandType, database);

    /// <summary>
    /// 初始化DAO
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    /// <param name="database"></param>
    protected void Init(string commandText, CommandType commandType, Database database)
    {
        Database = database ?? throw new ArgumentException("Database不能为空。", "database");
        // 创建时不设置Connection
        Command = Database.CreateCommand(commandText, commandType, null, null, false);
    }

    // 执行时才设置Connection，设置事务时会自动打开连接
    private CommandWrapper GetCommand(TransactionManager tm)
    {
        Database.RepairCommandConnection(Command.Command, tm);
        return Command;
    }

    #region IDisposable

    /// <summary>
    /// 释放所使用的所有资源。
    /// </summary>
    public void Dispose()
        => Command.Dispose();

    /// <summary>
    /// 释放所使用的所有资源。
    /// </summary>
    public void Close()
        => Dispose();

    #endregion
}