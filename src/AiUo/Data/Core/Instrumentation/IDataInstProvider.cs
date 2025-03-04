using System;

namespace AiUo.Data.Instrumentation;

/// <summary>
/// 定义Database数据访问时的测量事件的接口
/// </summary>
public interface IDataInstProvider
{
    /// <summary>
    /// 执行ConnectionOpened事件
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    void FireConnectionOpenedEvent(string connectionString);

    /// <summary>
    /// 执行ConnectionFailed事件
    /// </summary>
    /// <param name="connectionString">连接字符串</param>
    /// <param name="exception">执行失败时抛出的异常</param>
    void FireConnectionFailedEvent(string connectionString, Exception exception);

    /// <summary>
    /// 执行CommandExecuted事件
    /// </summary>
    /// <param name="command">执行的Command对象</param>
    /// <param name="startTime">执行起始时间</param>
    /// <param name="endTime">执行结束时间</param>
    void FireCommandExecutedEvent(CommandWrapper command, DateTime startTime, DateTime endTime);

    /// <summary>
    /// 执行CommandFailed事件
    /// </summary>
    /// <param name="command">执行的Command对象</param>
    /// <param name="exception">执行失败时抛出的异常</param>
    void FireCommandFailedEvent(CommandWrapper command, Exception exception);

    /// <summary>
    /// 执行TransactionUndisposed事件
    /// </summary>
    /// <param name="exception">执行失败时抛出的异常</param>
    void FireTransactionUndisposedEvent(Exception exception);
}