using Microsoft.Extensions.Logging;
using System;

namespace AiUo.Logging;

public interface ILogBuilder
{
    /// <summary>
    /// 日志的类别名称
    /// </summary>
    string CategoryName { get; set; }
    /// <summary>
    /// 日志级别
    /// </summary>
    LogLevel Level { get; set; }
    /// <summary>
    /// CustomeException时的日志级别，默认Infomation
    /// </summary>
    LogLevel CustomeExceptionLevel { get; set; }
    /// <summary>
    /// 是否Context日志
    /// </summary>
    bool IsContext { get; }
    /// <summary>
    /// 异常信息(如有多个，记录最后一个)
    /// </summary>
    Exception Exception { get; set; }
    CustomException CustomException { get; set; }
    /// <summary>
    /// 设置日志的类别名称
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    ILogBuilder SetCategoryName(string categoryName);
    /// <summary>
    /// 设置日志级别
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    ILogBuilder SetLevel(LogLevel level);
    /// <summary>
    /// 添加信息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    ILogBuilder AddMessage(string msg);
    /// <summary>
    /// 添加自定义Field
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    ILogBuilder AddField(string field, object value);
    /// <summary>
    /// 添加异常信息(可添加多个)
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    ILogBuilder AddException(Exception ex);
    /// <summary>
    /// 保存日志(上下文日志由RequestLoggingMiddleware统一记录)
    /// </summary>
    void Save();
}