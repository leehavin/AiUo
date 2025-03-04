using System;
using System.Collections.Generic;
using System.Data.Common;
using AiUo.Logging;

namespace AiUo.Data.Instrumentation;

/// <summary>
/// 数据操作执行事件监测类
/// 除了TransactionUndisposedEvent外其他事件不处理
/// </summary>
public class DefaultDataInstProvider : IDataInstProvider
{
    private DefaultDataInstProvider() { }
    /// <summary>
    /// 单例模式
    /// </summary>
    public static readonly DefaultDataInstProvider Instance = new DefaultDataInstProvider();
    /// <summary>
    /// 执行ConnectionOpened事件
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    public virtual void FireConnectionOpenedEvent(string connectionString)
    { }

    /// <summary>
    /// 执行CommandExecuted事件
    /// </summary>
    /// <param name="command">Command对象</param>
    /// <param name="startTime">执行起始时间</param>
    /// <param name="endTime">执行结束时间</param>
    public virtual void FireCommandExecutedEvent(CommandWrapper command, DateTime startTime, DateTime endTime)
    { }

    /// <summary>
    /// 执行ConnectionFailed事件
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="exception">执行失败时抛出的异常</param>
    public virtual void FireConnectionFailedEvent(string connectionString, Exception exception)
    {
        LogUtil.Error(exception, "无法连接数据库。connectionString:{connectionString}", connectionString);
    }

    /// <summary>
    /// 执行CommandFailed事件
    /// </summary>
    /// <param name="command">执行的Command对象</param>
    /// <param name="exception">执行失败时抛出的异常</param>
    public virtual void FireCommandFailedEvent(CommandWrapper command, Exception exception)
    {
        var list = new List<string>();
        foreach ( DbParameter item in command.Parameters )
            list.Add($"[{item.DbType}] {item.ParameterName} = {item.Value}");
        string msg = "Command对象执行错误。commandText:{commandText} commandParams:{commandParams}";
        LogUtil.Error(exception, msg, command.CommandText,string.Join(Environment.NewLine, list.ToArray()));
    }

    /// <summary>
    /// 执行TransactionUndisposed事件,处理事务没有Commit或Rollback的情况
    /// </summary>
    /// <param name="exception">执行失败时抛出的异常</param>
    public virtual void FireTransactionUndisposedEvent(Exception exception)
    {
        LogUtil.Error(exception, "TransactionManager事务未关闭。");
    }
}